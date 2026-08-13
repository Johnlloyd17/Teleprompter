using Android.App;
using Android.Content;
using Android.Views;

namespace Teleprompter.Platforms.Android
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { Intent.ActionMediaButton })]
    public class TeleprompterMediaButtonReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context? context, Intent? intent)
        {
            if (intent?.Action != Intent.ActionMediaButton)
                return;

            var session = TeleprompterMediaSessionService.CurrentSession;
            if (session is null)
                return;

            var keyEvent = intent.GetParcelableExtra(Intent.ExtraKeyEvent) as KeyEvent;
            if (keyEvent is null)
                return;

            session.Controller.DispatchMediaButtonEvent(keyEvent);
        }
    }
}
