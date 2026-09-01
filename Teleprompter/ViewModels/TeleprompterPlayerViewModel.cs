using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Teleprompter.Data;
using Teleprompter.Models;
using Teleprompter.Services;

namespace Teleprompter.ViewModels
{
    public enum PlaybackAction
    {
        Play,
        Pause,
        Stop,
        Rewind,
        Forward,
    }

    public partial class TeleprompterPlayerViewModel : ObservableObject, IQueryAttributable
    {
        private readonly DatabaseService _databaseService;
        private readonly IRemoteControlService _remoteControlService;
        private ScriptModel? _script;
        private int _scriptId;
        private bool _settingsDirty;
        private bool _remoteStarted;
        private CancellationTokenSource? _persistCts;
        private DateTime _lastProgressPersistTime = DateTime.MinValue;
        private static readonly TimeSpan ProgressPersistInterval = TimeSpan.FromSeconds(10);

        public event EventHandler<PlaybackAction>? ActionRequested;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _content = string.Empty;

        [ObservableProperty]
        private double _speed = 60;

        [ObservableProperty]
        private double _fontSize = 28;

        [ObservableProperty]
        private bool _mirrorMode;

        [ObservableProperty]
        private bool _highlightEnabled;

        [ObservableProperty]
        private double _highlightWpm = 180;

        [ObservableProperty]
        private Color _highlightColor = Colors.Yellow;

        [ObservableProperty]
        private Color _textColor = Colors.White;

        [ObservableProperty]
        private Color _backgroundColor = Colors.Black;

        [ObservableProperty]
        private double _progress;

        [ObservableProperty]
        private bool _isPlaying;

        [ObservableProperty]
        private bool _isFullScreen;

        public string SpeedText => $"{Speed:0} px/s";

        public string FontSizeText => $"{FontSize:0} pt";

        public string HighlightWpmText => $"{HighlightWpm:0} wpm";

        public string ProgressText => $"{Progress * 100:0}%";

        public bool IsNormalMode => !IsFullScreen;

        public TeleprompterPlayerViewModel(DatabaseService databaseService, IRemoteControlService remoteControlService)
        {
            _databaseService = databaseService;
            _remoteControlService = remoteControlService;
        }

        public void StartRemoteControl()
        {
            if (_remoteStarted)
                return;

            _remoteStarted = true;
            _remoteControlService.KeyPressed += OnRemoteKeyPressed;
            _remoteControlService.Start();
        }

        public void StopRemoteControl()
        {
            if (!_remoteStarted)
                return;

            _remoteStarted = false;
            _remoteControlService.KeyPressed -= OnRemoteKeyPressed;
            _remoteControlService.Stop();
        }

        private void OnRemoteKeyPressed(object? sender, RemoteKey key)
        {
            switch (key)
            {
                case RemoteKey.PlayPause:
                    TogglePlayCommand.Execute(null);
                    break;
                case RemoteKey.SpeedUp:
                    Speed = Math.Min(150, Speed + 5);
                    break;
                case RemoteKey.SpeedDown:
                    Speed = Math.Max(10, Speed - 5);
                    break;
            }
        }

        partial void OnSpeedChanged(double value)
        {
            OnPropertyChanged(nameof(SpeedText));
            _settingsDirty = true;
            SchedulePersist();
        }

        partial void OnFontSizeChanged(double value)
        {
            OnPropertyChanged(nameof(FontSizeText));
            _settingsDirty = true;
            SchedulePersist();
        }

        partial void OnMirrorModeChanged(bool value)
        {
            _settingsDirty = true;
            SchedulePersist();
        }

        partial void OnHighlightEnabledChanged(bool value)
        {
            _settingsDirty = true;
            SchedulePersist();
        }

        partial void OnHighlightWpmChanged(double value)
        {
            OnPropertyChanged(nameof(HighlightWpmText));
            _settingsDirty = true;
            SchedulePersist();
        }

        partial void OnHighlightColorChanged(Color value)
        {
            _settingsDirty = true;
            SchedulePersist();
        }

        partial void OnTextColorChanged(Color value)
        {
            _settingsDirty = true;
            SchedulePersist();
        }

        partial void OnBackgroundColorChanged(Color value)
        {
            _settingsDirty = true;
            SchedulePersist();
        }

        partial void OnIsFullScreenChanged(bool value)
        {
            OnPropertyChanged(nameof(IsNormalMode));
        }

