﻿using ButchersCashier.Models;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ButchersCashier;

namespace CashierApp
{
    public partial class AdminPage : ContentPage
    {
        private const string PrinterNameKey = "PrinterName";
        private List<Product> products;
        private TabbedPage parentTabbedPage;
        public AdminPage(TabbedPage parent)
        {

            InitializeComponent();
            parentTabbedPage = parent;
            PrinterNameEntry.Text = Preferences.Get(PrinterNameKey, "Citizen CT-S2000");
            LoadProducts();
        }

        private async void LoadProducts()
        {
            products = await ProductStorage.LoadProductsAsync();
            ProductCollectionView.ItemsSource = products; // Bind products to the CollectionView
        }
        private void OnSavePrinterNameClicked(object sender, EventArgs e)
        {
            // Save the printer name to preferences
            Preferences.Set(PrinterNameKey, PrinterNameEntry.Text);
            DisplayAlert("Sukces", "Nazwa drukarki została zapisana.", "OK");
        }
        private async void OnAddProductClicked(object sender, EventArgs e)
        {
            // Read input values
            string name = ProductNameEntry.Text;
            string category = ProductCategoryEntry.Text;
            string quantityType = QuantityTypePicker.SelectedItem?.ToString();
            string imagePath = ProductImagePathEntry.Text;
            string priceText = ProductPriceEntry.Text;

            priceText = priceText.Replace('.', ',');
            // Validate inputs
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(category) || string.IsNullOrEmpty(quantityType) || string.IsNullOrEmpty(imagePath) || string.IsNullOrWhiteSpace(priceText))
            {
                ResultLabel.Text = "Proszę uzupełnić wszytkie pola.";
                ResultLabel.TextColor = Colors.Red; // Change text color to red for error
                return;
            }

            if (!decimal.TryParse(priceText, out decimal price) || price < 0)
            {
                ResultLabel.Text = "Proszę wpisać poprawną cenę.";
                ResultLabel.TextColor = Colors.Red; // Change text color to red for error
                return;
            }

            try
            {
                // Load existing products
                var existingProducts = await ProductStorage.LoadProductsAsync();
                int newId = existingProducts
                    .Where(p => p.Id != null)
                    .Select(p => int.TryParse(p.Id, out int id) ? id : 0)
                    .DefaultIfEmpty(0)
                    .Max() + 1;
                // Create a new product
                Product newProduct = new Product
                {
                    Id = newId.ToString(),
                    Name = name,
                    Category = category,
                    ImagePath = imagePath,
                    QuantityType = quantityType,
                    Price = price
                };

                // Add the new product to the existing list
                existingProducts.Add(newProduct);

                // Save the updated list of products
                await ProductStorage.SaveProductsAsync(existingProducts);

                ResultLabel.Text = "Produkt dodany poprawnie!";
                ResultLabel.TextColor = Colors.Green; // Change text color to green for success
                ClearInputFields();
                LoadProducts(); // Refresh the product list after adding
            }
            catch (Exception ex)
            {
                ResultLabel.Text = $"Błąd: {ex.Message}";
                ResultLabel.TextColor = Colors.Red; // Change text color to red for error
            }
        }

        private async void OnEditButtonClicked(object sender, EventArgs e)
        {
            // Get the product to edit
            var button = sender as Button;
            var product = button?.CommandParameter as Product; // Get the selected product

            if (product != null)
            {
                // Navigate to the AdminEditPage with the selected product
                AddAdminEditTab(product);
            }
        }
        private void AddAdminEditTab(Product product)
        {
            // Check if a tab for this product is already open
            var existingTab = parentTabbedPage.Children
                .OfType<NavigationPage>()
                .FirstOrDefault(p => p.CurrentPage is AdminEditPage adminPage && adminPage.ProductName == product.Name);

            if (existingTab != null)
            {
                // If the tab is already open, bring it to the front
                parentTabbedPage.CurrentPage = existingTab;
                return; // Exit to prevent creating a new tab
            }

            // If no existing tab is found, create a new one
            var adminEditPage = new AdminEditPage(product);
            var navPage = new NavigationPage(adminEditPage)
            {
                Title = $"Aktualizuj {product.Name}"
            };

            parentTabbedPage.Children.Add(navPage);
        }
        private async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var productToDelete = button?.CommandParameter as Product;

            if (productToDelete != null)
            {
                // Confirm deletion
                bool confirm = await DisplayAlert("Usuń", $"Naprawdę chcesz usunąć {productToDelete.Name}?", "Tak", "Nie");
                if (confirm)
                {
                    // Remove the product from the list
                    products.Remove(productToDelete);

                    // Save the updated list
                    await ProductStorage.SaveProductsAsync(products);

                    // Refresh the product list
                    LoadProducts();

                    // Show success message
                    ResultLabel.Text = $"{productToDelete.Name} został usunięty.";
                    ResultLabel.TextColor = Colors.Green; // Change text color to green for success
                }
            }
        }

        private void ClearInputFields()
        {
            ProductNameEntry.Text = string.Empty;
            ProductCategoryEntry.Text = string.Empty;
            ProductImagePathEntry.Text = string.Empty;
            QuantityTypePicker.SelectedItem = null;
            ProductPriceEntry.Text = string.Empty; 
        }
    }
}
