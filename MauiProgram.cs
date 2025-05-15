using ButchersCashier.Data;
using ButchersCashier.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ButchersCashier
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ButchersCashierDB;Trusted_Connection=True;TrustServerCertificate=True;";
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

#if DEBUG
            builder.Logging.AddDebug();
            builder.Services.AddSingleton<IReceiptSaveService, ReceiptSaveService>();
           
#endif

            return builder.Build();
        }
    }
}
