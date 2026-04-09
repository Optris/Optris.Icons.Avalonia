[Home](README.md) | [Getting Started](getting-started.md) | [Usage Guide](usage-guide.md) | **Icon Packs** | [Custom Providers](custom-providers.md) | [API Reference](api-reference.md) | [Troubleshooting](troubleshooting.md)

# Icon Packs

Optris.Icons.Avalonia ships with two built-in icon providers. You can also [create your own](custom-providers.md).

## Font Awesome 6 Free

**Package:** `Optris.Icons.Avalonia.FontAwesome`
**Prefix:** `fa`

### Value format

```
fa-{style} fa-{icon-name}
```

If you omit the style, the first available style is used:

```xml
<i:Icon Value="fa-github" />           <!-- picks first available style -->
<i:Icon Value="fa-solid fa-house" />    <!-- explicit solid style -->
<i:Icon Value="fa-regular fa-heart" />  <!-- explicit regular style -->
<i:Icon Value="fa-brands fa-github" />  <!-- brand icon -->
```

### Available styles

| Style     | Prefix        | Short prefix | Description              |
| :-------- | :------------ | :----------- | :----------------------- |
| Solid     | `fa-solid`    | `fas`        | Filled icons (default)   |
| Regular   | `fa-regular`  | `far`        | Outlined icons           |
| Brands    | `fa-brands`   | `fab`        | Brand/logo icons         |

### Browse icons

Browse the full icon catalog at [fontawesome.com/search?o=r&m=free](https://fontawesome.com/search?o=r&m=free).

### Free vs. Pro

Only **Font Awesome Free** icons are included. Attempting to use a Pro-only icon will throw a `KeyNotFoundException` with a message about unsupported pro icons.

To use Font Awesome Pro icons, implement `IFontAwesomeUtf8JsonStreamProvider` to supply your own icon JSON data. See [Custom Providers](custom-providers.md) for details.

### Legacy name support

Older Font Awesome icon names (e.g. from FA 5) are automatically mapped to their FA 6 equivalents when possible.

## Material Design Icons

**Package:** `Optris.Icons.Avalonia.MaterialDesign`
**Prefix:** `mdi`

### Value format

```
mdi-{icon-name}
```

```xml
<i:Icon Value="mdi-github" />
<i:Icon Value="mdi-home" />
<i:Icon Value="mdi-arrow-left" />
```

### Browse icons

Browse the full icon catalog at [pictogrammers.com/library/mdi](https://pictogrammers.com/library/mdi/).

## Using multiple packs

Both providers can be registered simultaneously. The library routes icons to the correct provider based on the prefix (`fa` or `mdi`):

```csharp
IconProvider.Current
    .Register<FontAwesomeIconProvider>()
    .Register<MaterialDesignIconProvider>();
```

You can mix icons from different packs in the same view:

```xml
<i:Icon Value="fa-solid fa-home" />
<i:Icon Value="mdi-home" />
```

## Icon updates

Both icon sets are automatically synced from their upstream repositories via scheduled GitHub Actions. Updates are submitted as pull requests to keep the library current.
