using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Teleprompter.Data;
using Teleprompter.Models;
using Teleprompter.Services;

namespace Teleprompter.ViewModels
{
    public partial class ScriptEditorViewModel : ObservableObject, IQueryAttributable
    {
        private readonly DatabaseService _databaseService;
        private readonly IFileExportService _fileExportService;
        private ScriptModel? _script;

        [ObservableProperty]
        private int _scriptId;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _content = string.Empty;

        [ObservableProperty]
        private string _wordCountText = "0 words";

        public ScriptEditorViewModel(DatabaseService databaseService, IFileExportService fileExportService)
        {
            _databaseService = databaseService;
            _fileExportService = fileExportService;
        }

        partial void OnContentChanged(string value)
        {
            UpdateWordCount();
        }

        public async Task LoadScriptAsync(int scriptId)
        {
            if (scriptId == 0)
            {
                _script = new ScriptModel();
                ScriptId = 0;
                Title = string.Empty;
                Content = string.Empty;
                return;
            }

            var script = await _databaseService.GetScriptAsync(scriptId);
            if (script is null)
            {
                _script = new ScriptModel();
                ScriptId = 0;
                Title = string.Empty;
                Content = string.Empty;
                return;
            }

            _script = script;
            ScriptId = script.Id;
            Title = script.Title;
            Content = script.Content;
            UpdateWordCount();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var scriptId = query.TryGetValue("ScriptId", out var value) && value is int id
                ? id
                : 0;

            _ = LoadScriptAsync(scriptId);
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (_script is null)
                _script = new ScriptModel();

            _script.Title = string.IsNullOrWhiteSpace(Title) ? "Untitled script" : Title.Trim();
            _script.Content = Content ?? string.Empty;

            await _databaseService.SaveScriptAsync(_script);
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task ExportAsync()
        {
            if (string.IsNullOrWhiteSpace(Content))
            {
                await Shell.Current.DisplayAlertAsync("Export script", "There is no content to export.", "OK");
                return;
            }

            var choice = await Shell.Current.DisplayActionSheetAsync(
                "Export script",
                "Cancel",
                null,
                "Text (.txt)",
                "PDF (.pdf)");

            switch (choice)
            {
                case "Text (.txt)":
                    await _fileExportService.ExportTxtAsync(Title, Content);
                    break;
                case "PDF (.pdf)":
                    await _fileExportService.ExportPdfAsync(Title, Content);
                    break;
            }
        }

        private void UpdateWordCount()
        {
            var count = string.IsNullOrWhiteSpace(Content)
                ? 0
                : Content.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;

            WordCountText = $"{count} word{(count == 1 ? string.Empty : "s")}";
        }
    }
}
