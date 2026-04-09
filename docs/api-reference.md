[Home](README.md) | [Getting Started](getting-started.md) | [Usage Guide](usage-guide.md) | [Icon Packs](icon-packs.md) | [Custom Providers](custom-providers.md) | **API Reference** | [Troubleshooting](troubleshooting.md)

# API Reference

Namespace: `Optris.Icons.Avalonia`
XAML namespace: `xmlns:i="https://github.com/projektanker/icons.avalonia"`

## Controls

### Icon

`TemplatedControl` that displays an icon.

| Property    | Type            | Default        | Description                  |
| :---------- | :-------------- | :------------- | :--------------------------- |
| `Value`     | `string`        | `""`           | Icon identifier (e.g. `"fa-solid fa-check"`) |
| `Animation` | `IconAnimation` | `None`         | Animation mode               |
| `FontSize`  | `double`        | inherited      | Icon size in pixels          |
| `Foreground`| `IBrush`        | `Black`        | Icon fill color              |

### IconImage

`DrawingImage` that renders an icon. Use as `Image.Source`.

| Property | Type     | Default | Description              |
| :------- | :------- | :------ | :----------------------- |
| `Value`  | `string` | `""`    | Icon identifier          |
| `Brush`  | `IBrush` | `Black` | Fill brush               |
| `Pen`    | `IPen`   | none    | Stroke pen (0-width default) |

## Attached Properties

### Attached.Icon

Sets an icon on any `ContentControl` (Button, ToggleButton, etc.). Replaces the control's `Content` with an `Icon` control.

```xml
<Button i:Attached.Icon="fa-solid fa-save" />
```

### MenuItem.Icon

Sets an icon on an Avalonia `MenuItem`.

```xml
<MenuItem Header="Open" i:MenuItem.Icon="fa-solid fa-folder-open" />
```

## Enums

### IconAnimation

| Value   | Description                                      |
| :------ | :----------------------------------------------- |
| `None`  | No animation (default)                           |
| `Spin`  | Smooth 360-degree rotation, 2-second loop        |
| `Pulse` | Stepped rotation (8 discrete steps), 2-second loop |

## Core Classes

### IconProvider

Singleton registry that routes icon requests to the correct provider.

| Member                        | Description                                      |
| :---------------------------- | :----------------------------------------------- |
| `static Current`              | The singleton instance                           |
| `GetIcon(string value)`       | Resolves an icon by prefix-matching a registered provider |
| `Register<T>()`              | Registers a provider by type (requires parameterless constructor) |
| `Register(IIconProvider)`     | Registers a provider instance                    |

## Interfaces

### IIconReader

```csharp
public interface IIconReader
{
    IconModel GetIcon(string value);
}
```

Returns the icon model for the given value. Throws `KeyNotFoundException` if the icon is not found.

### IIconProvider

```csharp
public interface IIconProvider : IIconReader
{
    string Prefix { get; }
}
```

Extends `IIconReader` with a `Prefix` used for routing.

### IIconProviderContainer

```csharp
public interface IIconProviderContainer
{
    IIconProviderContainer Register(IIconProvider iconProvider);
    IIconProviderContainer Register<TIconProvider>() where TIconProvider : IIconProvider, new();
}
```

Registration interface. Returns `this` for fluent chaining.

## Model Types

### IconModel

```csharp
public record IconModel(ViewBoxModel ViewBox, PathModel Path);
```

### ViewBoxModel

```csharp
public record ViewBoxModel(int X, int Y, int Width, int Height)
{
    public static ViewBoxModel Parse(string viewBox); // parses "x y width height"
}
```

### PathModel

```csharp
public readonly record struct PathModel(string path);
```

Wraps an SVG path string. Implicitly converts to `string`.

## Built-in Providers

### FontAwesomeIconProvider

- **Package:** `Optris.Icons.Avalonia.FontAwesome`
- **Prefix:** `fa`
- **Constructors:**
  - `FontAwesomeIconProvider()` -- uses the built-in Free icon set
  - `FontAwesomeIconProvider(IFontAwesomeUtf8JsonStreamProvider)` -- uses a custom icon source

### MaterialDesignIconProvider

- **Package:** `Optris.Icons.Avalonia.MaterialDesign`
- **Prefix:** `mdi`
- **Constructor:** `MaterialDesignIconProvider()` -- loads embedded SVG assets
