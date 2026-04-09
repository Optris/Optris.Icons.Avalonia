[Home](README.md) | [Getting Started](getting-started.md) | [Usage Guide](usage-guide.md) | [Icon Packs](icon-packs.md) | **Custom Providers** | [API Reference](api-reference.md) | [Troubleshooting](troubleshooting.md)

# Custom Icon Providers

You can extend the library with your own icon sources by implementing the `IIconProvider` interface.

## Interfaces

```csharp
public interface IIconReader
{
    IconModel GetIcon(string value);
}

public interface IIconProvider : IIconReader
{
    string Prefix { get; }
}
```

- `Prefix` is used to route icon strings to the correct provider. It must be unique across all registered providers.
- `GetIcon` receives the full icon value string (e.g. `"myicons-star"`) and returns an `IconModel`.

## Model types

Icons are represented as SVG path data:

```csharp
// The icon: a viewBox and an SVG path
public record IconModel(ViewBoxModel ViewBox, PathModel Path);

// SVG viewBox dimensions
public record ViewBoxModel(int X, int Y, int Width, int Height);

// SVG path string (implicitly converts to string)
public readonly record struct PathModel(string path);
```

`ViewBoxModel` also has a static `Parse(string)` method that accepts the standard `"x y width height"` format.

## Step-by-step implementation

### 1. Create your provider class

```csharp
using System.Collections.Generic;
using Optris.Icons.Avalonia;
using Optris.Icons.Avalonia.Models;

public class MyIconProvider : IIconProvider
{
    private readonly Dictionary<string, IconModel> _icons = new()
    {
        ["myicons-star"] = new IconModel(
            new ViewBoxModel(0, 0, 24, 24),
            new PathModel("M12 2l3.09 6.26L22 9.27l-5 4.87L18.18 22 12 18.27 5.82 22 7 14.14l-5-4.87 6.91-1.01L12 2z")),
    };

    public string Prefix => "myicons";

    public IconModel GetIcon(string value)
    {
        if (_icons.TryGetValue(value, out var icon))
            return icon;

        throw new KeyNotFoundException($"Icon \"{value}\" not found in MyIconProvider.");
    }
}
```

### 2. Register the provider

Using the generic overload (requires a parameterless constructor):

```csharp
IconProvider.Current.Register<MyIconProvider>();
```

Or with a pre-constructed instance:

```csharp
var provider = new MyIconProvider(/* custom arguments */);
IconProvider.Current.Register(provider);
```

### 3. Use your icons

```xml
<i:Icon Value="myicons-star" FontSize="24" />
```

## Prefix rules

- Prefixes are matched case-insensitively using `StartsWith`.
- A prefix must not be a prefix of (or be prefixed by) any already-registered provider's prefix. For example, you cannot register `"fa"` and `"fab"` simultaneously.
- Attempting to register a conflicting prefix throws an `ArgumentException`.

## Advanced: custom Font Awesome icon sets

To use Font Awesome Pro or a custom subset, implement `IFontAwesomeUtf8JsonStreamProvider`:

```csharp
using System.IO;
using Optris.Icons.Avalonia.FontAwesome;

public class MyFontAwesomeStreamProvider : IFontAwesomeUtf8JsonStreamProvider
{
    public Stream GetUtf8JsonStream()
    {
        // Return a stream to your FA-format icons.json
        return File.OpenRead("path/to/my-icons.json");
    }
}
```

Then register with the custom stream provider:

```csharp
var provider = new FontAwesomeIconProvider(new MyFontAwesomeStreamProvider());
IconProvider.Current.Register(provider);
```

The JSON format must match Font Awesome's `icons.json` structure.

## See also

- [API Reference](api-reference.md) -- full type reference
- [Icon Packs](icon-packs.md) -- built-in icon providers
