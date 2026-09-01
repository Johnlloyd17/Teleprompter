using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

using WinBorder = Microsoft.UI.Xaml.Controls.Border;
using WinBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using WinButton = Microsoft.UI.Xaml.Controls.Button;
using WinControl = Microsoft.UI.Xaml.Controls.Control;
using WinPanel = Microsoft.UI.Xaml.Controls.Panel;

namespace Teleprompter.Platforms.Windows;

public class CustomShellHandler : ShellHandler
{
    protected override void ConnectHandler(ShellView platformView)
    {
        base.ConnectHandler(platformView);

        ApplyBackButtonColor(platformView);

        platformView.Loaded += (_, _) => ApplyBackButtonColor(platformView);
        platformView.ActualThemeChanged += (sender, _) =>
        {
            if (sender is ShellView navView)
            {
                ApplyBackButtonColor(navView);
            }
        };

        var shell = this.VirtualView;
        shell.Navigated += (_, _) => ApplyBackButtonColor(platformView);
    }

    private static void ApplyBackButtonColor(ShellView navView, int retry = 0)
    {
        var buttons = FindVisibleBackButtons(navView);
        if (buttons.Count == 0)
        {
            // The toolbar/back button may not be in the visual tree yet (e.g. right after a
            // navigation). Retry a few times on the next UI loop, then give up.
            if (retry < 5)
            {
                var next = retry + 1;
                navView.DispatcherQueue.TryEnqueue(
                    DispatcherQueuePriority.Low,
                    () => ApplyBackButtonColor(navView, next));
            }

            return;
        }

        // Default (normal) color is WHITE; hover is a light grey.
        // (White assumes a dark nav bar. If your nav bar is light, white won't be visible
        // and you'd want to re-add a dark hover background.)
        // Tweak these ARGB values to taste.
        var normal = Microsoft.UI.Colors.White;
        var hoverForeground = Microsoft.UI.ColorHelper.FromArgb(255, 0xD9, 0xD9, 0xD9); // light grey
        var pressedForeground = Microsoft.UI.ColorHelper.FromArgb(255, 0xC0, 0xC0, 0xC0);
        var disabledForeground = Microsoft.UI.ColorHelper.FromArgb(255, 0x99, 0x99, 0x99);
        var transparent = Microsoft.UI.Colors.Transparent;

        foreach (var button in buttons)
        {
            button.Foreground = new WinBrush(normal);
            button.Resources["NavigationViewButtonForeground"] = new WinBrush(normal);
            button.Resources["NavigationViewButtonForegroundPointerOver"] = new WinBrush(hoverForeground);
            button.Resources["NavigationViewButtonForegroundPressed"] = new WinBrush(pressedForeground);
            button.Resources["NavigationViewButtonForegroundDisabled"] = new WinBrush(disabledForeground);
            button.Resources["NavigationViewButtonBackgroundPointerOver"] = new WinBrush(transparent);
            button.Resources["NavigationViewButtonBackgroundPressed"] = new WinBrush(transparent);
        }
    }

    private static List<WinButton> FindVisibleBackButtons(DependencyObject parent)
    {
        var result = new List<WinButton>();

        if (parent == null)
        {
            return result;
        }

        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is WinButton button &&
                button.Name == "NavigationViewBackButton" &&
                button.Visibility == Microsoft.UI.Xaml.Visibility.Visible)
            {
                result.Add(button);
            }

            result.AddRange(FindVisibleBackButtons(child));
        }

        return result;
    }

    private static WinBrush? GetBackgroundColor(DependencyObject element)
    {
        var current = element;
        while (current != null)
        {
            if (current is WinControl control && control.Background is WinBrush controlBrush)
            {
                return controlBrush;
            }

            if (current is WinPanel panel && panel.Background is WinBrush panelBrush)
            {
                return panelBrush;
            }

            if (current is WinBorder border && border.Background is WinBrush borderBrush)
            {
                return borderBrush;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static bool IsDark(WinBrush brush)
    {
        var c = brush.Color;
        var luminance = (0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B) / 255.0;
        return luminance < 0.5;
    }
}
