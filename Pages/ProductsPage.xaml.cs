using System;
using System.Collections.Generic;
using System.Globalization;
using ButchersCashier;
using Microsoft.Maui.Controls;
using ButchersCashier.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using ButchersCashier.Services;
using CashierApps;

namespace CashierApp
{
    public partial class ProductsPage : ContentPage
    {
        private decimal totalAmount = 0;
        private List<Product> products = new List<Product>();
        private Dictionary<string, (decimal TotalPrice, decimal Quantity)> receiptItems = new();
        public event Action<Product> ProductSelected;
        private Dictionary<string, int> itemClickCounts = new();
        private ObservableCollection<ReceiptItem> _receiptItems;
        private readonly IReceiptSaveService _receiptSaveService;
        private readonly ReceiptPrinterService _receiptPrinterService;
        public ObservableCollection<ReceiptItem> ReceiptItems
        {
            get => _receiptItems;
            set
            {
                if (_receiptItems != value)
                {
                    _receiptItems = value;
                    OnPropertyChanged(nameof(ReceiptItems)); // Notify UI of the change

                }
            }
        }

        public ProductsPage(ObservableCollection<ReceiptItem> receiptItems, IReceiptSaveService receiptSaveService)
        {
            InitializeComponent();
            BindingContext = this;

            // Assigning the new receiptItems to the property and notifying the change
            _receiptItems = receiptItems;
            OnPropertyChanged(nameof(ReceiptItems)); // Notify change to the UI
            foreach (var item in ReceiptItems)
            {
                item.PropertyChanged += OnReceiptItemChanged;
            }
            _receiptSaveService = receiptSaveService;
            _receiptPrinterService = new ReceiptPrinterService("CITIZEN NAZWA");
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadProducts();
        }
        private async Task LoadProducts()
        {
            try
            {
                products = await ProductStorage.LoadProductsAsync();
                AddProductButtons(); // Populate the grid after loading products
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Problem z załadowaniem produktów: {ex.Message}", "OK");
            }
        }
        private void OnQuantityTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (Entry)sender;
            string input = e.NewTextValue;

            // Allow the user to clear the entry without it reverting
            if (string.IsNullOrWhiteSpace(input))
            {
                // Set Quantity to zero, or handle as needed for an empty input
                ((ReceiptItem)entry.BindingContext).Quantity = 0;
                return;
            }

            // Normalize decimal separator for consistency
            input = input.Replace(",", ".");

