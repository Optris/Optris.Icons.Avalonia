# Using Font Awesome Pro

> **Licensing.** Optris.Icons.Avalonia ships **Font Awesome Free** icon data in its NuGet packages. To use Pro or Pro+ icons you must have a valid [Font Awesome Pro license](https://fontawesome.com/plans) and supply the Pro `icons.json` from your own download at runtime — the Optris packages never redistribute Pro assets.

This page applies to both `Optris.Icons.Avalonia.FontAwesome` (FA6) and `Optris.Icons.Avalonia.FontAwesome7` (FA7). The API is symmetric; the examples below show the FA6 type, replace with `FontAwesome7IconProvider` as needed.

## Where to get the Pro `icons.json`

The easiest source is the **SVG + Web** download from [fontawesome.com/download](https://fontawesome.com/download) (available to Pro licensees). Inside the zip, the file is at `metadata/icons.json`. Its schema matches the Free metadata the library already reads, just with more icons and more styles per icon (`light`, `thin`, `duotone`, `sharp-solid`, `sharp-regular`, …).

Other Pro delivery formats (Kits, Desktop fonts, the `@awesome.me/kit-…` npm package) do not ship this file as-is — use the SVG + Web download if you want the zero-friction path.

## Registering a provider with your Pro data

Four static factories cover the common hosting options. Pick whichever fits how you ship the file in your app.

### 1. Embedded in your assembly (recommended)

Drop `icons.json` into your project and mark it as `EmbeddedResource`:

```xml
<ItemGroup>
  <EmbeddedResource Include="Assets/icons.json" />
</ItemGroup>
```

Register with the resource name (`<AssemblyName>.<FolderPath>.icons.json`):

```csharp
IconProvider.Current.Register(
    FontAwesomeIconProvider.FromEmbeddedResource(
        typeof(App).Assembly,
        "MyApp.Assets.icons.json"));
```

### 2. Avalonia asset (`avares://`)

If you already use Avalonia's asset pipeline, mark the file as `AvaloniaResource` and register by URI:

```xml
<ItemGroup>
  <AvaloniaResource Include="Assets/icons.json" />
</ItemGroup>
```

```csharp
IconProvider.Current.Register(
    FontAwesomeIconProvider.FromAvaloniaResource(
        new Uri("avares://MyApp/Assets/icons.json")));
```

### 3. File on disk

If the icon data is shipped alongside your executable or downloaded at runtime:

```csharp
IconProvider.Current.Register(
    FontAwesomeIconProvider.FromFile("icons.json"));
```

### 4. Arbitrary stream

For unusual hosting (encrypted assets, downloaded on first launch, etc.) pass any readable `Stream`. The stream is fully buffered into memory during this call; you remain responsible for disposing it:

```csharp
using var stream = OpenMyIconsJson();
IconProvider.Current.Register(
    FontAwesomeIconProvider.FromStream(stream));
```

## Using Pro-only styles in XAML

After registering a Pro provider, any style present in your `icons.json` becomes addressable using the usual key syntax:

```xml
<i:Icon Value="fa-light fa-anchor" />
<i:Icon Value="fa-thin fa-anchor" />
<i:Icon Value="fa-duotone fa-anchor" />
<i:Icon Value="fa-sharp-solid fa-anchor" />
<i:Icon Value="fa-sharp-duotone fa-anchor" />
```

The same applies to the `fa7-` prefix when using `FontAwesome7IconProvider`:

```xml
<i:Icon Value="fa7-chisel fa7-anchor" />
<i:Icon Value="fa7-etch fa7-anchor" />
```

No code change is needed per style — the provider reads whatever styles are in the JSON you supplied.

## Composing your own stream provider

The static factories are thin wrappers around public `IFontAwesomeUtf8JsonStreamProvider` (FA6) / `IFontAwesome7Utf8JsonStreamProvider` (FA7) implementations:

| Factory | Primitive |
| :-- | :-- |
| `FromFile(path)` | `FontAwesomeFileUtf8JsonStreamProvider` |
| `FromEmbeddedResource(asm, name)` | `FontAwesomeEmbeddedResourceUtf8JsonStreamProvider` |
| `FromAvaloniaResource(uri)` | `FontAwesomeAvaloniaResourceUtf8JsonStreamProvider` |
| `FromStream(stream)` | internal buffered provider |

If you have a scenario none of those fit — for example, decrypting the JSON on read — implement the interface directly and pass it to the existing constructor:

```csharp
public sealed class MyCustomStreamProvider : IFontAwesomeUtf8JsonStreamProvider
{
    public Stream GetUtf8JsonStream() => /* your logic */;
}

IconProvider.Current.Register(new FontAwesomeIconProvider(new MyCustomStreamProvider()));
```

## Troubleshooting

**`KeyNotFoundException: FontAwesome style "light" not found for icon "…"`** — The icon doesn't ship that style in your `icons.json`. Either the file is Free (no `light`), or the icon genuinely doesn't have that style in the Pro data.

**`KeyNotFoundException: FontAwesome icon "…" not found!`** — The icon name isn't in your `icons.json` at all. Check the spelling against your Pro download; some icons were renamed between FA5 → FA6 (the library handles those) and between FA6 → FA7 (you must use the matching package).

**`InvalidOperationException: Embedded resource "…" was not found`** — The `LogicalName` GitHub computes for an `EmbeddedResource` is `<AssemblyName>.<PathWithDotsInsteadOfSlashes>.<FileName>`. If your file is `Assets/icons.json` in `MyApp.csproj`, the resource name is `MyApp.Assets.icons.json`.
