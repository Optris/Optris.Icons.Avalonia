using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Optris.Icons.Avalonia.FontAwesome7.Models;
using Optris.Icons.Avalonia.Models;

namespace Optris.Icons.Avalonia.FontAwesome7;

/// <summary>
/// Implements the <see cref="IIconProvider"/> interface to provide Font Awesome 7 icons.
/// </summary>
public class FontAwesome7IconProvider : IIconProvider, IIconKeyProvider
{
    private const string _fa7ProviderPrefix = "fa7";
    private readonly IFontAwesome7Utf8JsonStreamProvider _streamProvider;
    private readonly Lazy<Dictionary<string, FontAwesome7Icon>> _lazyIcons;
    private readonly Lazy<IReadOnlyList<string>> _lazyKeys;

    /// <summary>
    /// Initializes a new instance of the <see cref="FontAwesome7IconProvider"/> using the <see cref="FontAwesome7FreeUtf8JsonStreamProvider"/>
    /// to get the UTF-8 encoded json stream to deserialize the icon collection.
    /// </summary>
    public FontAwesome7IconProvider()
        : this(new FontAwesome7FreeUtf8JsonStreamProvider()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontAwesome7IconProvider"/> using the specified <see cref="IFontAwesome7Utf8JsonStreamProvider"/>
    /// to get the UTF-8 encoded json stream to deserialize the icon collection.
    /// </summary>
    public FontAwesome7IconProvider(IFontAwesome7Utf8JsonStreamProvider streamProvider)
    {
        _streamProvider = streamProvider;
        _lazyIcons = new(Parse);
        _lazyKeys = new(BuildKeys);
    }

    /// <inheritdoc/>
    public string Prefix => _fa7ProviderPrefix;

    /// <inheritdoc/>
    public IReadOnlyList<string> Keys => _lazyKeys.Value;

    private Dictionary<string, FontAwesome7Icon> Icons => _lazyIcons.Value;

    /// <inheritdoc/>
    public IconModel GetIcon(string value)
    {
        if (!FontAwesome7IconKey.TryParse(value, out FontAwesome7IconKey key))
        {
            throw new KeyNotFoundException($"FontAwesome 7 icon \"{value}\" not found!");
        }
        else if (!Icons.TryGetValue(key.Value, out FontAwesome7Icon icon))
        {
            throw new KeyNotFoundException($"FontAwesome 7 icon \"{key.Value}\" not found!");
        }
        else if (!key.Style.HasValue)
        {
            return icon.Svg.Values.First().ToIconModel();
        }
        else if (icon.Svg.TryGetValue(key.Style.Value, out Svg svg))
        {
            return svg.ToIconModel();
        }

        throw new KeyNotFoundException(
            $"FontAwesome 7 style \"{key.Style}\" not found for icon \"{key.Value}\". Maybe you are trying to use an unsupported pro icon."
        );
    }

    private IReadOnlyList<string> BuildKeys()
    {
        var keys = new List<string>();
        foreach (var kvp in Icons)
        {
            foreach (var style in kvp.Value.Svg.Keys)
            {
                keys.Add($"{_fa7ProviderPrefix}-{style.ToString().ToLowerInvariant()} {_fa7ProviderPrefix}-{kvp.Key}");
            }
        }

        keys.Sort(StringComparer.Ordinal);
        return keys;
    }

    private Dictionary<string, FontAwesome7Icon> Parse()
    {
        using var stream = _streamProvider.GetUtf8JsonStream();

        var result = JsonSerializer.Deserialize(
            stream,
            FontAwesome7IconsJsonContext.Default.DictionaryStringFontAwesome7Icon
        );

        return result;
    }
}
