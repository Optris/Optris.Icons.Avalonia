using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;

namespace Demo;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        if (!OperatingSystem.IsBrowser())
            this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var iconStream = AssetLoader.Open(new Uri("avares://Demo/Assets/icon-core.ico"));
            desktop.MainWindow = new Window
            {
                Title = "Icons.Avalonia Demo",
                Icon = new WindowIcon(iconStream),
                Width = 900,
                Height = 700,
                Content = new MainView(),
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new MainView();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
