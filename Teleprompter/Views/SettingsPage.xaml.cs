using Teleprompter.ViewModels;

namespace Teleprompter.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                Command = new Command(async () => await Shell.Current.GoToAsync(".."))
            });
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            _ = _viewModel.InitializeAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _ = _viewModel.FlushPersistAsync();
        }
    }
}
