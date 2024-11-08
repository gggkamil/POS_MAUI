using System.Globalization;
using ButchersCashier.Auth;
using ButchersCashier.Models;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;
using ButchersCashier;
using ButchersCashier.Pages;
using CashierApps;
using System.Collections.ObjectModel;
using ButchersCashier.Services;

namespace CashierApp
{
    public partial class MainPage : TabbedPage
    {
        private decimal totalAmount = 0; // Variable to store the total amount 
        private List<Product> products = new List<Product>(); // List to store products 
        private readonly LocalAuthService _authService; // Authentication service 
        private readonly IReceiptSaveService _receiptSaveService; // Add the ReceiptSaveService dependency

        public ObservableCollection<ReceiptItem> ReceiptItems { get; set; } = new();

        // Update MainPage constructor to accept IReceiptSaveService
        public MainPage(string username, IReceiptSaveService receiptSaveService)
        {
            InitializeComponent();
            _authService = new LocalAuthService();
            _authService.SetCurrentUser(username);
            _receiptSaveService = receiptSaveService; // Initialize the receipt save service

            AddProductsTab();
            AddExcelListTab();
            CheckUserAccess();
        }

        private void CheckUserAccess()
        {
            string currentUser = _authService.GetCurrentUsername();
            if (!string.IsNullOrEmpty(currentUser) && currentUser.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                AddAdminTab();
            }
        }

        private void AddProductsTab()
        {
            var productPage = new CashierApp.ProductsPage(ReceiptItems, _receiptSaveService);
            Children.Add(new NavigationPage(productPage) { Title = "Produkty" });
        }

        private void AddAdminEditTab(Product product)
        {
            var adminEditPage = new CashierApp.AdminEditPage(product);
            Children.Add(new NavigationPage(adminEditPage) { Title = "Edit Products" });
        }

        private async void OnProductSelected(Product selectedProduct)
        {
            if (selectedProduct != null)
            {
                AddAdminEditTab(selectedProduct);
            }
        }

        private void AddAdminTab()
        {
            var adminPage = new CashierApp.AdminPage(this);
            Children.Add(new NavigationPage(adminPage) { Title = "Admin" });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            _authService.Logout();
            Application.Current.MainPage = new LoginPage(_receiptSaveService);
        }

        private void AddExcelListTab()
        {
            // Pass both ReceiptItems and IReceiptSaveService to ExcelFileListPage
            var excelListPage = new ExcelFileListPage(ReceiptItems, _receiptSaveService);
            Children.Add(new NavigationPage(excelListPage) { Title = "WZ" });
        }
    }
}
