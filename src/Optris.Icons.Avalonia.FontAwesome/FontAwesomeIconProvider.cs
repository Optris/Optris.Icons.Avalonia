using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Optris.Icons.Avalonia.FontAwesome.Models;
using Optris.Icons.Avalonia.Models;

namespace Optris.Icons.Avalonia.FontAwesome;

/// <summary>
/// Implements the <see cref="IIconProvider"/> interface to provide font-awesome icons.
/// </summary>
public class FontAwesomeIconProvider : IIconProvider, IIconKeyProvider
{
    private const string _faProviderPrefix = "fa";
    private readonly IFontAwesomeUtf8JsonStreamProvider _streamProvider;
    private readonly Lazy<Dictionary<string, FontAwesomeIcon>> _lazyIcons;
    private readonly Lazy<IReadOnlyList<string>> _lazyKeys;

    /// <summary>
    /// Initializes a new instance of the <see cref="FontAwesomeIconProvider"/> using the <see cref="FontAwesomeFreeUtf8JsonStreamProvider"/>
    /// to get the UTF-8 encoded json stream to deserialize the icon collection.
    /// </summary>
    public FontAwesomeIconProvider()
        : this(new FontAwesomeFreeUtf8JsonStreamProvider()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontAwesomeIconProvider"/> using the specified <see cref="IFontAwesomeUtf8JsonStreamProvider"/>
    /// to get the UTF-8 encoded json stream to deserialize the icon collection.
    /// </summary>
    public FontAwesomeIconProvider(IFontAwesomeUtf8JsonStreamProvider streamProvider)
    {
        _streamProvider = streamProvider;
        _lazyIcons = new(Parse);
        _lazyKeys = new(BuildKeys);
    }

    /// <inheritdoc/>
    public string Prefix => _faProviderPrefix;

    /// <inheritdoc/>
    public IReadOnlyList<string> Keys => _lazyKeys.Value;

    private Dictionary<string, FontAwesomeIcon> Icons => _lazyIcons.Value;

    /// <inheritdoc/>
    public IconModel GetIcon(string value)
    {
        if (!FontAwesomeIconKey.TryParse(value, out FontAwesomeIconKey key))
        {
            throw new KeyNotFoundException($"FontAwesome icon \"{value}\" not found!");
        }
        else if (!Icons.TryGetValue(key.Value, out FontAwesomeIcon icon))
        {
            throw new KeyNotFoundException($"FontAwesome icon \"{key.Value}\" not found!");
        }
        else if (string.IsNullOrEmpty(key.Style))
        {
            return icon.Svg.Values.First().ToIconModel();
        }
        else if (icon.Svg.TryGetValue(key.Style, out Svg svg))
        {
            return svg.ToIconModel();
        }

        throw new KeyNotFoundException(
            $"FontAwesome style \"{key.Style}\" not found for icon \"{key.Value}\". Maybe you are trying to use an unsupported pro icon."
        );
    }

    private IReadOnlyList<string> BuildKeys()
    {
        var keys = new List<string>();
        foreach (var kvp in Icons)
        {
            foreach (var style in kvp.Value.Svg.Keys)
            {
                keys.Add($"{_faProviderPrefix}-{style.ToLowerInvariant()} {_faProviderPrefix}-{kvp.Key}");
            }
        }

        keys.Sort(StringComparer.Ordinal);
        return keys;
    }

    private Dictionary<string, FontAwesomeIcon> Parse()
    {
        using var stream = _streamProvider.GetUtf8JsonStream();

        var result = JsonSerializer.Deserialize(
            stream,
            FontAwesomeIconsJsonContext.Default.DictionaryStringFontAwesomeIcon
        );

        return result;
    }
}
