using ButchersCashier.Pages;
using ButchersCashier.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ButchersCashier
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            // Retrieve services from the DI container
            var receiptSaveService = serviceProvider.GetService<IReceiptSaveService>();

            // Pass required services to LoginPage
            MainPage = new LoginPage(receiptSaveService);
        }
    }
}