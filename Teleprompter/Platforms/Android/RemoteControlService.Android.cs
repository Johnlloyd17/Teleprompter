using Android.Content;
using AndroidX.Core.Content;
using Teleprompter.Platforms.Android;

namespace Teleprompter.Services
{
    public partial class RemoteControlService
    {
        public static RemoteControlService? Instance { get; private set; }

        public void Start()
        {
            Instance = this;
            var context = Android.App.Application.Context;
            var intent = new Intent(context, typeof(TeleprompterMediaSessionService));
            ContextCompat.StartForegroundService(context, intent);
        }

        public void Stop()
        {
            Instance = null;
            var context = Android.App.Application.Context;
            context.StopService(new Intent(context, typeof(TeleprompterMediaSessionService)));
        }

        public void HandleKey(RemoteKey key)
        {
            KeyPressed?.Invoke(this, key);
        }
    }
}