            // Only update Quantity if the input is a valid decimal
            if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal quantity))
            {
                ((ReceiptItem)entry.BindingContext).Quantity = quantity;
            }
            else
            {
                // Revert to the last valid value without updating the binding property
                entry.Text = e.OldTextValue;
            }
        }

        public void DeleteReceiptItem(ReceiptItem item)
        {
            if (ReceiptItems.Contains(item))
            {
                if (itemClickCounts.ContainsKey(item.Name))
                {
                    itemClickCounts[item.Name] = 0;
                }
                ReceiptItems.Remove(item); 
                RefreshTotalAmount(); 
            }
        }
        private void AddProductButtons()
        {
            ProductButtonsGrid.Children.Clear();
            ProductButtonsGrid.RowDefinitions.Clear();

            int row = 0, col = 0;
            int totalColumns = 4; // Number of columns in the grid

            foreach (var product in products)
            {
                if (col == 0) // Start a new row every four products
                {
                    ProductButtonsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }

                // Create a button layout with image and label
                var productLayout = new VerticalStackLayout
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Spacing = 5 // Add spacing between image and label
                };

                // Image for the product
                var productImage = new Image
                {
                    Source = product.ImagePath,
                    WidthRequest = 100, // Increased width
                    HeightRequest = 100, // Increased height
                    HorizontalOptions = LayoutOptions.Center,
                    Aspect = Aspect.AspectFit // Maintain aspect ratio
                };

                // Label for the product name
                var productLabel = new Label
                {
                    Text = product.Name,
                    FontSize = 14, // Increased font size for visibility
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center, // Center vertically
                    Margin = new Thickness(0, 5, 0, 0),
                    TextColor = Colors.Black // Ensure text color is black
                };

                // Add the image and label to the button layout
                productLayout.Children.Add(productImage);
                productLayout.Children.Add(productLabel);

                // Create a Frame for the product
                var productFrame = new Frame
                {
                    BackgroundColor = GetBackgroundColorForQuantityType(product.QuantityType),
                    Content = productLayout,
                    Padding = 10, // Padding around the content
                    CornerRadius = 10,
                    HasShadow = true,
                    WidthRequest = 180, // Increased width for uniformity
                    HeightRequest = 160 // Increased height to accommodate the label
                };

                // Hook up the tap gesture
                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += (s, e) => OnItemClicked(product);
                productFrame.GestureRecognizers.Add(tapGestureRecognizer);

                // Add to the grid
                ProductButtonsGrid.Children.Add(productFrame);

                // Set row and column positions
                Grid.SetColumn(productFrame, col);
                Grid.SetRow(productFrame, row);

                col++;
                if (col >= totalColumns) 
                {
                    col = 0;
                    row++;
                }
            }
        }

        private async Task<decimal> PromptForWeightAsync()
        {
            // Display a prompt with a Cancel button
            string result = await DisplayPromptAsync(
                title: "Wpisz wagę",
                message: "Proszę wpisać wagę (w kilogramach):",
                accept: "OK",
                cancel: "Cancel",
                placeholder: "0,0",
                keyboard: Keyboard.Default);  // Using Default to allow for both comma and dot inputs

            if (result == null)
            {
                // Return 0 or handle as needed to signify canceling
                return 0;
            }

            // Normalize input by replacing any comma with a dot before parsing
            result = result.Replace(",", ".");

            // Validate and return the entered weight if valid
            if (decimal.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal weight) && weight > 0)
            {
                return weight; // Return the entered weight if valid
            }
            else
            {
                await DisplayAlert("Błędne dane", "Wpisz poprawną wagę.", "OK");
                return await PromptForWeightAsync(); // Retry if invalid
            }
        }

        private async void OnItemClicked(Product product)
        {
            // Notify that a product has been selected
            ProductSelected?.Invoke(product);

            // Handle receipt update based on quantity type
            if (product.QuantityType == "Kilograms")
            {
                decimal weight = await PromptForWeightAsync();
                if (weight == 0) return;
                decimal itemPrice = product.Price * weight;

                // Update receipt with weight and total price
                UpdateReceipt(product.Name, itemPrice, weight);
            }
            else if (product.QuantityType == "Items")
            {
                // Increment the click count for this product
                if (!itemClickCounts.ContainsKey(product.Name))
                {
                    itemClickCounts[product.Name] = 0; // Initialize if not present
                }
                itemClickCounts[product.Name]++;

                // Calculate the total price based on the number of clicks
                decimal itemPrice = product.Price * itemClickCounts[product.Name]; // Fixed price per click

                // Update the receipt with the latest total price and quantity (click count)
                UpdateReceipt(product.Name, itemPrice, itemClickCounts[product.Name]);
            }
        }


        private void UpdateReceipt(string itemName, decimal itemPrice, decimal quantity)
        {
            var existingItem = ReceiptItems.FirstOrDefault(item => item.Name == itemName);
            if (existingItem != null)
            {
                existingItem.Quantity = quantity; // Set quantity directly from clicks
                existingItem.UnitPrice = itemPrice / quantity; // Update the unit price if needed
            }
            else
            {
                // Pass 'this' to the ReceiptItem constructor to give it a reference to the current ProductsPage instance
                var newItem = new ReceiptItem(this) // Pass the current ProductsPage instance
                {
                    Name = itemName,
                    Quantity = quantity,
                    UnitPrice = itemPrice / quantity // Fixed item price divided by the number of items
                };
                newItem.PropertyChanged += OnReceiptItemChanged;
                ReceiptItems.Add(newItem);
            }

            RefreshTotalAmount(); // Ensure the total is updated after adding or modifying an item
        }


        private void RefreshTotalAmount()
        {
            totalAmount = ReceiptItems.Sum(item => item.TotalPrice);
            TotalLabel.Text = $"Suma: {totalAmount.ToString("C", CultureInfo.CurrentCulture)}";
        }
        private async Task<string> PromptForNameAsync()
        {
            string result = await DisplayPromptAsync(
                title: "Wpisz imię i nazwisko klienta:",
                message: "Proszę wpisać imię i nazwisko:",
                accept: "OK",
                cancel: "Cancel",
                placeholder: "imię i nazwisko",
                keyboard: Keyboard.Default);

            if (result == null)
            {
                return null;
            }
            if (result != null)
            {
                return result;
            }
            else
            {
                await DisplayAlert("Błędne dane", "Wpisz poprawne imię.", "OK");
                return await PromptForNameAsync();
            }
        }
        private void OnReceiptItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ReceiptItem.TotalPrice))
            {
                RefreshTotalAmount();
            }
        }
        private async Task<bool> RequestStoragePermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            }
            return status == PermissionStatus.Granted;
        }

        private async void OnSaveAndClearReceiptClicked(object sender, EventArgs e)
        {
          
            string customerName = await PromptForNameAsync();

            if (customerName == null)
            {
             
                await DisplayAlert("Anulowano", "Brak imienia, nie można kontynuować.", "OK");
                return;
            }

           
            if (!await RequestStoragePermissionAsync())
            {
                await DisplayAlert("Permission Denied", "Unable to save the receipt without storage permissions.", "OK");
                return;
            }

            await _receiptSaveService.RequestSaveAsync();
            var excelHelper = new ExcelHelper();
            var filePath = await excelHelper.SaveReceiptToExcelAsync(ReceiptItems);
            MessagingCenter.Send(this, "SaveReceiptItems");

            if (filePath == null)
            {
                await DisplayAlert("Info", "Rachunek jest pusty. Nic do zapisania.", "OK");
                return;
            }

            await DisplayAlert("Sukces!", $"Rachunek zapisany w : {filePath}", "OK");

           
            string receiptText = GenerateReceiptText(customerName);

            try
            {
                await _receiptPrinterService.PrintReceiptAsync(receiptText);
                await DisplayAlert("Sukces!", "Rachunek został wydrukowany.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd drukowania", $"Nie udało się wydrukować rachunku: {ex.Message}", "OK");
            }

            ReceiptItems.Clear();
            RefreshTotalAmount();
        }





        private Color GetBackgroundColorForQuantityType(string quantityType)
        {
            return quantityType switch
            {
                "Low" => Colors.Red,
                "Kilograms" => Colors.BurlyWood,
                "Items" => Colors.Green,
                _ => Colors.Gray,
            };
        }
        private string GenerateReceiptText(string customerName)
        {
            var receiptText = new StringBuilder();

            receiptText.AppendLine("---- Rachunek ----");
            receiptText.AppendLine($"Klient: {customerName}");
            receiptText.AppendLine("Produkt\tIlość\tCena");

            foreach (var item in ReceiptItems)
            {
                receiptText.AppendLine($"{item.Name}\t{item.Quantity}\t{item.TotalPrice:C}");
            }

            receiptText.AppendLine("-----------------");
            receiptText.AppendLine($"Suma: {totalAmount:C}");

            return receiptText.ToString();
        }

        private void PrintReceiptAndroid(string receiptText)
        {

        }


    }
}
