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
            OrdersContainer.Children.Clear(); // Clear any previous content in the container

            foreach (var order in Orders)
            {
                // Create a grid to hold the order info, product summary, and delete button
                var mainGrid = new Grid
                {
                    ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },  // For order information and product summary
                new ColumnDefinition { Width = GridLength.Auto }  // For delete button
            },
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 5)
                };

                // Add basic order information (Order ID and Customer Name)
                var orderInfo = new Label
                {
                    Text = $"Order ID: {order.OrderId} - {order.CustomerName}",
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 18,
                    VerticalOptions = LayoutOptions.Center
                };

                // Add a tap gesture recognizer to the order info
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += async (sender, e) => await OnOrderSelected(order);
                

                // Create a small grid to display product names (top) and quantities (bottom)
                var productGrid = new Grid
                {
                    ColumnSpacing = 5,
                    RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // Product Names
                new RowDefinition { Height = GridLength.Auto }  // Quantities
            }
                };

                int column = 0;
                foreach (var product in order.ProductQuantities)
                {
                    if (product.Quantity > 0 || !string.IsNullOrEmpty(product.Superscript))
                    {
                        var productNameLabel = new Label
                        {
                            Text = product.ProductName,
                            FontSize = 12,
                            HorizontalTextAlignment = TextAlignment.Center
                        };
                        productGrid.Children.Add(productNameLabel);
                        Grid.SetRow(productNameLabel, 0); // Top row for product names
                        Grid.SetColumn(productNameLabel, column);

                        var columnDefinition = new ColumnDefinition { Width = GridLength.Auto };
                        productGrid.ColumnDefinitions.Add(columnDefinition);

                        // Add quantities (second row)
                        var quantityLabel = new Label
                        {
                            Text = $"{product.Quantity}{(string.IsNullOrEmpty(product.Superscript) ? "" : product.Superscript)}",
                            FontSize = 12,
                            HorizontalTextAlignment = TextAlignment.Center
                        };
                        productGrid.Children.Add(quantityLabel);
                        Grid.SetRow(quantityLabel, 1); // Bottom row for quantities
                        Grid.SetColumn(quantityLabel, column);

                        column++;
                    }
                }

                // Add Delete Button
                var deleteButton = new Button
                {
                    Text = "X",
                    Command = new Command(async () => await DeleteOrder(order)), // Bind to delete logic
                    WidthRequest = 24,
                    HeightRequest = 24,
                    FontSize = 12,
                    BackgroundColor = Colors.Red,
                    TextColor = Colors.White,
                    CornerRadius = 12,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center
                };

                // Add elements to the main grid
                var orderInfoStack = new StackLayout { Spacing = 5 };
                orderInfoStack.Children.Add(orderInfo);
                if (column > 0) // Only add product grid if there are products to display
                {
                    orderInfoStack.Children.Add(productGrid);
                }

                mainGrid.Children.Add(orderInfoStack);
                Grid.SetColumn(orderInfoStack, 0); // Place in the first column

                mainGrid.Children.Add(deleteButton);
                Grid.SetColumn(deleteButton, 1); // Place in the second column

                // Wrap the grid in a frame for styling
                var frame = new Frame
                {
                    BorderColor = Colors.Gray,
                    Padding = new Thickness(10, 5),
                    CornerRadius = 8,
                    Content = mainGrid // Set the grid as the content of the frame
                };
                frame.GestureRecognizers.Add(tapGesture);
                // Add the frame to the container
                OrdersContainer.Children.Add(frame);
            }
        }




        private async Task DeleteOrder(OrderRow order)
        {
            bool confirm = await DisplayAlert("Delete Order",
                $"Are you sure you want to delete the order for '{order.CustomerName}'?", "Yes", "No");

            if (!confirm) return;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0];

                    // Find the row corresponding to the order
                    int rowToDelete = -1;
                    for (int row = 2; row <= worksheet.Dimension.Rows; row++) // Start from row 2 to skip headers
                    {
                        if (int.TryParse(worksheet.Cells[row, 1].Text, out var id) && id == order.OrderId)
                        {
                            rowToDelete = row;
                            break;
                        }
                    }

                    if (rowToDelete == -1)
                    {
                        await DisplayAlert("Error", "Order not found.", "OK");
                        return;
                    }

                    // Delete the row
                    worksheet.DeleteRow(rowToDelete);

                    // Save the Excel file
                    package.Save();
                }

                // Remove from the observable collection and refresh UI
                Orders.Remove(order);
                DisplayOrders();

                await DisplayAlert("Success", "Order deleted successfully.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not delete order: {ex.Message}", "OK");
            }
        }

        private async void AddOrderButton_Clicked(object sender, EventArgs e)
        {
            string clientName = await DisplayPromptAsync("New Order", "Enter the client's name:");

            if (!string.IsNullOrWhiteSpace(clientName))
            {
                // Add new order to Excel
                AddOrderToExcel(clientName);

                // Reload orders from Excel and refresh the UI
                Orders.Clear();
                LoadOrdersFromExcel();
            }
        }
        private void AddOrderToExcel(string clientName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];

                int lastRow = worksheet.Dimension.Rows;
                int nextOrderId = lastRow; // Assuming the last row has the last order's ID

                // Add new order data
                worksheet.Cells[lastRow + 1, 1].Value = nextOrderId;
                worksheet.Cells[lastRow + 1, 2].Value = clientName;

                // Add default values for product columns if needed
                for (int col = 3; col <= worksheet.Dimension.Columns; col++)
                {
                    worksheet.Cells[lastRow + 1, col].Value = ""; // Initialize with empty values
                }

                package.Save();
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
