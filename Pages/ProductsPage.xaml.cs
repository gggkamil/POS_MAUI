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
                foreach (var product in products)
                {
                    AddProductButton(product);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load products: {ex.Message}", "OK");
            }
        }

        private void AddProductButton(Product product)
        {
            Button productButton = new Button
            {
                Text = product.Name,
                ImageSource = product.ImagePath,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = 100,
                HeightRequest = 100
            };
            productButton.Clicked += OnItemClicked;
            ProductButtonsStack.Children.Add(productButton);
        }

        private void OnItemClicked(object sender, EventArgs e)
        {
            if (sender is Button clickedButton)
            {
                string itemName = clickedButton.Text;
                decimal itemPrice = GetItemPrice(itemName);

                AddToReceipt(itemName, itemPrice);
                totalAmount += itemPrice;
                TotalLabel.Text = $"Total: {totalAmount.ToString("C", CultureInfo.CurrentCulture)}";
            }
        }

        private decimal GetItemPrice(string itemName)
        {
            return itemName switch
            {
                "Apple" => 0.99m,
                "Banana" => 0.49m,
                "Orange" => 0.79m,
                "Watermelon" => 3.00m,
                _ => 0
            };
        }

        private void AddToReceipt(string itemName, decimal itemPrice)
        {
            Label receiptItem = new Label
            {
                Text = $"{itemName}: {itemPrice.ToString("C", CultureInfo.CurrentCulture)}",
                FontSize = 16,
                HorizontalOptions = LayoutOptions.Start
            };
            ReceiptList.Children.Add(receiptItem);
        }
    }
}
