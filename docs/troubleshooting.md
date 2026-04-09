[Home](README.md) | [Getting Started](getting-started.md) | [Usage Guide](usage-guide.md) | [Icon Packs](icon-packs.md) | [Custom Providers](custom-providers.md) | [API Reference](api-reference.md) | **Troubleshooting**

# Troubleshooting

## Common errors

### "No IIconProvider with prefix matching ... was registered"

**Cause:** No icon provider was registered before the icon was used.

**Fix:** Ensure you call `IconProvider.Current.Register<...>()` in `Program.cs` **before** `AppBuilder.Configure<App>()`. See [Getting Started](getting-started.md).

### "FontAwesome icon ... not found"

**Cause:** The icon name doesn't exist in the Font Awesome Free set, or there's a typo.

**Fix:** Verify the icon name at [fontawesome.com/search?o=r&m=free](https://fontawesome.com/search?o=r&m=free). Icon names use the `fa-{name}` format (e.g. `fa-house`, not `fa-home` in FA 6).

### "FontAwesome style ... not found ... unsupported pro icon"

**Cause:** You're requesting a style (solid, regular, brands) that only exists in Font Awesome Pro.

**Fix:** Use a different style that's available in the Free set, or supply a Pro icon source via `IFontAwesomeUtf8JsonStreamProvider`. See [Custom Providers](custom-providers.md).

### Prefix conflict on registration

**Cause:** Two providers have prefixes where one is a prefix of the other (e.g. `"fa"` and `"fab"`).

**Fix:** Each provider must have a prefix that doesn't conflict with existing providers. Choose a distinct prefix for your custom provider.

## Display issues

### Icon not showing (blank)

- Verify the icon name is correct and the provider is registered.
- Check that the XAML namespace is declared: `xmlns:i="https://github.com/projektanker/icons.avalonia"`
- Ensure the `Value` property is set (not empty).

### Icon too small or too large

`FontSize` controls icon dimensions. If not set, it inherits from the parent. Set it explicitly:

```xml
<i:Icon Value="fa-solid fa-check" FontSize="24" />
```

### Icon color not changing

- On the `Icon` control, use `Foreground`.
- On `IconImage`, use `Brush` (not `Foreground`).

```xml
<!-- Icon control -->
<i:Icon Value="fa-solid fa-check" Foreground="Red" />

<!-- IconImage -->
<i:IconImage Value="fa-solid fa-check" Brush="Red" />
```

### Animation not working

The `Animation` property must be set on the `Icon` control itself, not on a parent:

```xml
<!-- Correct -->
<i:Icon Value="fa-solid fa-spinner" Animation="Pulse" />

<!-- Won't work: Animation on a wrapper -->
<Border>
  <i:Icon Value="fa-solid fa-spinner" />
</Border>
```

## Migration from Projektanker.Icons.Avalonia

If you're migrating from the original `Projektanker.Icons.Avalonia` package:

1. **NuGet packages:** Replace `Projektanker.Icons.Avalonia.*` with `Optris.Icons.Avalonia.*`
2. **C# namespaces:** Change `using Projektanker.Icons.Avalonia` to `using Optris.Icons.Avalonia`
3. **XAML namespace:** No change needed -- `xmlns:i="https://github.com/projektanker/icons.avalonia"` is preserved for backward compatibility
4. **API:** The public API is unchanged

## XAML namespace URL

The XAML namespace `https://github.com/projektanker/icons.avalonia` is an **opaque XML identifier**, not a clickable web URL. This is intentional and preserved for backward compatibility with existing XAML files.
