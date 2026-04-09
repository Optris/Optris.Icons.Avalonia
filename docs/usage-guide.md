[Home](README.md) | [Getting Started](getting-started.md) | **Usage Guide** | [Icon Packs](icon-packs.md) | [Custom Providers](custom-providers.md) | [API Reference](api-reference.md) | [Troubleshooting](troubleshooting.md)

# Usage Guide

This guide covers all the ways to display and customize icons. See [Getting Started](getting-started.md) for initial setup.

## Icon control

The `Icon` control is the primary way to display an icon:

```xml
<i:Icon Value="fa-solid fa-anchor" />
```

### Sizing

Icon size is controlled by `FontSize` (inherited from `TemplatedControl`):

```xml
<i:Icon Value="fa-solid fa-anchor" FontSize="32" />
```

If not set, `FontSize` is inherited from the parent control.

### Color

Use the `Foreground` property:

```xml
<i:Icon Value="fa-solid fa-anchor" Foreground="DodgerBlue" />
```

## Attached property on Button / ContentControl

Use `Attached.Icon` to add an icon to any `ContentControl` (Button, ToggleButton, etc.):

```xml
<Button i:Attached.Icon="fa-solid fa-save" />
```

This replaces the button's `Content` with an `Icon` control. The icon inherits the button's `Foreground` and `FontSize`.

## Attached property on MenuItem

Use `MenuItem.Icon` to set the icon on a menu item:

```xml
<MenuItem Header="Settings" i:MenuItem.Icon="fa-solid fa-gear" />
```

## IconImage (Image source)

Use `IconImage` as the source of an Avalonia `Image` control for more control over rendering:

```xml
<Image Width="48" Height="48">
  <Image.Source>
    <i:IconImage Value="fa-solid fa-anchor" Brush="DodgerBlue" />
  </Image.Source>
</Image>
```

`IconImage` properties:
- `Value` -- the icon identifier string
- `Brush` -- fill color (defaults to black)
- `Pen` -- stroke pen (defaults to no stroke)

## Animations

Two built-in animations are available via the `Animation` property:

### Spin

Smooth continuous 360-degree rotation (2-second loop):

```xml
<i:Icon Value="fa-solid fa-sync" Animation="Spin" />
```

### Pulse

Stepped rotation in 8 discrete increments (2-second loop), giving a "ticking" effect:

```xml
<i:Icon Value="fa-solid fa-spinner" Animation="Pulse" />
```

## Styling with Avalonia selectors

You can style icons using standard Avalonia style selectors:

```xml
<Style Selector="i|Icon">
  <Setter Property="FontSize" Value="20" />
  <Setter Property="Foreground" Value="Gray" />
</Style>
```

Target specific animations:

```xml
<Style Selector="i|Icon[Animation=Spin]">
  <Setter Property="Foreground" Value="Green" />
</Style>
```

## Data binding

Bind the `Value` property to dynamically change icons:

```xml
<i:Icon Value="{Binding CurrentIcon}" FontSize="24" />
```

```csharp
public class MyViewModel : INotifyPropertyChanged
{
    private string _currentIcon = "fa-solid fa-play";

    public string CurrentIcon
    {
        get => _currentIcon;
        set { _currentIcon = value; OnPropertyChanged(); }
    }

    // Toggle between play and pause
    public void Toggle()
    {
        CurrentIcon = CurrentIcon.Contains("play")
            ? "fa-solid fa-pause"
            : "fa-solid fa-play";
    }
}
```

## See also

- [Icon Packs](icon-packs.md) -- naming conventions and how to browse available icons
- [API Reference](api-reference.md) -- full list of classes and properties
