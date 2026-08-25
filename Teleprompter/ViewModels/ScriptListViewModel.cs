using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Teleprompter.Data;
using Teleprompter.Models;
using Teleprompter.Services;
using Teleprompter.Views;

namespace Teleprompter.ViewModels
{
    public partial class ScriptListViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private readonly IFileImportService _fileImportService;

        public ObservableCollection<ScriptModel> Scripts { get; } = new();

        [ObservableProperty]
        private bool _hasSession;

        [ObservableProperty]
        private string _sessionTitle = string.Empty;

        public ScriptListViewModel(DatabaseService databaseService, IFileImportService fileImportService)
        {
            _databaseService = databaseService;
            _fileImportService = fileImportService;
        }

        public async Task InitializeAsync()
        {
            await LoadScriptsAsync();
            await LoadSessionAsync();
        }

        [RelayCommand]
        private async Task LoadScriptsAsync()
        {
            var scripts = await _databaseService.GetScriptsAsync();

            Scripts.Clear();
            foreach (var script in scripts)
                Scripts.Add(script);
        }

        [RelayCommand]
        private async Task AddScriptAsync()
        {
            await Shell.Current.GoToAsync(nameof(ScriptEditorPage));
        }

        [RelayCommand]
        private async Task ImportScriptAsync()
        {
            var imported = await _fileImportService.PickAndImportAsync();
            if (imported is null)
                return;

            await _databaseService.SaveScriptAsync(new ScriptModel
            {
                Title = imported.Title,
                Content = imported.Content,
            });

            await LoadScriptsAsync();
            await Shell.Current.DisplayAlertAsync("Imported", $"\"{imported.Title}\" has been imported.", "OK");
        }

        [RelayCommand]
        private async Task OpenScriptAsync(ScriptModel script)
        {
            var parameters = new ShellNavigationQueryParameters
            {
                { "ScriptId", script.Id }
            };
            await Shell.Current.GoToAsync(nameof(ScriptEditorPage), parameters);
        }

        [RelayCommand]
        private async Task OpenPlayerAsync(ScriptModel script)
        {
            var parameters = new ShellNavigationQueryParameters
            {
                { "ScriptId", script.Id }
            };
            await Shell.Current.GoToAsync(nameof(TeleprompterPlayerPage), parameters);
        }

        private async Task LoadSessionAsync()
        {
            var history = await _databaseService.GetLatestPlaybackHistoryAsync();
            if (history is null || history.LastPositionPercent >= 0.98)
            {
                HasSession = false;
                SessionTitle = string.Empty;
                return;
            }

            var script = await _databaseService.GetScriptAsync(history.ScriptId);
            HasSession = script is not null;
            SessionTitle = script?.Title ?? string.Empty;
        }

        [RelayCommand]
        private async Task ResumeSessionAsync()
        {
            var history = await _databaseService.GetLatestPlaybackHistoryAsync();
            if (history is null)
                return;

            var parameters = new ShellNavigationQueryParameters
            {
                { "ScriptId", history.ScriptId },
                { "PositionPercent", history.LastPositionPercent },
            };
            await Shell.Current.GoToAsync(nameof(TeleprompterPlayerPage), parameters);
        }

        [RelayCommand]
        private async Task OpenSettingsAsync()
        {
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }

        [RelayCommand]
        private async Task DeleteScriptAsync(ScriptModel script)
        {
            var confirmed = await Shell.Current.DisplayAlertAsync(
                "Delete script",
                $"Delete \"{script.Title}\"?",
                "Delete",
                "Cancel");

            if (!confirmed)
                return;

            await _databaseService.DeleteScriptAsync(script);
            await LoadScriptsAsync();
            await LoadSessionAsync();
        }
    }
}
