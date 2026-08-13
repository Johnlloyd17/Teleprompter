using Teleprompter.ViewModels;

namespace Teleprompter.Views
{
    public partial class ScriptEditorPage : ContentPage
    {
        public ScriptEditorPage(ScriptEditorViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
