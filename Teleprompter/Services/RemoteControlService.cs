namespace Teleprompter.Services
{
    public enum RemoteKey
    {
        PlayPause,
        SpeedUp,
        SpeedDown,
    }

    public interface IRemoteControlService
    {
        event EventHandler<RemoteKey>? KeyPressed;
        void Start();
        void Stop();
    }

    public partial class RemoteControlService : IRemoteControlService
    {
        public event EventHandler<RemoteKey>? KeyPressed;

#if WINDOWS
        private Microsoft.UI.Xaml.FrameworkElement? _root;

        public void Start()
        {
            var window = Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            if (window?.Content is Microsoft.UI.Xaml.FrameworkElement root)
            {
                _root = root;
                _root.KeyDown += OnRootKeyDown;
            }
        }

        public void Stop()
        {
            if (_root is not null)
                _root.KeyDown -= OnRootKeyDown;
            _root = null;
        }

        private void OnRootKeyDown(object? sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Space:
                    e.Handled = true;
                    KeyPressed?.Invoke(this, RemoteKey.PlayPause);
                    break;
                case Windows.System.VirtualKey.Up:
                case Windows.System.VirtualKey.Right:
                    e.Handled = true;
                    KeyPressed?.Invoke(this, RemoteKey.SpeedUp);
                    break;
                case Windows.System.VirtualKey.Down:
                case Windows.System.VirtualKey.Left:
                    e.Handled = true;
                    KeyPressed?.Invoke(this, RemoteKey.SpeedDown);
                    break;
            }
        }
#elif !ANDROID
        public void Start() { }
        public void Stop() { }
#endif
    }
}
