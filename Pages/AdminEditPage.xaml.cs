using System;
using System.Linq;
using ButchersCashier;
using Microsoft.Maui.Controls;
using ButchersCashier.Models;

namespace CashierApp
{
    public partial class AdminEditPage : ContentPage
    {
        private Product _product;

        public AdminEditPage(Product product)
        {
            InitializeComponent();
            _product = product;
            LoadProductData();
        }

        private void LoadProductData()
        {
            ProductNameEntry.Text = _product.Name;
            ProductCategoryEntry.Text = _product.Category;
            QuantityTypePicker.SelectedItem = _product.QuantityType;
            ProductImagePathEntry.Text = _product.ImagePath;
            ProductPriceEntry.Text = _product.Price.ToString("F2");
        }

        private async void OnSaveChangesClicked(object sender, EventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(ProductNameEntry.Text) ||
                string.IsNullOrWhiteSpace(ProductCategoryEntry.Text) ||
                QuantityTypePicker.SelectedItem == null ||
                string.IsNullOrWhiteSpace(ProductImagePathEntry.Text) ||
                !decimal.TryParse(ProductPriceEntry.Text, out decimal price))
            {
                ResultLabel.Text = "Proszę uzupełnić wszystkie pola.";
                return;
            }

            // Update product details
            _product.Name = ProductNameEntry.Text;
            _product.Category = ProductCategoryEntry.Text;
            _product.QuantityType = QuantityTypePicker.SelectedItem.ToString();
            _product.ImagePath = ProductImagePathEntry.Text;
            _product.Price = price;

            // Load existing products, update, and save
            var products = await ProductStorage.LoadProductsAsync();
            var existingProduct = products.FirstOrDefault(p => p.Id == _product.Id); // Find by unique ID
            if (existingProduct != null)
            {
                // Update the existing product
                existingProduct.Name = _product.Name;
                existingProduct.Category = _product.Category;
                existingProduct.QuantityType = _product.QuantityType;
                existingProduct.ImagePath = _product.ImagePath;
                existingProduct.Price = _product.Price;
            }
            else
            {
                // Optionally, add as new if it doesn't exist
                products.Add(_product);
            }
            await ProductStorage.SaveProductsAsync(products);

            ResultLabel.Text = "Produkt zaktualizowany!";
        }
    }
}
