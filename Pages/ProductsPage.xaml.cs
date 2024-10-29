﻿using System;
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
        public event Action<Product> ProductSelected;
        public ProductsPage()
        {
            InitializeComponent();
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
        private async Task<decimal> PromptForWeightAsync()
        {
            // Display a prompt with a Cancel button
            string result = await DisplayPromptAsync(
                title: "Wpisz wagę",
                message: "Proszę wpisać wagę (w kilogramach):",
                accept: "OK",
                cancel: "Cancel",
                placeholder: "0.0",
                keyboard: Keyboard.Numeric);

            if (result == null)
            {
                // Return 0 or handle as needed to signify canceling
                return 0;
            }

            // Replace dot with comma for decimal separator
            result = result.Replace(".", ",");

            // Validate and return the entered weight if valid
            if (decimal.TryParse(result, out decimal weight) && weight > 0)
            {
                return weight; // Return the entered weight if valid
            }
            else
            {
                await DisplayAlert("Błędne dane", "Wpisz poprawną wagę.", "OK");
                return await PromptForWeightAsync(); // Retry if invalid
            }
        }
        private async  void OnItemClicked(Product product)
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
                decimal itemPrice = product.Price;
                // Update receipt with count of clicks (each click counts as one item)
                UpdateReceipt(product.Name, itemPrice, 1);
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
                    Text = $"{itemName}: {quantity} | {totalPrice.ToString("C", CultureInfo.CurrentCulture)}",
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Start
                };

                ReceiptList.Children.Add(receiptItem);
                totalAmount += totalPrice;
            }

            TotalLabel.Text = $"Suma: {totalAmount.ToString("C", CultureInfo.CurrentCulture)}";
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
                "Kilograms" => Colors.BurlyWood,
                "Items" => Colors.Green,
                _ => Colors.Gray,
            };
        }

    }
}
