using ButchersCashier.Auth;
using ButchersCashier.Services;
using CashierApp;
using Microsoft.Maui.Controls;

namespace ButchersCashier.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly LocalAuthService _authService = new LocalAuthService();
        private readonly IReceiptSaveService _receiptSaveService;
        public LoginPage(IReceiptSaveService receiptSaveService)
        {
            InitializeComponent();
            _receiptSaveService = receiptSaveService;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry.Text;
            var password = PasswordEntry.Text;

            var isAuthenticated = await _authService.LoginAsync(username, password);
            if (isAuthenticated)
            {
                
                Application.Current.MainPage = new MainPage(username, _receiptSaveService);
            }
            else
            {
                await DisplayAlert("Błąd", "Niepoprawne hasło lub login", "OK");
            }
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry.Text;
            var password = PasswordEntry.Text;

            await _authService.RegisterAsync(username, password);
            await DisplayAlert("Success", "Utworzono użytkownika!", "OK");
        }
    }
}
