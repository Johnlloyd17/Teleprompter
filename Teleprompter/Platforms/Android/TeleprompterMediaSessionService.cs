using Android.App;
using Android.Content;
using Android.Media.Session;
using Android.OS;
using AndroidX.Core.App;
using Teleprompter.Services;

namespace Teleprompter.Platforms.Android
{
#pragma warning disable CA1416
    [Service(Enabled = true, Exported = false, ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeMediaPlayback)]
    public class TeleprompterMediaSessionService : Service
    {
        private const int NotificationId = 1001;
        private const string ChannelId = "teleprompter_remote_control";

        public static MediaSession? CurrentSession { get; private set; }

        private MediaSession? _session;
        private bool _foreground;

        public override void OnCreate()
        {
            base.OnCreate();
            CreateNotificationChannel();
            EnsureForeground();

            _session = new MediaSession(this, "TeleprompterRemoteControl");
            CurrentSession = _session;
            _session.SetFlags(MediaSession.FlagHandlesMediaButtons | MediaSession.FlagHandlesTransportControls);
            _session.SetCallback(new TeleprompterCallback());
            _session.Active = true;
            SetMediaButtonReceiver();
        }

        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            EnsureForeground();
            return StartCommandResult.Sticky;
        }

        public override IBinder? OnBind(Intent? intent) => null;

        public override void OnDestroy()
        {
            _session?.Release();
            _session = null;
            CurrentSession = null;
            base.OnDestroy();
        }

        private void SetMediaButtonReceiver()
        {
            if (_session is null)
                return;

            var intent = new Intent(this, typeof(TeleprompterMediaButtonReceiver));
            intent.SetAction(Intent.ActionMediaButton);
            var pending = PendingIntent.GetBroadcast(this, 0, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            _session.SetMediaButtonReceiver(pending);
        }

        private void EnsureForeground()
        {
            if (_foreground)
                return;

            var notification = new NotificationCompat.Builder(this, ChannelId)
                .SetContentTitle("Teleprompter")
                .SetContentText("Remote control active")
                .SetSmallIcon(global::Android.Resource.Drawable.IcMediaPlay)
                .SetOngoing(true)
                .Build();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                StartForeground(NotificationId, notification, global::Android.Content.PM.ForegroundService.TypeMediaPlayback);
            else
                StartForeground(NotificationId, notification);
            _foreground = true;
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            var channel = new NotificationChannel(ChannelId, "Teleprompter remote control", NotificationImportance.Low);
            var manager = (NotificationManager?)GetSystemService(NotificationService);
            manager?.CreateNotificationChannel(channel);
        }
    }

    internal class TeleprompterCallback : MediaSession.Callback
    {
        public override void OnPlay() => Dispatch(RemoteKey.PlayPause);
        public override void OnPause() => Dispatch(RemoteKey.PlayPause);
        public override void OnSkipToNext() => Dispatch(RemoteKey.SpeedUp);
        public override void OnSkipToPrevious() => Dispatch(RemoteKey.SpeedDown);

        private static void Dispatch(RemoteKey key) => RemoteControlService.Instance?.HandleKey(key);
    }
#pragma warning restore CA1416
}
