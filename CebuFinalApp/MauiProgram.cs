using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace CebuFinalApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiMaps()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Font Awesome 6 Free-Solid-900.otf", "FASolid");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            AppDomain.CurrentDomain.UnhandledException += (s,e) =>
    Console.WriteLine($"Unhandled: {e.ExceptionObject}");
TaskScheduler.UnobservedTaskException += (s,e) =>
{
    Console.WriteLine($"UnobservedTask: {e.Exception}");
    e.SetObserved();
};

            return builder.Build();
        }
    }
}
