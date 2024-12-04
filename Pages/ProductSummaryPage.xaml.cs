using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Maui.Controls;

namespace CashierApp
{
    public partial class ProductSummaryPage : ContentPage
    {
        private string folderPath = @"C:\Kasa\Zamówienia";
        private string fileName = "wydawka_uaktualniona_tylko.xlsx";
        private string conversionFileName = "przelicznik.xlsx";
        private string filePath;
        private string conversionFilePath;
        private Dictionary<string, decimal> productSummary = new Dictionary<string, decimal>();
        private Dictionary<string, decimal> meatRequirements = new Dictionary<string, decimal>();

        public ProductSummaryPage()
        {
            InitializeComponent();
            filePath = Path.Combine(folderPath, fileName);
            conversionFilePath = Path.Combine(folderPath, conversionFileName);
            LoadProductSummary();
        }

        public void RefreshOrders()
        {
            ProductSummaryGrid.Clear();
            MeatRequirementsGrid.Clear(); // Ensure meat grid is also cleared
            LoadProductSummary();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshOrders();
        }

        private void LoadProductSummary()
        {
            productSummary.Clear(); // Ensure the dictionary is cleared before loading new data
            var conversionFactors = LoadConversionFactors(conversionFilePath); // Load conversion factors

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

                // Calculate total meat requirements
                meatRequirements = CalculateMeatRequirements(productSummary, conversionFactors);

                // Display in grids
                DisplaySummaryInGrid(productSummary);
                DisplayMeatRequirementsInGrid(meatRequirements);

                if (productSummary.Count == 0)
                {
                    DisplayAlert("Info", "No products found to display.", "OK");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"Error reading file: {ex.Message}", "OK");
            }
        }


