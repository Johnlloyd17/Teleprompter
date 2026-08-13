using Teleprompter.Views;

namespace Teleprompter
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ScriptEditorPage), typeof(ScriptEditorPage));
            Routing.RegisterRoute(nameof(TeleprompterPlayerPage), typeof(TeleprompterPlayerPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        }
    }
}
