using System;
using System.Collections.Generic;
using System.Globalization;
using ButchersCashier;
using Microsoft.Maui.Controls;
using ButchersCashier.Models;

namespace CashierApp
{
    public partial class ProductsPage : ContentPage
    {
        private decimal totalAmount = 0;
        private List<Product> products = new List<Product>();
        private Dictionary<string, (decimal TotalPrice, decimal Quantity)> receiptItems = new();

        public ProductsPage()
        {
            InitializeComponent();
            LoadProducts(); // Load products on initialization
        }

        private async void LoadProducts()
        {
            try
            {
                products = await ProductStorage.LoadProductsAsync();
                AddProductButtons(); // Populate the grid after loading products
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load products: {ex.Message}", "OK");
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
                    VerticalOptions = LayoutOptions.Center
                };

                // Image for the product
                var productImage = new Image
                {
                    Source = product.ImagePath,
                    WidthRequest = 80,
                    HeightRequest = 80,
                    HorizontalOptions = LayoutOptions.Center
                };

                // Label for the product name
                var productLabel = new Label
                {
                    Text = product.Name,
                    FontSize = 12,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.End,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                // Add the image and label to the button layout
                productLayout.Children.Add(productImage);
                productLayout.Children.Add(productLabel);

                // Create a Frame for the product
                var productFrame = new Frame
                {
                    BackgroundColor = GetBackgroundColorForQuantityType(product.QuantityType),
                    Content = productLayout,
                    Padding = 0,
                    CornerRadius = 10,
                    HasShadow = true
                };

                // Hook up the tap gesture
                var tapGestureRecognizer = new TapGestureRecognizer();
                // Pass the product to OnItemClicked
                tapGestureRecognizer.Tapped += (s, e) => OnItemClicked(product);
                productFrame.GestureRecognizers.Add(tapGestureRecognizer);

                // Add to the grid
                ProductButtonsGrid.Children.Add(productFrame);

                // Set row and column positions
                Grid.SetColumn(productFrame, col);
                Grid.SetRow(productFrame, row);

                col++;
                if (col >= totalColumns) // Move to the next row after filling the columns
                {
                    col = 0;
                    row++;
                }
            }
        }

        private void OnItemClicked(Product product)
        {
            if (product.QuantityType == "Kilograms")
            {
                decimal weight = GetWeightFromScale(); // Get weight from scale
                decimal itemPrice = product.Price * weight;

                // Update receipt with weight and total price
                UpdateReceipt(product.Name, itemPrice, weight);
            }
            else if (product.QuantityType == "Items")
            {
                decimal itemPrice = product.Price;
                // Update receipt with count of clicks
                UpdateReceipt(product.Name, itemPrice, 1); // Each click counts as one item
            }
        }

        private void UpdateReceipt(string itemName, decimal itemPrice, decimal quantity)
        {
            // Update the total price and quantity for the product
            if (receiptItems.ContainsKey(itemName))
            {
                var existingItem = receiptItems[itemName];
                existingItem.TotalPrice += itemPrice;
                existingItem.Quantity += quantity;
                receiptItems[itemName] = existingItem;
            }
            else
            {
                receiptItems[itemName] = (itemPrice, quantity);
            }

            // Refresh the receipt display
            RefreshReceipt();
        }

        private void RefreshReceipt()
        {
            ReceiptList.Children.Clear();
            totalAmount = 0;

            foreach (var item in receiptItems)
            {
                string itemName = item.Key;
                var (totalPrice, quantity) = item.Value;

                Label receiptItem = new Label
                {
                    Text = $"{itemName}: {quantity} @ {totalPrice.ToString("C", CultureInfo.CurrentCulture)}",
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Start
                };

                ReceiptList.Children.Add(receiptItem);
                totalAmount += totalPrice;
            }

            TotalLabel.Text = $"Total: {totalAmount.ToString("C", CultureInfo.CurrentCulture)}";
        }

        private decimal GetWeightFromScale()
        {
            // Placeholder for getting weight from an electronic scale
            // Replace this with actual code to interface with the scale
            return 1.0m; // Example weight
        }

        private Color GetBackgroundColorForQuantityType(string quantityType)
        {
            return quantityType switch
            {
                "Low" => Colors.Red,
                "Medium" => Colors.Yellow,
                "High" => Colors.Green,
                _ => Colors.Gray,
            };
        }
    }
}
