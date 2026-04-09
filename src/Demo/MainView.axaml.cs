using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

namespace Demo;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnIconTapped(object sender, TappedEventArgs e)
    {
        if (sender is Control { Tag: IconEntry entry } control)
        {
            var dock = control.FindAncestorOfType<DockPanel>();
            if (dock?.DataContext is IconBrowserViewModel vm)
            {
                vm.SelectedIcon = entry;
            }
        }
    }

    private async void OnCopyClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Control control)
            return;

        var dock = control.FindAncestorOfType<DockPanel>();
        if (dock?.DataContext is IconBrowserViewModel { SelectedIcon: { } icon })
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard is not null)
            {
                await clipboard.SetTextAsync(icon.Key);
            }
        }
    }
}
