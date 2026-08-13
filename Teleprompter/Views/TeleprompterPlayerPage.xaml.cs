using Teleprompter.ViewModels;

namespace Teleprompter.Views
{
    public partial class TeleprompterPlayerPage : ContentPage
    {
        private readonly TeleprompterPlayerViewModel _viewModel;

        public TeleprompterPlayerPage(TeleprompterPlayerViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
            _viewModel.ActionRequested += OnActionRequested;
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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.StartRemoteControl();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.StopRemoteControl();
            _ = _viewModel.FlushPersistAsync();
        }
    }
}