        partial void OnProgressChanged(double value)
        {
            OnPropertyChanged(nameof(ProgressText));

            var now = DateTime.UtcNow;
            if (now - _lastProgressPersistTime < ProgressPersistInterval)
                return;

            _lastProgressPersistTime = now;
            SchedulePersist();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var scriptId = query.TryGetValue("ScriptId", out var idValue) && idValue is int id
                ? id
                : 0;
            var position = query.TryGetValue("PositionPercent", out var positionValue) && positionValue is double p
                ? p
                : 0;

            _ = LoadScriptAsync(scriptId, position);
        }

        private async Task LoadScriptAsync(int id, double positionPercent)
        {
            var script = await _databaseService.GetScriptAsync(id);
            if (script is null)
                return;

            _script = script;
            _scriptId = script.Id;
            Title = script.Title;
            Content = script.Content;

            await ApplySettingsAsync();

            Progress = positionPercent;
            _settingsDirty = false;
        }

        private async Task ApplySettingsAsync()
        {
            var globals = await _databaseService.GetGlobalDefaultsAsync();
            var perScript = await _databaseService.GetScriptSettingsAsync(_scriptId);

            if (perScript is null)
            {
                Speed = globals.Speed;
                FontSize = globals.FontSize;
                MirrorMode = globals.MirrorMode;
                TextColor = globals.TextColor;
                BackgroundColor = globals.BackgroundColor;
                HighlightEnabled = globals.HighlightEnabled;
                HighlightWpm = globals.HighlightWpm;
                HighlightColor = globals.HighlightColor;
                return;
            }

            Speed = perScript.ScrollSpeed > 0 ? perScript.ScrollSpeed : globals.Speed;
            FontSize = perScript.FontSize > 0 ? perScript.FontSize : globals.FontSize;
            MirrorMode = perScript.MirrorMode;
            HighlightEnabled = perScript.HighlightWpm > 0 ? perScript.HighlightEnabled : globals.HighlightEnabled;
            HighlightWpm = perScript.HighlightWpm > 0 ? perScript.HighlightWpm : globals.HighlightWpm;

            HighlightColor = Color.TryParse(perScript.HighlightColor, out var highlightColor)
                ? highlightColor
                : globals.HighlightColor;

            TextColor = Color.TryParse(perScript.TextColor, out var textColor)
                ? textColor
                : globals.TextColor;

            BackgroundColor = Color.TryParse(perScript.BackgroundColor, out var backgroundColor)
                ? backgroundColor
                : globals.BackgroundColor;
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
                await Task.Delay(1000, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (_settingsDirty)
                await SaveSettingsAsync();

            await SaveHistoryAsync(Progress);
        }

        private async Task SaveSettingsAsync()
        {
            if (_scriptId == 0)
                return;

            var settings = new ScriptSettingsModel
            {
                ScriptId = _scriptId,
                ScrollSpeed = Speed,
                FontSize = (int)Math.Round(FontSize),
                MirrorMode = MirrorMode,
                TextColor = TextColor.ToHex(),
                BackgroundColor = BackgroundColor.ToHex(),
                HighlightEnabled = HighlightEnabled,
                HighlightWpm = HighlightWpm,
                HighlightColor = HighlightColor.ToHex(),
            };

            await _databaseService.SaveScriptSettingsAsync(settings);
            _settingsDirty = false;
        }

        private async Task SaveHistoryAsync(double positionPercent)
        {
            if (_scriptId == 0)
                return;

            await _databaseService.SavePlaybackHistoryAsync(new PlaybackHistoryModel
            {
                ScriptId = _scriptId,
                LastPositionPercent = positionPercent,
                LastOpenedAt = DateTime.Now,
            });
        }

        public async Task FlushPersistAsync()
        {
            _persistCts?.Cancel();

            if (_settingsDirty)
                await SaveSettingsAsync();

            await SaveHistoryAsync(Progress);
        }

        [RelayCommand]
        private void TogglePlay() =>
            ActionRequested?.Invoke(this, IsPlaying ? PlaybackAction.Pause : PlaybackAction.Play);

        [RelayCommand]
        private void ToggleFullScreen() => IsFullScreen = !IsFullScreen;

        [RelayCommand]
        private void Stop() => ActionRequested?.Invoke(this, PlaybackAction.Stop);

        [RelayCommand]
        private void Rewind() => ActionRequested?.Invoke(this, PlaybackAction.Rewind);

        [RelayCommand]
        private void Forward() => ActionRequested?.Invoke(this, PlaybackAction.Forward);

        [RelayCommand]
        private void IncreaseFont() => FontSize = FontSize + 2;

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
    }
}
