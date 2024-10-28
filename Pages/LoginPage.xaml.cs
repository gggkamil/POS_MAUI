using ButchersCashier.Auth;
using CashierApp;
using Microsoft.Maui.Controls;

namespace ButchersCashier.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly LocalAuthService _authService = new LocalAuthService();

        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry.Text;
            var password = PasswordEntry.Text;

            var isAuthenticated = await _authService.LoginAsync(username, password);
            if (isAuthenticated)
            {
                await DisplayAlert("Success", "You are logged in!", "OK");
                Application.Current.MainPage = new MainPage(username);
            }
            else
            {
                await DisplayAlert("Error", "Invalid credentials", "OK");
            }
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry.Text;
            var password = PasswordEntry.Text;

            await _authService.RegisterAsync(username, password);
            await DisplayAlert("Success", "Registration complete!", "OK");
        }
    }
}
