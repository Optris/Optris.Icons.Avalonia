using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Demo;
using Optris.Icons.Avalonia;
using Optris.Icons.Avalonia.FontAwesome;
using Optris.Icons.Avalonia.FontAwesome7;
using Optris.Icons.Avalonia.MaterialDesign;

[assembly: SupportedOSPlatform("browser")]

internal sealed partial class Program
{
    private static Task Main(string[] args) => BuildAvaloniaApp()
            .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current
            .Register<FontAwesomeIconProvider>()
            .Register<FontAwesome7IconProvider>()
            .Register<MaterialDesignIconProvider>();

        return AppBuilder.Configure<App>();
    }
}
