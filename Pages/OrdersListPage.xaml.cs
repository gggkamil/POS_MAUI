using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using OfficeOpenXml;
using System.IO;
using System.Threading.Tasks;

namespace CashierApp
{
    public partial class OrdersListPage : ContentPage
    {
        private readonly OrderRow _selectedOrder;
        private readonly string _filePath = @"C:\Kasa\Zamówienia\wydawka_uaktualniona_tylko.xlsx"; // Update file path as needed

        public OrdersListPage(OrderRow selectedOrder)
        {
            InitializeComponent();
            _selectedOrder = selectedOrder;
            DisplaySelectedOrder(selectedOrder);
        }

        private void DisplaySelectedOrder(OrderRow order)
        {
            OrdersStack.Children.Clear(); // Clear previous content

            // Create a frame for the selected order
            var frame = new Frame
            {
                BorderColor = Colors.Gray,
                Padding = 10,
                CornerRadius = 8,
                Margin = new Thickness(0, 5)
            };

            var stackLayout = new StackLayout { Spacing = 10 };

            // Display Customer Name and Order ID
            stackLayout.Children.Add(new Label
            {
                Text = $"Order ID: {order.OrderId} - {order.CustomerName}",
                FontAttributes = FontAttributes.Bold,
                FontSize = 18
            });

            // Title for Products Section
            stackLayout.Children.Add(new Label
            {
                Text = "Products and Quantities",
                FontAttributes = FontAttributes.Bold,
                FontSize = 16,
                Margin = new Thickness(0, 10, 0, 5)
            });

            // Product Grid: Display products and their quantities
            var productGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star }, // Product Name Column
                    new ColumnDefinition { Width = GridLength.Auto }   // Editable Quantity Column
                },
                RowSpacing = 5,
                ColumnSpacing = 15 // Increased spacing between columns
            };

            // Add Header Row for Product Name and Quantity
            productGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var productNameHeader = new Label
            {
                Text = "Product Name",
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 0, 10, 0)
            };
            productGrid.Children.Add(productNameHeader);
            Grid.SetColumn(productNameHeader, 0);
            Grid.SetRow(productNameHeader, 0);

            var quantityHeader = new Label
            {
                Text = "Quantity",
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.End
            };
            productGrid.Children.Add(quantityHeader);
            Grid.SetColumn(quantityHeader, 1);
            Grid.SetRow(quantityHeader, 0);

            for (int i = 0; i < order.ProductQuantities.Count; i++)
            {
                var productQuantity = order.ProductQuantities[i];
                int rowIndex = i + 1; 

                productGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var productLabel = new Label
                {
                    Text = productQuantity.ProductName,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                productGrid.Children.Add(productLabel);
                Grid.SetColumn(productLabel, 0);
                Grid.SetRow(productLabel, rowIndex);

                // Editable Quantity Entry
                var quantityEntry = new Entry
                {
                    Text = productQuantity.Quantity.ToString(),
                    Keyboard = Keyboard.Numeric,
                    HorizontalOptions = LayoutOptions.End,
                    WidthRequest = 80
                };

                // Update quantity when the entry is modified
                quantityEntry.TextChanged += (sender, e) =>
                {
                    if (decimal.TryParse(e.NewTextValue, out decimal newQuantity))
                    {
                        productQuantity.Quantity = newQuantity;
                    }
                };

                productGrid.Children.Add(quantityEntry);
                Grid.SetColumn(quantityEntry, 1);
                Grid.SetRow(quantityEntry, rowIndex);
            }

            // Add the product grid to the stack layout
            stackLayout.Children.Add(productGrid);

            // Add the "Save" button
            var saveButton = new Button
            {
                Text = "Save Changes",
                BackgroundColor = Colors.DodgerBlue,
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 15, 0, 0)
            };
            saveButton.Clicked += OnSaveButtonClicked;

            // Add the Save button to the stack layout
            stackLayout.Children.Add(saveButton);

            frame.Content = stackLayout;

            // Add the frame to the OrdersStack
            OrdersStack.Children.Add(frame);
        }

        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            try
            {
                await SaveOrderToExcel(_selectedOrder);
                await DisplayAlert("Success", "Order changes saved successfully!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not save order: {ex.Message}", "OK");
            }
        }

        private async Task SaveOrderToExcel(OrderRow order)
        {
            if (!File.Exists(_filePath))
            {
                await DisplayAlert("Error", "Excel file not found.", "OK");
                return;
            }

            // Open and update the Excel file using EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(_filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];

                // Locate the row matching the order ID
                int orderRowIndex = -1;
                for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                {
                    if (int.TryParse(worksheet.Cells[row, 1].Text, out int orderId) && orderId == order.OrderId)
                    {
                        orderRowIndex = row;
                        break;
                    }
                }

                if (orderRowIndex == -1)
                {
                    await DisplayAlert("Error", "Order not found in Excel file.", "OK");
                    return;
                }

                // Update product quantities in the Excel file
                foreach (var productQuantity in order.ProductQuantities)
                {
                    for (int col = 3; col <= worksheet.Dimension.Columns; col++)
                    {
                        var header = worksheet.Cells[1, col].Text;
                        if (header == productQuantity.ProductName)
                        {
                            worksheet.Cells[orderRowIndex, col].Value = productQuantity.Quantity;
                            break;
                        }
                    }
                }

                // Save changes to the Excel file
                package.Save();
            }
        }
    }
}
