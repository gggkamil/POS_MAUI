using ButchersCashier.Models;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ButchersCashier;

namespace CashierApp
{
    public partial class AdminPage : ContentPage
    {
        public AdminPage()
        {
            InitializeComponent();
        }

        private async void OnAddProductClicked(object sender, EventArgs e)
        {
            // Read input values
            string name = ProductNameEntry.Text;
            string category = ProductCategoryEntry.Text;
            string quantityType = QuantityTypePicker.SelectedItem?.ToString();
            string imagePath = ProductImagePathEntry.Text;
            string priceText = ProductPriceEntry.Text;

            // Validate inputs
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(category) || string.IsNullOrEmpty(quantityType) || string.IsNullOrEmpty(imagePath) || string.IsNullOrWhiteSpace(priceText))
            {
                ResultLabel.Text = "Please fill all fields.";
                ResultLabel.TextColor = Colors.Red; // Change text color to red for error
                return;
            }
            if (!decimal.TryParse(priceText, out decimal price) || price < 0)
            {
                ResultLabel.Text = "Please enter a valid price.";
                return;
            }
            List<Product> existingProducts = new List<Product>();

            try
            {
                // Load existing products
                existingProducts = await ProductStorage.LoadProductsAsync();

                // Create a new product
                Product newProduct = new Product
                {
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

                ResultLabel.Text = "Product added successfully!";
                ResultLabel.TextColor = Colors.Green; // Change text color to green for success
                ClearInputFields();
            }
            catch (Exception ex)
            {
                ResultLabel.Text = $"Error: {ex.Message}";
                ResultLabel.TextColor = Colors.Red; // Change text color to red for error
            }
        }

        private void ClearInputFields()
        {
            ProductNameEntry.Text = string.Empty;
            ProductCategoryEntry.Text = string.Empty;
            ProductImagePathEntry.Text = string.Empty;
            QuantityTypePicker.SelectedItem = null;
            ProductPriceEntry.Text = string.Empty;// Clear the selected item in the Picker
        }
    }
}
