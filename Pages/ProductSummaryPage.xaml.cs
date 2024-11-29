using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Maui.Controls;

namespace CashierApp
{
    public partial class ProductSummaryPage : ContentPage
    {
        private string folderPath = @"C:\Kasa\Zamówienia";
        private string fileName = "wydawka_uaktualniona_tylko.xlsx";
        private string filePath;

        public ProductSummaryPage()
        {
            InitializeComponent();
            filePath = Path.Combine(folderPath, fileName);
            LoadProductSummary();
        }
        public void RefreshOrders()
        {
           ProductSummaryGrid.Clear();
            LoadProductSummary(); 
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshOrders();
        }
        private void LoadProductSummary()
        {

            var productSummary = new Dictionary<string, decimal>();

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0];

                    if (worksheet.Dimension == null)
                    {
                        DisplayAlert("Info", "No data found in the worksheet.", "OK");
                        return;
                    }

                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    for (int row = 2; row <= rowCount; row++) // Skip header row
                    {
                        for (int col = 3; col <= colCount; col++) // Start from column 3 for products
                        {
                            string productName = worksheet.Cells[1, col].Text; // Product name from header
                            string cellValue = worksheet.Cells[row, col].Text; // Quantity and superscript

                            decimal quantity = 0;
                            string superscript = null;

                            if (!string.IsNullOrEmpty(cellValue))
                            {
                                var numericPart = new string(cellValue.TakeWhile(char.IsDigit).ToArray());
                                var superscriptPart = new string(cellValue.SkipWhile(char.IsDigit).ToArray());

                                if (decimal.TryParse(numericPart, out var parsedQuantity))
                                {
                                    quantity = parsedQuantity;
                                }

                                if (!string.IsNullOrEmpty(superscriptPart))
                                {
                                    superscript = ConvertFromSuperscript(superscriptPart);
                                }
                            }


                            // Combine product and superscript to create a unique key
                            string productKey = $"{productName}^{superscript ?? "No Superscript"}";
                            if (productSummary.ContainsKey(productKey))
                            {
                                productSummary[productKey] += quantity;
                            }
                            else
                            {
                                productSummary[productKey] = quantity;
                            }
                        }
                    }
                }

                DisplaySummaryInGrid(productSummary);
                if (productSummary.Count == 0)
                {
                    DisplayAlert("Info", "No products found to display.", "OK");
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"Error reading file: {ex.Message}", "OK");
            }
        }

        private void DisplaySummaryInGrid(Dictionary<string, decimal> productSummary)
        {
            // Clear existing rows and children to avoid duplication
            ProductSummaryGrid.RowDefinitions.Clear();
            ProductSummaryGrid.Children.Clear();

            // Add header row
            ProductSummaryGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var headerProductLabel = new Label
            {
                Text = "Product",
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center
            };
            ProductSummaryGrid.Children.Add(headerProductLabel);
            Grid.SetRow(headerProductLabel, 0);
            Grid.SetColumn(headerProductLabel, 0);

            var headerQuantityLabel = new Label
            {
                Text = "Quantity",
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center
            };
            ProductSummaryGrid.Children.Add(headerQuantityLabel);
            Grid.SetRow(headerQuantityLabel, 0);
            Grid.SetColumn(headerQuantityLabel, 1);

            int rowIndex = 1; // Start from row 1 for product data

            foreach (var product in productSummary)
            {
                // Create a new row for the product
                ProductSummaryGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Add product name label
                var productNameLabel = new Label
                {
                    Text = product.Key,
                    HorizontalOptions = LayoutOptions.Start,
                    FontSize = 14
                };
                ProductSummaryGrid.Children.Add(productNameLabel);
                Grid.SetRow(productNameLabel, rowIndex); // STATIC CALL
                Grid.SetColumn(productNameLabel, 0);     // STATIC CALL

                // Add quantity label
                var quantityLabel = new Label
                {
                    Text = product.Value.ToString(),
                    HorizontalOptions = LayoutOptions.Center,
                    FontSize = 14
                };
                ProductSummaryGrid.Children.Add(quantityLabel);
                Grid.SetRow(quantityLabel, rowIndex);    // STATIC CALL
                Grid.SetColumn(quantityLabel, 1);       // STATIC CALL

                rowIndex++; // Move to the next row
            }
        }




        private string ConvertFromSuperscript(string text)
        {
            var superscriptMap = new Dictionary<char, char>
            {
                { '⁰', '0' }, { '¹', '1' }, { '²', '2' }, { '³', '3' },
                { '⁴', '4' }, { '⁵', '5' }, { '⁶', '6' }, { '⁷', '7' },
                { '⁸', '8' }, { '⁹', '9' }, { 'ᵃ', 'a' }, { 'ᵇ', 'b' },
                { 'ᶜ', 'c' }, { 'ᵈ', 'd' }, { 'ᵉ', 'e' }, { 'ᶠ', 'f' },
                { 'ᶢ', 'g' }, { 'ʰ', 'h' }, { 'ⁱ', 'i' }, { 'ʲ', 'j' },
                { 'ᵏ', 'k' }, { 'ˡ', 'l' }, { 'ᵐ', 'm' }, { 'ⁿ', 'n' },
                { 'ᵒ', 'o' }, { 'ᵖ', 'p' }, { 'ʳ', 'r' }, { 'ˢ', 's' },
                { 'ᵗ', 't' }, { 'ᵘ', 'u' }, { 'ᵛ', 'v' }, { 'ʷ', 'w' },
                { 'ˣ', 'x' }, { 'ʸ', 'y' }, { 'ᶻ', 'z' }, { '‧', '.' }
            };

            return string.Concat(text.Select(c => superscriptMap.ContainsKey(c) ? superscriptMap[c] : c));
        }
    }
}
