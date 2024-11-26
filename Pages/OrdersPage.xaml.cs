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

                    if (worksheet.Dimension == null)
                    {
                        await DisplayAlert("Info", "No data found in the worksheet.", "OK");
                        return;
                    }

                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    for (int row = 2; row <= rowCount; row++) // Starting from row 2 to skip headers
                    {
                        var customerName = worksheet.Cells[row, 2].Text?.Trim();
                        if (string.IsNullOrEmpty(customerName)) continue;

                        var orderId = int.TryParse(worksheet.Cells[row, 1].Text, out var id) ? id : 0;
                        var productQuantities = new ObservableCollection<ProductQuantity>();

                        for (int col = 3; col <= colCount; col++) // Starting at column 3 for product data
                        {
                            string productName = worksheet.Cells[1, col].Text; // Get header (product name)
                            string cellValue = worksheet.Cells[row, col].Text; // Get cell value (quantity and superscript)

                            decimal quantity = 0;
                            string superscript = null;

                            if (!string.IsNullOrEmpty(cellValue))
                            {
                                // Extract numeric quantity and superscript
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

                            productQuantities.Add(new ProductQuantity
                            {
                                ProductName = productName,
                                Quantity = quantity,
                                Superscript = superscript
                            });
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
        public string Superscript { get; set; }
    }

    public class OrderRow
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public ObservableCollection<ProductQuantity> ProductQuantities { get; set; }
    }
}
