using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ButchersCashier;
using ButchersCashier.Models;

namespace CashierApp
{
    public partial class OrdersPage : ContentPage
    {
        private string folderPath = @"C:\Kasa\Zamówienia";
        private string fileName = "wydawka_uaktualniona_tylko.xlsx";
        private string filePath;

        public ObservableCollection<OrderRow> Orders { get; set; } = new();
        private List<Product> Products { get; set; } = new();

        public OrdersPage()
        {
            InitializeComponent();
            filePath = Path.Combine(folderPath, fileName);
            BindingContext = this;

            LoadDataAsync();
        }

        // Load data from the JSON product file and Excel orders file
        private async Task LoadDataAsync()
        {
            Products = await ProductStorage.LoadProductsAsync(); // Load products from JSON
            LoadOrdersFromExcel();
        }

        // Load orders from the Excel file
        private async void LoadOrdersFromExcel()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    if (worksheet.Dimension == null)
                    {
                        await DisplayAlert("Info", "The Excel sheet is empty or has no data.", "OK");
                        return;
                    }

                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++) // Starting from row 2 to skip headers
                    {
                        // Read the Order ID from the "LP." column (assuming it's column 1)
                        string orderIdText = worksheet.Cells[row, 1].Text;
                        if (string.IsNullOrWhiteSpace(orderIdText) || !int.TryParse(orderIdText, out int orderId))
                        {
                            continue; // Skip rows without a valid Order ID
                        }

                        // Read the Customer Name from the "Imię i nazwisko" column (assuming it's column 2)
                        string customerName = worksheet.Cells[row, 2].Text;
                        if (string.IsNullOrWhiteSpace(customerName))
                        {
                            continue; // Skip rows without a Customer Name
                        }

                        var productQuantities = new ObservableCollection<ProductQuantity>();
                        bool hasNonZeroQuantity = false;

                        // Assuming product quantities start from column 3 onward in the Excel sheet
                        for (int col = 3; col <= worksheet.Dimension.Columns; col++)
                        {
                            string productQuantityText = worksheet.Cells[row, col].Text;
                            decimal quantity = decimal.TryParse(productQuantityText, out decimal q) ? q : 0;

                            if (col - 3 < Products.Count) // Ensure we’re within Products list bounds
                            {
                                var product = Products[col - 3];
                                productQuantities.Add(new ProductQuantity { ProductName = product.Name, Quantity = quantity });

                                if (quantity > 0)
                                {
                                    hasNonZeroQuantity = true;
                                }
                            }
                        }

                        // Add the order if it has any non-zero product quantities
                        if (hasNonZeroQuantity)
                        {
                            Orders.Add(new OrderRow
                            {
                                OrderId = orderId,
                                CustomerName = customerName,
                                ProductQuantities = productQuantities
                            });
                        }
                    }

                    DisplayOrderCards(); // Method to display the orders in card format
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not load orders: {ex.Message}", "OK");
            }
        }



        // Display each order as a card in the UI
        private void DisplayOrderCards()
        {
            OrdersContainer.Children.Clear();

            foreach (var order in Orders)
            {
                var card = new Frame
                {
                    BorderColor = Colors.Gray,
                    Padding = 10,
                    Margin = 5,
                    CornerRadius = 5,
                    BackgroundColor = Colors.LightGray
                };

                var stackLayout = new StackLayout { Spacing = 5 };

                // Customer Name
                stackLayout.Children.Add(new Label
                {
                    Text = $"Customer: {order.CustomerName}",
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 16
                });

                // Product Quantities
                foreach (var productQuantity in order.ProductQuantities)
                {
                    stackLayout.Children.Add(new Label
                    {
                        Text = $"{productQuantity.ProductName}: {productQuantity.Quantity}",
                        FontSize = 14
                    });
                }

                card.Content = stackLayout;
                OrdersContainer.Children.Add(card);
            }
        }

        // Class to hold product name and quantity for each order
        public class ProductQuantity
        {
            public string ProductName { get; set; }
            public decimal Quantity { get; set; }
        }

        public class OrderRow
        {
            public int OrderId { get; set; } // Unique identifier for the order (index)
            public string CustomerName { get; set; }
            public ObservableCollection<ProductQuantity> ProductQuantities { get; set; }
        }
    }
}
