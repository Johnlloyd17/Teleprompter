using Teleprompter.ViewModels;

namespace Teleprompter.Views
{
    public partial class TeleprompterPlayerPage : ContentPage
    {
        private readonly TeleprompterPlayerViewModel _viewModel;
        private readonly BackButtonBehavior _backButtonBehavior;

        public TeleprompterPlayerPage(TeleprompterPlayerViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
            _viewModel.ActionRequested += OnActionRequested;

            _backButtonBehavior = new BackButtonBehavior
            {
                Command = new Command(async () => await Shell.Current.GoToAsync(".."))
            };
            Shell.SetBackButtonBehavior(this, _backButtonBehavior);
        }

        private void OnActionRequested(object? sender, PlaybackAction action)
        {
            switch (action)
            {
                case PlaybackAction.Play:
                    PlayerView.Play();
                    break;
                case PlaybackAction.Pause:
                    PlayerView.Pause();
                    _ = _viewModel.FlushPersistAsync();
                    break;
                case PlaybackAction.Stop:
                    PlayerView.Stop();
                    _ = _viewModel.FlushPersistAsync();
                    break;
                case PlaybackAction.Rewind:
                    PlayerView.SeekBy(-5);
                    break;
                case PlaybackAction.Forward:
                    PlayerView.SeekBy(5);
                    break;
            }
        }

        private void OnProgressChanged(object? sender, double progress) =>
            _viewModel.Progress = progress;

        private void OnIsPlayingChanged(object? sender, bool isPlaying) =>
            _viewModel.IsPlaying = isPlaying;

        private void OnSwipeUp(object? sender, SwipedEventArgs e)
        {
            var secondsPerWord = 60.0 / Math.Max(1, PlayerView.HighlightWpm);
            PlayerView.SmoothSeekBy(secondsPerWord);
        }

        private void OnSwipeDown(object? sender, SwipedEventArgs e)
        {
            var secondsPerWord = 60.0 / Math.Max(1, PlayerView.HighlightWpm);
            PlayerView.SmoothSeekBy(-secondsPerWord);
        }

        private void OnReadingPanelTapped(object? sender, TappedEventArgs e)
        {
            var position = e.GetPosition(PlayerView);
            if (position is null)
                return;

            var halfWidth = PlayerView.Width / 2;
            var secondsPerWord = 60.0 / Math.Max(1, PlayerView.HighlightWpm);
            PlayerView.SmoothSeekBy(position.Value.X > halfWidth ? secondsPerWord : -secondsPerWord);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.StartRemoteControl();
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.StopRemoteControl();
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _ = _viewModel.FlushPersistAsync();
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(_viewModel.IsFullScreen))
                return;

            _backButtonBehavior.IsEnabled = !_viewModel.IsFullScreen;

            if (_viewModel.IsFullScreen)
            {
#if ANDROID
                var activity = Platform.CurrentActivity;
                if (activity is not null)
                {
                    var cfg = activity.Resources?.Configuration;
                    var isLandscape = cfg?.Orientation == global::Android.Content.Res.Orientation.Landscape;
                    activity.RequestedOrientation = (global::Android.Content.PM.ScreenOrientation)(isLandscape ? 6 : 1);
                }
#endif
            }
            else
            {
#if ANDROID
                var activity = Platform.CurrentActivity;
                if (activity is not null)
                    activity.RequestedOrientation = (global::Android.Content.PM.ScreenOrientation)(-1);
#endif
            }

            Dispatcher.Dispatch(() => PlayerView.RequestRefit());
        }

        private void OnExitFullScreenClicked(object? sender, EventArgs e) =>
            _viewModel.IsFullScreen = false;
    }
}