        private Dictionary<string, Dictionary<string, decimal>> LoadConversionFactors(string conversionFilePath)
        {
            var conversionFactors = new Dictionary<string, Dictionary<string, decimal>>();

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(new FileInfo(conversionFilePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0];

                    if (worksheet.Dimension == null) throw new Exception("Conversion data not found.");

                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    for (int row = 2; row <= rowCount; row++) // Start from row 2 (meats row)
                    {
                        string meatType = worksheet.Cells[row, 1].Text; // First column: Meat type
                        conversionFactors[meatType] = new Dictionary<string, decimal>();

                        for (int col = 2; col <= colCount; col++) // Start from column 2 for products
                        {
                            string productName = worksheet.Cells[1, col].Text; // Header: Product name
                            if (decimal.TryParse(worksheet.Cells[row, col].Text, out var factor))
                            {
                                conversionFactors[meatType][productName] = factor;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"Error reading conversion file: {ex.Message}", "OK");
            }

            return conversionFactors;
        }

        private Dictionary<string, decimal> CalculateMeatRequirements(Dictionary<string, decimal> productSummary, Dictionary<string, Dictionary<string, decimal>> conversionFactors)
        {
            var meatRequirements = new Dictionary<string, decimal>();

            foreach (var product in productSummary)
            {
                string productName = product.Key.Split('^')[0]; // Extract product name
                decimal productQuantity = product.Value;

                foreach (var meat in conversionFactors)
                {
                    string meatType = meat.Key;

                    if (meat.Value.ContainsKey(productName))
                    {
                        decimal factor = meat.Value[productName];
                        decimal requirement = productQuantity * factor;

                        if (meatRequirements.ContainsKey(meatType))
                        {
                            meatRequirements[meatType] += requirement;
                        }
                        else
                        {
                            meatRequirements[meatType] = requirement;
                        }
                    }
                }
            }

            return meatRequirements;
        }

        private void DisplayMeatRequirementsInGrid(Dictionary<string, decimal> meatRequirements)
        {
            MeatRequirementsGrid.RowDefinitions.Clear();
            MeatRequirementsGrid.Children.Clear();

            // Add header row
            MeatRequirementsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var headerMeatLabel = new Label
            {
                Text = "Meat",
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            };
            MeatRequirementsGrid.Children.Add(headerMeatLabel);
            Grid.SetRow(headerMeatLabel, 0);
            Grid.SetColumn(headerMeatLabel, 0);

            var headerRequirementLabel = new Label
            {
                Text = "Requirement",
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            };
            MeatRequirementsGrid.Children.Add(headerRequirementLabel);
            Grid.SetRow(headerRequirementLabel, 0);
            Grid.SetColumn(headerRequirementLabel, 1);

            int rowIndex = 1;
            foreach (var meat in meatRequirements)
            {
                MeatRequirementsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var meatLabel = new Label
                {
                    Text = meat.Key,
                    HorizontalOptions = LayoutOptions.Start,
                    TextColor = Colors.Black,
                    FontSize = 14
                };
                MeatRequirementsGrid.Children.Add(meatLabel);
                Grid.SetRow(meatLabel, rowIndex);
                Grid.SetColumn(meatLabel, 0);

                var requirementLabel = new Label
                {
                    Text = meat.Value.ToString("F3"),
                    HorizontalOptions = LayoutOptions.Center,
                    TextColor = Colors.Black,
                    FontSize = 14
                };
                MeatRequirementsGrid.Children.Add(requirementLabel);
                Grid.SetRow(requirementLabel, rowIndex);
                Grid.SetColumn(requirementLabel, 1);

                rowIndex++;
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
                { 'ˣ', 'x' }, { 'ʸ', 'y' }, { 'ᶻ', 'z' }
            };

            return new string(text.Select(c => superscriptMap.ContainsKey(c) ? superscriptMap[c] : c).ToArray());
        }

        private void DisplaySummaryInGrid(Dictionary<string, decimal> productSummary)
        {
            ProductSummaryGrid.RowDefinitions.Clear();
            ProductSummaryGrid.Children.Clear();

            // Add header row
            ProductSummaryGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var headerProductLabel = new Label
            {
                Text = "Product",
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            };
            ProductSummaryGrid.Children.Add(headerProductLabel);
            Grid.SetRow(headerProductLabel, 0);
            Grid.SetColumn(headerProductLabel, 0);

            var headerQuantityLabel = new Label
            {
                Text = "Quantity",
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            };
            ProductSummaryGrid.Children.Add(headerQuantityLabel);
            Grid.SetRow(headerQuantityLabel, 0);
            Grid.SetColumn(headerQuantityLabel, 1);

            int rowIndex = 1;
            foreach (var product in productSummary)
            {
                ProductSummaryGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var productLabel = new Label
                {
                    Text = product.Key,
                    HorizontalOptions = LayoutOptions.Start,
                    TextColor = Colors.Black,
                    FontSize = 14
                };
                ProductSummaryGrid.Children.Add(productLabel);
                Grid.SetRow(productLabel, rowIndex);
                Grid.SetColumn(productLabel, 0);

                var quantityLabel = new Label
                {
                    Text = product.Value.ToString("F3"),
                    HorizontalOptions = LayoutOptions.Center,
                    TextColor = Colors.Black,
                    FontSize = 14
                };
                ProductSummaryGrid.Children.Add(quantityLabel);
                Grid.SetRow(quantityLabel, rowIndex);
                Grid.SetColumn(quantityLabel, 1);

                rowIndex++;
            }
        }
        private void OnSaveSummaryClicked(object sender, EventArgs e)
        {
            // Get the current product summary and meat requirements data
            var productSummary = GetProductSummaryData(); // Replace with your method to retrieve product summary data
            var meatRequirements = GetMeatRequirementsData(); // Replace with your method to retrieve meat requirements data

            // Call the method to save the summary to Excel
            SaveSummaryToExcel(productSummary, meatRequirements);
        }
        private void SaveSummaryToExcel(Dictionary<string, decimal> productSummary, Dictionary<string, decimal> meatRequirements)
        {
            try
            {
                // Generate the file name with the current date (e.g., "Summary20241204.xlsx")
                string currentDate = DateTime.Now.ToString("yyyyMMdd");
                string newFilePath = Path.Combine(folderPath, $"Summary{currentDate}.xlsx");

                // Check if the directory exists, create if not
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                using (var package = new ExcelPackage())
                {
                    // Create a worksheet for Product Summary
                    var productSheet = package.Workbook.Worksheets.Add("Product Summary");

                    // Create headers for the product summary
                    productSheet.Cells[1, 1].Value = "Product";
                    productSheet.Cells[1, 2].Value = "Quantity";

                    // Add actual product data
                    int row = 2;
                    foreach (var product in productSummary)
                    {
                        productSheet.Cells[row, 1].Value = product.Key;  // Product Name (key)
                        productSheet.Cells[row, 2].Value = product.Value; // Quantity
                        row++;
                    }

                    // Create a worksheet for Meat Requirements
                    var meatSheet = package.Workbook.Worksheets.Add("Meat Requirements");

                    // Create headers for the meat requirements
                    meatSheet.Cells[1, 1].Value = "Meat Type";
                    meatSheet.Cells[1, 2].Value = "Requirement";

                    // Add actual meat requirement data
                    row = 2;
                    foreach (var meat in meatRequirements)
                    {
                        meatSheet.Cells[row, 1].Value = meat.Key;  // Meat Type (key)
                        meatSheet.Cells[row, 2].Value = meat.Value; // Requirement
                        row++;
                    }

                    // Save the file to the specified path
                    FileInfo fileInfo = new FileInfo(newFilePath);
                    package.SaveAs(fileInfo);

                    // Display a success message
                    DisplayAlert("Success", $"Summary has been saved to: {newFilePath}", "OK");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"Error saving file: {ex.Message}", "OK");
            }
        }

        private Dictionary<string, decimal> GetProductSummaryData()
        {
            // Return the actual product summary data stored in the class-level variable
            return productSummary;
        }
        private Dictionary<string, decimal> GetMeatRequirementsData()
        {
            // Return the actual meat requirements data stored in the class-level variable
            return meatRequirements;
        }
    }
}
