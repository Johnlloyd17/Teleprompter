using Microsoft.Extensions.DependencyInjection;
using Teleprompter.Data;

namespace Teleprompter
{
    public partial class App : Application
    {
        private readonly DatabaseService _databaseService;

        public App(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            _ = ApplyThemeAsync();
            return new Window(new AppShell());
        }

        private async Task ApplyThemeAsync()
        {
            var theme = await _databaseService.GetSettingAsync(AppSettingKeys.Theme);
            UserAppTheme = theme switch
            {
                "Light" => AppTheme.Light,
                "Dark" => AppTheme.Dark,
                _ => AppTheme.Unspecified,
            };
        }
    }
}
