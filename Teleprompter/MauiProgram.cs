using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Teleprompter.Data;
using Teleprompter.Services;
using Teleprompter.ViewModels;
using Teleprompter.Views;

namespace Teleprompter
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<IFileImportService, FileImportService>();
            builder.Services.AddSingleton<IFileExportService, FileExportService>();
            builder.Services.AddSingleton<IRemoteControlService, RemoteControlService>();
            builder.Services.AddTransient<ScriptListViewModel>();
            builder.Services.AddTransient<ScriptEditorViewModel>();
            builder.Services.AddTransient<TeleprompterPlayerViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<ScriptListPage>();
            builder.Services.AddTransient<ScriptEditorPage>();
            builder.Services.AddTransient<TeleprompterPlayerPage>();
            builder.Services.AddTransient<SettingsPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
