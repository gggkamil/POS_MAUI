using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CashierApp
{
    public partial class OrdersPage : ContentPage
    {
        private string folderPath = @"C:\Kasa\Zamówienia";
        private string fileName = "wydawka_uaktualniona_tylko.xlsx";
        private string filePath;

        public ObservableCollection<OrderRow> Orders { get; set; } = new();

        public OrdersPage()
        {
            InitializeComponent();
            filePath = Path.Combine(folderPath, fileName);
            BindingContext = this;

            EnsureFileExists();
            LoadOrdersFromExcel();
        }

        private async void LoadOrdersFromExcel()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0];

                    // Check if worksheet has a valid Dimension
                    if (worksheet.Dimension == null)
                    {
                        await DisplayAlert("Info", "No data found in the worksheet.", "OK");
                        return;
                    }

                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    for (int row = 2; row <= rowCount; row++) // Starting from row 2 to skip headers
                    {
                        // Read values in the row, including the CustomerName
                        var customerName = worksheet.Cells[row, 2].Text?.Trim(); // Assuming column 2 is "CustomerName"

                        // Skip row if CustomerName is empty
                        if (string.IsNullOrEmpty(customerName))
                            continue;

                        var orderId = int.TryParse(worksheet.Cells[row, 1].Text, out var id) ? id : 0;
                        var productQuantities = new ObservableCollection<ProductQuantity>();

                        // Loop through the columns for product quantities starting from column 3 (assuming column 1 is ID and column 2 is CustomerName)
                        for (int col = 3; col <= colCount; col++)
                        {
                            string productName = worksheet.Cells[1, col].Text; // Column headers in row 1 are product names
                            if (decimal.TryParse(worksheet.Cells[row, col].Text, out var quantity))
                            {
                                productQuantities.Add(new ProductQuantity
                                {
                                    ProductName = productName,
                                    Quantity = quantity
                                });
                            }
                        }

                        Orders.Add(new OrderRow
                        {
                            OrderId = orderId,
                            CustomerName = customerName,
                            ProductQuantities = productQuantities
                        });
                    }

                    DisplayOrders();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not load orders: {ex.Message}", "OK");
            }
        }


        private void EnsureFileExists()
        {
            // Create directory if it doesn’t exist
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Create file with initial data if it doesn’t exist
            if (!File.Exists(filePath))
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets.Add("Orders");
                    worksheet.Cells[1, 1].Value = "LP."; // Order ID
                    worksheet.Cells[1, 2].Value = "Imię i nazwisko"; // Customer name

                    // Add more product columns as needed
                    worksheet.Cells[1, 3].Value = "Product 1";
                    worksheet.Cells[1, 4].Value = "Product 2";
                    worksheet.Cells[1, 5].Value = "Product 3";

                    package.Save();
                }
            }
        }

        private void DisplayOrders()
        {
            OrdersContainer.Children.Clear();  // Clear any previous content in the container

            // Loop through each order and display it
            foreach (var order in Orders)
            {
                var frame = new Frame
                {
                    BorderColor = Colors.Gray,
                    Padding = 10,
                    CornerRadius = 8,
                    Margin = new Thickness(0, 5)
                };

                var stackLayout = new StackLayout { Spacing = 5 };

                // Add basic order information (Order ID and Customer Name)
                stackLayout.Children.Add(new Label
                {
                    Text = $"Order ID: {order.OrderId} - {order.CustomerName}",
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 18
                });

                // Add a tap gesture recognizer for navigation
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += async (sender, e) => await OnOrderSelected(order); // Navigate to Order Details page on tap
                frame.GestureRecognizers.Add(tapGesture);

                frame.Content = stackLayout; // Set the content of the frame
                OrdersContainer.Children.Add(frame); // Add the frame to the container
            }
        }


        private async Task OnOrderSelected(OrderRow selectedOrder)
        {
            await Navigation.PushAsync(new OrdersListPage(selectedOrder));
        }
    }

    public class ProductQuantity
    {
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
    }

    public class OrderRow
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public ObservableCollection<ProductQuantity> ProductQuantities { get; set; }
    }
}
