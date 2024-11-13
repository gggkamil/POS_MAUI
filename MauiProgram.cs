using ButchersCashier.Services;
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
                    //fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
            builder.Services.AddSingleton<IReceiptSaveService, ReceiptSaveService>();
            builder.Services.AddSingleton<ReceiptService>();
#endif

            return builder.Build();
        }
    }
}
