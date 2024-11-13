using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace CashierApp
{
    public partial class OrdersListPage : ContentPage
    {
        public OrdersListPage(OrderRow selectedOrder)
        {
            InitializeComponent();
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
            new ColumnDefinition { Width = GridLength.Auto } // Editable Quantity Column
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
                Margin = new Thickness(0, 0, 10, 0) // Margin for spacing
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

            // Dynamically add rows for each product
            for (int i = 0; i < order.ProductQuantities.Count; i++)
            {
                var productQuantity = order.ProductQuantities[i];
                int rowIndex = i + 1; // Row index for each product (start from 1 due to header row)

                // Define the row in the grid for the current product
                productGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Product Name Label
                var productLabel = new Label
                {
                    Text = productQuantity.ProductName,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start,
                    Margin = new Thickness(0, 0, 10, 0) // Margin for spacing
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
                    WidthRequest = 80 // Define width for consistent layout
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
            frame.Content = stackLayout;

            // Add the frame to the OrdersStack
            OrdersStack.Children.Add(frame);
        }




    }
}
