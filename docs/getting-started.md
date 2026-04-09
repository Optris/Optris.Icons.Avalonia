[Home](README.md) | **Getting Started** | [Usage Guide](usage-guide.md) | [Icon Packs](icon-packs.md) | [Custom Providers](custom-providers.md) | [API Reference](api-reference.md) | [Troubleshooting](troubleshooting.md)

# Getting Started

This guide walks you through adding icons to an Avalonia application using Optris.Icons.Avalonia.

## Prerequisites

- .NET 8, 9, or 10
- An [Avalonia](https://avaloniaui.net/) 12 project

## Installation

Install the core package and at least one icon provider via NuGet:

```bash
dotnet add package Optris.Icons.Avalonia
dotnet add package Optris.Icons.Avalonia.FontAwesome
dotnet add package Optris.Icons.Avalonia.MaterialDesign
```

You only need the icon packs you plan to use. Most projects use Font Awesome, Material Design, or both.

## Register icon providers

Register the icon providers **before** building the Avalonia app, typically in `Program.cs`:

```csharp
using Optris.Icons.Avalonia;
using Optris.Icons.Avalonia.FontAwesome;
using Optris.Icons.Avalonia.MaterialDesign;

IconProvider.Current
    .Register<FontAwesomeIconProvider>()
    .Register<MaterialDesignIconProvider>();
```

This tells the library which icon packs are available at runtime.

## Add the XAML namespace

In any AXAML file where you want to use icons, add this namespace:

```xml
xmlns:i="https://github.com/projektanker/icons.avalonia"
```

> **Note:** This URL is an opaque XML namespace identifier, not a clickable web link. It is preserved from the original project for backward compatibility.

## Display your first icon

```xml
<i:Icon Value="fa-solid fa-check" FontSize="24" />
```

This renders a Font Awesome solid check icon at 24px.

For Material Design:

```xml
<i:Icon Value="mdi-check" FontSize="24" />
```

## Verify it works

Run your app and confirm the icon renders. If you see an error about no registered provider, make sure the `Register` calls in `Program.cs` run before `AppBuilder.Configure<App>()`.

Try the [live demo](https://optris.github.io/Optris.Icons.Avalonia/) to browse icons interactively.

## Next steps

- [Usage Guide](usage-guide.md) -- all the ways to use icons (controls, attached properties, images, animations)
- [Icon Packs](icon-packs.md) -- browse available icons and understand naming conventions
- [Custom Providers](custom-providers.md) -- implement your own icon source
