#if !NET10_0_OR_GREATER
namespace Teleprompter.Services
{
    // .NET 10 added the async DisplayAlertAsync / DisplayActionSheetAsync APIs.
    // These extensions provide the same members on older target frameworks so
    // call sites remain identical no matter which framework version is built.
    internal static class MauiCompatibilityExtensions
    {
        public static Task DisplayAlertAsync(this Shell shell, string title, string message, string cancel)
        {
            return shell.DisplayAlert(title, message, cancel);
        }

        public static Task<bool> DisplayAlertAsync(this Shell shell, string title, string message, string accept, string cancel)
        {
            return shell.DisplayAlert(title, message, accept, cancel);
        }

        public static Task<string?> DisplayActionSheetAsync(this Shell shell, string title, string? cancel, string? destruction, params string[] buttons)
        {
            return shell.DisplayActionSheet(title, cancel, destruction, buttons);
        }
    }
}
#endif
