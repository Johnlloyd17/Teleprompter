using Teleprompter.ViewModels;

namespace Teleprompter.Views
{
    public partial class ScriptListPage : ContentPage
    {
        private readonly ScriptListViewModel _viewModel;

        public ScriptListPage(ScriptListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            _ = _viewModel.InitializeAsync();
        }
    }
}
