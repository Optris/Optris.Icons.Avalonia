using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

namespace Demo;

public partial class MainView : UserControl
{
    private Border _selectedBorder;

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
        if (sender is Border { Tag: IconEntry entry } border)
        {
            var dock = border.FindAncestorOfType<DockPanel>();
            if (dock?.DataContext is IconBrowserViewModel vm)
            {
                vm.SelectedIcon = entry;
                SetSelectedBorder(border);
            }
        }
    }

    private async void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.C && e.KeyModifiers == KeyModifiers.Control)
        {
            if (sender is DockPanel { DataContext: IconBrowserViewModel { SelectedIcon: { } icon } })
            {
                var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                if (clipboard is not null)
                {
                    await clipboard.SetTextAsync(icon.Key);
                    e.Handled = true;
                }
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

    private void SetSelectedBorder(Border border)
    {
        _selectedBorder?.Classes.Remove("selected");
        border.Classes.Add("selected");
        _selectedBorder = border;
    }
}
