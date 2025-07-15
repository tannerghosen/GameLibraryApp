using Microsoft.Extensions.Logging;

namespace GamesLibraryApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Settings.Init();
            GLASQL.Init();
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
