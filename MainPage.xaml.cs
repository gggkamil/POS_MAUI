﻿using System.Globalization;
using ButchersCashier.Auth;
using ButchersCashier.Models;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;
using ButchersCashier;
using ButchersCashier.Pages;

namespace CashierApp
{
    public partial class MainPage : TabbedPage
    {
        private decimal totalAmount = 0; // Variable to store the total amount
        private List<Product> products = new List<Product>(); // List to store products
        private readonly LocalAuthService _authService; // Authentication service

        public MainPage(string username)
        {
            InitializeComponent();
            _authService = new LocalAuthService(); // Initialize the authentication service
            _authService.SetCurrentUser(username);
            CheckUserAccess(); // Check if the user is admin and manage tabs
            AddProductsTab();
        }

        private void CheckUserAccess()
        {
            string currentUser = _authService.GetCurrentUsername();
            if (!string.IsNullOrEmpty(currentUser) && currentUser.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                AddAdminTab(); // Add the Admin tab only for admin users
            }
        }

        private void AddProductsTab()
        {
            var productPage = new CashierApp.ProductsPage();
            Children.Add(new NavigationPage(productPage) { Title = "Products" });
        }
        private void AddAdminTab()
        {
 
            var adminPage = new CashierApp.AdminPage(); 
        
            Children.Add(new NavigationPage(adminPage) { Title = "Admin" });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
        }



        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            _authService.Logout(); // Clear the current user
            Application.Current.MainPage = new LoginPage(); // Navigate back to the login page
        }
    }
}
