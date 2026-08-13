using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Teleprompter.Data;

namespace Teleprompter.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private CancellationTokenSource? _persistCts;

        [ObservableProperty]
        public partial double Speed { get; set; }

        [ObservableProperty]
        public partial double FontSize { get; set; }

        [ObservableProperty]
        public partial bool MirrorMode { get; set; }

        [ObservableProperty]
        public partial bool HighlightEnabled { get; set; }

        [ObservableProperty]
        public partial double HighlightWpm { get; set; } = 180;

        [ObservableProperty]
        public partial Color HighlightColor { get; set; } = Colors.Yellow;

        [ObservableProperty]
        public partial Color TextColor { get; set; } = Colors.White;

        [ObservableProperty]
        public partial Color BackgroundColor { get; set; } = Colors.Black;

        [ObservableProperty]
        public partial string Theme { get; set; } = "System";

        public string SpeedText => $"{Speed:0} px/s";

        public string FontSizeText => $"{FontSize:0} pt";

        public string HighlightWpmText => $"{HighlightWpm:0} wpm";

        public SettingsViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        partial void OnSpeedChanged(double value)
        {
            OnPropertyChanged(nameof(SpeedText));
            SchedulePersist();
        }

        partial void OnFontSizeChanged(double value)
        {
            OnPropertyChanged(nameof(FontSizeText));
            SchedulePersist();
        }

        partial void OnMirrorModeChanged(bool value) => SchedulePersist();

        partial void OnHighlightEnabledChanged(bool value) => SchedulePersist();

        partial void OnHighlightWpmChanged(double value)
        {
            OnPropertyChanged(nameof(HighlightWpmText));
            SchedulePersist();
        }

        partial void OnHighlightColorChanged(Color value) => SchedulePersist();

        partial void OnTextColorChanged(Color value) => SchedulePersist();

        partial void OnBackgroundColorChanged(Color value) => SchedulePersist();

        partial void OnThemeChanged(string value)
        {
            ApplyTheme(value);
            SchedulePersist();
        }

        public async Task InitializeAsync()
        {
            var globals = await _databaseService.GetGlobalDefaultsAsync();
            Speed = globals.Speed;
            FontSize = globals.FontSize;
            MirrorMode = globals.MirrorMode;
            TextColor = globals.TextColor;
            BackgroundColor = globals.BackgroundColor;
            HighlightEnabled = globals.HighlightEnabled;
            HighlightWpm = globals.HighlightWpm;
            HighlightColor = globals.HighlightColor;
            Theme = await _databaseService.GetSettingAsync(AppSettingKeys.Theme) ?? "System";
            ApplyTheme(Theme);
        }

        private void SchedulePersist()
        {
            _persistCts?.Cancel();
            _persistCts = new CancellationTokenSource();
            var token = _persistCts.Token;

            _ = PersistAsync(token);
        }

        private async Task PersistAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(800, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            await SaveAsync();
        }

        private async Task SaveAsync()
        {
            await _databaseService.SaveGlobalDefaultsAsync(new GlobalDefaults
            {
                Speed = Speed,
                FontSize = (int)Math.Round(FontSize),
                MirrorMode = MirrorMode,
                TextColor = TextColor,
                BackgroundColor = BackgroundColor,
                HighlightEnabled = HighlightEnabled,
                HighlightWpm = HighlightWpm,
                HighlightColor = HighlightColor,
            });

            await _databaseService.SetSettingAsync(AppSettingKeys.Theme, Theme);
        }

        public async Task FlushPersistAsync()
        {
            _persistCts?.Cancel();
            await SaveAsync();
        }

        [RelayCommand]
        private void IncreaseFont() => FontSize = Math.Min(96, FontSize + 2);

        [RelayCommand]
        private void DecreaseFont() => FontSize = Math.Max(10, FontSize - 2);

        [RelayCommand]
        private void SelectTextColor(string hex)
        {
            if (Color.TryParse(hex, out var color))
                TextColor = color;
        }

        [RelayCommand]
        private void SelectBackgroundColor(string hex)
        {
            if (Color.TryParse(hex, out var color))
                BackgroundColor = color;
        }

        [RelayCommand]
        private void SelectHighlightColor(string hex)
        {
            if (Color.TryParse(hex, out var color))
                HighlightColor = color;
        }

        [RelayCommand]
        private void SetTheme(string theme) => Theme = theme;

        private static void ApplyTheme(string theme)
        {
            if (Application.Current is null)
                return;

            Application.Current.UserAppTheme = theme switch
            {
                "Light" => AppTheme.Light,
                "Dark" => AppTheme.Dark,
                _ => AppTheme.Unspecified,
            };
        }
    }
}
