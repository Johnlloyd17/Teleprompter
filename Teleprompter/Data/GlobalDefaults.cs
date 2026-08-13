namespace Teleprompter.Data
{
    public static class AppSettingKeys
    {
        public const string DefaultSpeed = "DefaultSpeed";
        public const string DefaultFontSize = "DefaultFontSize";
        public const string DefaultMirrorMode = "DefaultMirrorMode";
        public const string DefaultTextColor = "DefaultTextColor";
        public const string DefaultBackgroundColor = "DefaultBackgroundColor";
        public const string DefaultHighlightEnabled = "DefaultHighlightEnabled";
        public const string DefaultHighlightWpm = "DefaultHighlightWpm";
        public const string DefaultHighlightColor = "DefaultHighlightColor";
        public const string Theme = "Theme";
    }

    public class GlobalDefaults
    {
        public double Speed { get; set; } = 60;

        public int FontSize { get; set; } = 28;

        public bool MirrorMode { get; set; }

        public Color TextColor { get; set; } = Colors.White;

        public Color BackgroundColor { get; set; } = Colors.Black;

        public bool HighlightEnabled { get; set; }

        public double HighlightWpm { get; set; } = 180;

        public Color HighlightColor { get; set; } = Colors.Yellow;
    }
}
