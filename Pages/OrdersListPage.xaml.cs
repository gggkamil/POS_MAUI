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
        private readonly string _filePath = @"C:\Kasa\Zamówienia\wydawka_uaktualniona_tylko.xlsx";

        public OrdersListPage(OrderRow selectedOrder)
        {
            InitializeComponent();
            _selectedOrder = selectedOrder;
            DisplaySelectedOrder(selectedOrder);
        }

        private void DisplaySelectedOrder(OrderRow order)
        {
            OrdersStack.Children.Clear();

            var frame = new Frame
            {
                BorderColor = Colors.Gray,
                Padding = 10,
                CornerRadius = 8,
                Margin = new Thickness(0, 5),
                BackgroundColor = Colors.White
            };

            var stackLayout = new StackLayout { Spacing = 10 };

            stackLayout.Children.Add(new Label
            {
                Text = $"Order ID: {order.OrderId} - {order.CustomerName}",
                FontAttributes = FontAttributes.Bold,
                FontSize = 18
                
            });

            var productGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto } // New column for superscript
                },
                RowSpacing = 5,
                ColumnSpacing = 15
            };

            // Add headers
            productGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var productNameHeader = new Label
            {
                Text = "Product Name",
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Start
            };
            productGrid.Children.Add(productNameHeader);
            Grid.SetRow(productNameHeader, 0);
            Grid.SetColumn(productNameHeader, 0);

            var quantityHeader = new Label
            {
                Text = "Ilość",
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            };
            productGrid.Children.Add(quantityHeader);
            Grid.SetRow(quantityHeader, 0);
            Grid.SetColumn(quantityHeader, 1);

            var superscriptHeader = new Label
            {
                Text = "Znak",
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            };
            productGrid.Children.Add(superscriptHeader);
            Grid.SetRow(superscriptHeader, 0);
            Grid.SetColumn(superscriptHeader, 2);

            for (int i = 0; i < order.ProductQuantities.Count; i++)
            {
                var productQuantity = order.ProductQuantities[i];
                int rowIndex = i + 1;

                productGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Product Name
                var productLabel = new Label
                {
                    Text = productQuantity.ProductName,
                    TextColor = Colors.Black,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start
                };
                productGrid.Children.Add(productLabel);
                Grid.SetRow(productLabel, rowIndex);
                Grid.SetColumn(productLabel, 0);

                // Quantity Entry
                var quantityEntry = new Entry
                {
                    Text = productQuantity.Quantity.ToString(),
                    Keyboard = Keyboard.Numeric,
                    HorizontalOptions = LayoutOptions.Center,
                    WidthRequest = 80,
                    TextColor = productQuantity.Quantity == 0 ? Colors.Red : Colors.Black
                };

                quantityEntry.TextChanged += (sender, e) =>
                {
                    if (decimal.TryParse(e.NewTextValue, out decimal newQuantity))
                    {
                        productQuantity.Quantity = newQuantity;
                    }
                };
                productGrid.Children.Add(quantityEntry);
                Grid.SetRow(quantityEntry, rowIndex);
                Grid.SetColumn(quantityEntry, 1);

                // Superscript Entry
                var superscriptEntry = new Entry
                {
                    Text = productQuantity.Superscript,
                    Placeholder = "znak",
                    HorizontalOptions = LayoutOptions.Center,
                    TextColor =  Colors.Black,
                    WidthRequest = 80
                };
                superscriptEntry.TextChanged += (sender, e) =>
                {
                    productQuantity.Superscript = e.NewTextValue;
                };
                productGrid.Children.Add(superscriptEntry);
                Grid.SetRow(superscriptEntry, rowIndex);
                Grid.SetColumn(superscriptEntry, 2);
            }

            stackLayout.Children.Add(productGrid);

            var saveButton = new Button
            {
                Text = "Zapisz",
                BackgroundColor = Colors.Green,
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 15, 0, 0)
            };
            saveButton.Clicked += OnSaveButtonClicked;

            stackLayout.Children.Add(saveButton);
            frame.Content = stackLayout;
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

            // Ensure EPPlus is licensed for non-commercial use
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(_filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];

                // Find the row corresponding to the order
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

                // Update product quantities with superscripts
                foreach (var productQuantity in order.ProductQuantities)
                {
                    for (int col = 3; col <= worksheet.Dimension.Columns; col++)
                    {
                        var header = worksheet.Cells[1, col].Text;

                        if (header == productQuantity.ProductName)
                        {
                            string quantityText = productQuantity.Quantity.ToString();

                            // Append the superscript if it exists
                            if (!string.IsNullOrEmpty(productQuantity.Superscript))
                            {
                                quantityText += ConvertToSuperscript(productQuantity.Superscript);
                            }

                            worksheet.Cells[orderRowIndex, col].Value = quantityText;
                            break;
                        }
                    }
                }

                // Save the Excel file
                package.Save();
            }
        }
        private string ConvertToSuperscript(string text)
        {
            var superscriptMap = new Dictionary<char, char>
            {
                { '0', '⁰' }, { '1', '¹' }, { '2', '²' }, { '3', '³' },
                { '4', '⁴' }, { '5', '⁵' }, { '6', '⁶' }, { '7', '⁷' },
                { '8', '⁸' }, { '9', '⁹' }, { 'a', 'ᵃ' }, { 'b', 'ᵇ' },
                { 'c', 'ᶜ' }, { 'd', 'ᵈ' }, { 'e', 'ᵉ' }, { 'f', 'ᶠ' },
                { 'g', 'ᶢ' }, { 'h', 'ʰ' }, { 'i', 'ⁱ' }, { 'j', 'ʲ' },
                { 'k', 'ᵏ' }, { 'l', 'ˡ' }, { 'm', 'ᵐ' }, { 'n', 'ⁿ' },
                { 'o', 'ᵒ' }, { 'p', 'ᵖ' }, { 'r', 'ʳ' }, { 's', 'ˢ' },
                { 't', 'ᵗ' }, { 'u', 'ᵘ' }, { 'v', 'ᵛ' }, { 'w', 'ʷ' },
                { 'x', 'ˣ' }, { 'y', 'ʸ' }, { 'z', 'ᶻ' },
                { 'A', 'ᴬ' }, { 'B', 'ᴮ' }, { 'C', 'ᶜ' }, { 'D', 'ᴰ' },
                { 'E', 'ᴱ' }, { 'F', 'ᶠ' }, { 'G', 'ᴳ' }, { 'H', 'ᴴ' },
                { 'I', 'ᴵ' }, { 'J', 'ᴶ' }, { 'K', 'ᴷ' }, { 'L', 'ᴸ' },
                { 'M', 'ᴹ' }, { 'N', 'ᴺ' }, { 'O', 'ᴼ' }, { 'P', 'ᴾ' },
                { 'R', 'ᴿ' }, { 'S', 'ˢ' }, { 'T', 'ᵀ' }, { 'U', 'ᵁ' },
                { 'V', 'ⱽ' }, { 'W', 'ᵂ' }, { 'X', 'ˣ' }, { 'Y', 'ʸ' },
                { 'Z', 'ᶻ' },{ '.' ,'‧'}, {',','‧'}
            };


            return string.Concat(text.Select(c => superscriptMap.ContainsKey(c) ? superscriptMap[c] : c));
        }

    }
}
