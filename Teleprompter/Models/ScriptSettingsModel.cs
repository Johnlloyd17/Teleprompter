using SQLite;

namespace Teleprompter.Models
{
    [Table("ScriptSettings")]
    public class ScriptSettingsModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed, NotNull]
        public int ScriptId { get; set; }

        public double ScrollSpeed { get; set; }

        public bool HighlightEnabled { get; set; }

        public double HighlightWpm { get; set; }

        public string? HighlightColor { get; set; }

        public int FontSize { get; set; }

        public string? FontFamily { get; set; }

        public bool MirrorMode { get; set; }

        public string? TextColor { get; set; }

        public string? BackgroundColor { get; set; }

        public string? Alignment { get; set; }
    }
}
