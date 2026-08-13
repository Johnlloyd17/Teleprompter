using System.Globalization;
using SQLite;
using Teleprompter.Models;

namespace Teleprompter.Data
{
    public class DatabaseService
    {
        private const string DbFileName = "teleprompter.db3";

        private SQLiteAsyncConnection? _connection;

        private async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            if (_connection is not null)
                return _connection;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, DbFileName);
            var connection = new SQLiteAsyncConnection(databasePath);

            await connection.CreateTableAsync<ScriptModel>();
            await connection.CreateTableAsync<FolderModel>();
            await connection.CreateTableAsync<ScriptSettingsModel>();
            await connection.CreateTableAsync<AppSettingModel>();
            await connection.CreateTableAsync<PlaybackHistoryModel>();

            _connection = connection;
            return connection;
        }

        #region Scripts

        public async Task<List<ScriptModel>> GetScriptsAsync()
        {
            var connection = await GetConnectionAsync();
            return await connection.Table<ScriptModel>()
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();
        }

        public async Task<ScriptModel?> GetScriptAsync(int id)
        {
            var connection = await GetConnectionAsync();
            return await connection.FindAsync<ScriptModel>(id);
        }

        public async Task<int> SaveScriptAsync(ScriptModel script)
        {
            var connection = await GetConnectionAsync();
            script.UpdatedAt = DateTime.Now;
            script.WordCount = CountWords(script.Content);

            if (script.Id == 0)
            {
                script.CreatedAt = DateTime.Now;
                return await connection.InsertAsync(script);
            }

            await connection.UpdateAsync(script);
            return script.Id;
        }

        public async Task<int> DeleteScriptAsync(ScriptModel script)
        {
            var connection = await GetConnectionAsync();
            await connection.ExecuteAsync("DELETE FROM PlaybackHistory WHERE ScriptId = ?", script.Id);
            await connection.ExecuteAsync("DELETE FROM ScriptSettings WHERE ScriptId = ?", script.Id);
            return await connection.DeleteAsync(script);
        }

        #endregion

        #region Folders

        public async Task<List<FolderModel>> GetFoldersAsync()
        {
            var connection = await GetConnectionAsync();
            return await connection.Table<FolderModel>()
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<int> SaveFolderAsync(FolderModel folder)
        {
            var connection = await GetConnectionAsync();
            if (folder.Id == 0)
            {
                folder.CreatedAt = DateTime.Now;
                return await connection.InsertAsync(folder);
            }

            await connection.UpdateAsync(folder);
            return folder.Id;
        }

        public async Task<int> DeleteFolderAsync(FolderModel folder)
        {
            var connection = await GetConnectionAsync();
            return await connection.DeleteAsync(folder);
        }

        #endregion

        #region Script Settings

        public async Task<ScriptSettingsModel?> GetScriptSettingsAsync(int scriptId)
        {
            var connection = await GetConnectionAsync();
            return await connection.Table<ScriptSettingsModel>()
                .Where(s => s.ScriptId == scriptId)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveScriptSettingsAsync(ScriptSettingsModel settings)
        {
            var connection = await GetConnectionAsync();
            var existing = await connection.Table<ScriptSettingsModel>()
                .Where(s => s.ScriptId == settings.ScriptId)
                .FirstOrDefaultAsync();

            if (existing is null)
                return await connection.InsertAsync(settings);

            settings.Id = existing.Id;
            await connection.UpdateAsync(settings);
            return settings.Id;
        }

        #endregion

        #region Playback History

        public async Task<PlaybackHistoryModel?> GetLatestPlaybackHistoryAsync()
        {
            var connection = await GetConnectionAsync();
            return await connection.Table<PlaybackHistoryModel>()
                .OrderByDescending(h => h.LastOpenedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<PlaybackHistoryModel?> GetPlaybackHistoryAsync(int scriptId)
        {
            var connection = await GetConnectionAsync();
            return await connection.Table<PlaybackHistoryModel>()
                .Where(h => h.ScriptId == scriptId)
                .FirstOrDefaultAsync();
        }

        public async Task SavePlaybackHistoryAsync(PlaybackHistoryModel history)
        {
            var connection = await GetConnectionAsync();
            var existing = await connection.Table<PlaybackHistoryModel>()
                .Where(h => h.ScriptId == history.ScriptId)
                .FirstOrDefaultAsync();

            if (existing is null)
            {
                await connection.InsertAsync(history);
            }
            else
            {
                history.Id = existing.Id;
                await connection.UpdateAsync(history);
            }
        }

        #endregion

        #region App Settings

        public async Task<string?> GetSettingAsync(string key)
        {
            var connection = await GetConnectionAsync();
            var setting = await connection.FindAsync<AppSettingModel>(key);
            return setting?.Value;
        }

        public async Task SetSettingAsync(string key, string value)
        {
            var connection = await GetConnectionAsync();
            var setting = await connection.FindAsync<AppSettingModel>(key);

            if (setting is null)
            {
                await connection.InsertAsync(new AppSettingModel { Key = key, Value = value });
            }
            else
            {
                setting.Value = value;
                await connection.UpdateAsync(setting);
            }
        }

        #endregion

        #region Global Defaults

        public async Task<GlobalDefaults> GetGlobalDefaultsAsync()
        {
            var defaults = new GlobalDefaults
            {
                Speed = double.TryParse(await GetSettingAsync(AppSettingKeys.DefaultSpeed), out var speed) ? speed : 60,
                FontSize = int.TryParse(await GetSettingAsync(AppSettingKeys.DefaultFontSize), out var fontSize) ? fontSize : 28,
                MirrorMode = bool.TryParse(await GetSettingAsync(AppSettingKeys.DefaultMirrorMode), out var mirror) && mirror,
                HighlightEnabled = bool.TryParse(await GetSettingAsync(AppSettingKeys.DefaultHighlightEnabled), out var highlight) && highlight,
                HighlightWpm = double.TryParse(await GetSettingAsync(AppSettingKeys.DefaultHighlightWpm), out var wpm) ? wpm : 180,
                HighlightColor = Color.TryParse(await GetSettingAsync(AppSettingKeys.DefaultHighlightColor), out var highlightColor)
                    ? highlightColor
                    : Colors.Yellow,
            };

            if (Color.TryParse(await GetSettingAsync(AppSettingKeys.DefaultTextColor), out var textColor))
                defaults.TextColor = textColor;

            if (Color.TryParse(await GetSettingAsync(AppSettingKeys.DefaultBackgroundColor), out var backgroundColor))
                defaults.BackgroundColor = backgroundColor;

            return defaults;
        }

        public async Task SaveGlobalDefaultsAsync(GlobalDefaults defaults)
        {
            await SetSettingAsync(AppSettingKeys.DefaultSpeed, defaults.Speed.ToString(CultureInfo.InvariantCulture));
            await SetSettingAsync(AppSettingKeys.DefaultFontSize, defaults.FontSize.ToString(CultureInfo.InvariantCulture));
            await SetSettingAsync(AppSettingKeys.DefaultMirrorMode, defaults.MirrorMode.ToString());
            await SetSettingAsync(AppSettingKeys.DefaultTextColor, defaults.TextColor.ToHex());
            await SetSettingAsync(AppSettingKeys.DefaultBackgroundColor, defaults.BackgroundColor.ToHex());
            await SetSettingAsync(AppSettingKeys.DefaultHighlightEnabled, defaults.HighlightEnabled.ToString());
            await SetSettingAsync(AppSettingKeys.DefaultHighlightWpm, defaults.HighlightWpm.ToString(CultureInfo.InvariantCulture));
            await SetSettingAsync(AppSettingKeys.DefaultHighlightColor, defaults.HighlightColor.ToHex());
        }

        #endregion

        private static int CountWords(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return 0;

            return content.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }
}
