# Optris.Icons.Avalonia

> **Optris-maintained fork.** The [original Projektanker.Icons.Avalonia](https://github.com/Projektanker/Icons.Avalonia) is unmaintained — Projektanker GmbH has been dissolved and PRs to the upstream repo will not be reviewed. Optris GmbH has adopted this project to keep it alive for the Avalonia community, starting with an Avalonia 12 port.
>
> Published to NuGet as **`Optris.Icons.Avalonia`**, **`Optris.Icons.Avalonia.FontAwesome`**, and **`Optris.Icons.Avalonia.MaterialDesign`**. The C# namespace has been renamed from `Projektanker.Icons.Avalonia` to `Optris.Icons.Avalonia` (the `Projektanker` name was misleading post-dissolution). The XAML XML namespace URL `https://github.com/projektanker/icons.avalonia` is preserved as an opaque identifier so existing consumer XAML keeps working without changes.
>

---

A library to easily display icons in an Avalonia App.

**[Live Demo](https://optris.github.io/Optris.Icons.Avalonia/)** — try it in your browser

[![🧪 Test](https://github.com/Optris/Optris.Icons.Avalonia/actions/workflows/push.yml/badge.svg)](https://github.com/Optris/Optris.Icons.Avalonia/actions/workflows/push.yml)
[![🔄 Sync FontAwesome](https://github.com/Optris/Optris.Icons.Avalonia/actions/workflows/sync-fontawesome.yml/badge.svg)](https://github.com/Optris/Optris.Icons.Avalonia/actions/workflows/sync-fontawesome.yml)
[![🔄 Sync Material Design](https://github.com/Optris/Optris.Icons.Avalonia/actions/workflows/sync-materialdesign.yml/badge.svg)](https://github.com/Optris/Optris.Icons.Avalonia/actions/workflows/sync-materialdesign.yml)

## NuGet

| Name                                                                                                                  | Description                                                     | Version                                                                      |
| :-------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------------- | :--------------------------------------------------------------------------- |
| [Optris.Icons.Avalonia](https://www.nuget.org/packages/Optris.Icons.Avalonia/)                                        | Core library                                                    | ![Nuget](https://badgen.net/nuget/v/Optris.Icons.Avalonia)                  |
| [Optris.Icons.Avalonia.FontAwesome](https://www.nuget.org/packages/Optris.Icons.Avalonia.FontAwesome/)                | [Font Awesome 6 Free](https://fontawesome.com)                  | ![Nuget](https://badgen.net/nuget/v/Optris.Icons.Avalonia.FontAwesome)      |
| [Optris.Icons.Avalonia.MaterialDesign](https://www.nuget.org/packages/Optris.Icons.Avalonia.MaterialDesign/)          | [Material Design Icons](https://pictogrammers.com/library/mdi/) | ![Nuget](https://badgen.net/nuget/v/Optris.Icons.Avalonia.MaterialDesign)   |

## Icon providers

| Name           | Prefix | Example      |
| :------------- | :----: | :----------- |
| FontAwesome 6  |  `fa`  | `fa-github`  |
| MaterialDesign | `mdi`  | `mdi-github` |

## Usage

A full example is available in the [Demo project](src/Demo/) ([live](https://optris.github.io/Optris.Icons.Avalonia/)).

### 1. Register icon providers on app start up

Register the icon provider(s) with the `IconProvider.Current`.

```csharp
using Avalonia;
using Optris.Icons.Avalonia;
using Optris.Icons.Avalonia.FontAwesome;
using Optris.Icons.Avalonia.MaterialDesign;

namespace Demo.Desktop;

internal static class Program
{
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current
            .Register<FontAwesomeIconProvider>()
            .Register<MaterialDesignIconProvider>();

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    }
}
```

### 2. Add xml namespace

Add `xmlns:i="https://github.com/projektanker/icons.avalonia"` to your view.

### 3. Use the icon

**Standalone**

```xml
<i:Icon Value="fa-solid fa-anchor" />
```

**Attached to ContentControl (e.g. Button)**

```xml
<Button i:Attached.Icon="fa-solid fa-anchor" />
```

**Attached to MenuItem**

```xml
<MenuItem Header="About" i:MenuItem.Icon="fa-solid fa-circle-info" />
```

**Custom icon size**

```xml
<i:Icon Value="fa-solid fa-anchor" FontSize="24" />
```

**Animated**

```xml
<i:Icon Value="fa-spinner" Animation="Pulse" />
<i:Icon Value="fa-sync" Animation="Spin" />
```

**As an [Image](https://docs.avaloniaui.net/docs/reference/controls/image) source**

```xml
<Image>
  <Image.Source>
    <i:IconImage Value="fa-solid fa-anchor" Brush="(defaults to black)" />
  </Image.Source>
</Image>
```

### Done

![Screenshot](/resources/demo.png?raw=true)

## Implement your own Icon Provider

Just implement the `IIconProvider` interface:

```csharp
namespace Optris.Icons.Avalonia;

public interface IIconReader
{
    IconModel GetIcon(string value);
}

public interface IIconProvider : IIconReader
{
    string Prefix { get; }
}
```

and register it with the `IIconProviderContainer`:

```csharp
IconProvider.Current.Register<MyCustomIconProvider>()
```

or

```csharp
IIconProvider provider = new MyCustomIconProvider(/* custom ctor arguments */);
IconProvider.Current.Register(provider);
```

The `IIconProvider.Prefix` property has to be unique within all registered providers. It is used to select the right provider. E.g. `FontAwesomeIconProvider`'s prefix is `fa`.

## Releasing a new version

The major version tracks Avalonia (e.g. `12.x.y` for Avalonia 12). Minor and patch versions are for library changes.

1. Tag the release: `git tag -a v12.0.1 -m "v12.0.1"`
2. Push the tag: `git push origin v12.0.1`

The [release workflow](.github/workflows/release.yml) extracts the version from the tag, packs all three packages with it, and publishes to nuget.org. No need to edit version numbers in source files.
