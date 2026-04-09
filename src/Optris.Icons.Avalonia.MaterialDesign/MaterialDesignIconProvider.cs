using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Optris.Icons.Avalonia.Models;

namespace Optris.Icons.Avalonia.MaterialDesign;

/// <summary>
/// Implements the <see cref="IIconProvider"/> interface to provide Material Design icons.
/// </summary>
public class MaterialDesignIconProvider : IIconProvider, IIconKeyProvider
{
    private const string _mdiProviderPrefix = "mdi";

    private static readonly Assembly _assembly = typeof(MaterialDesignIconProvider).Assembly;

    private static readonly string _resourceNameTemplate
        = $"{_assembly.GetName().Name}.Assets.{{0}}.svg";

    private static readonly Regex _viewBoxRegex = new("viewBox=\"([0-9 -]+)\"");
    private static readonly Regex _pathRegex = new("<path d=\"(.+)\"");
    private readonly Dictionary<string, IconModel> _icons = new();
    private readonly Lazy<IReadOnlyList<string>> _lazyKeys = new(BuildKeys);

    public string Prefix => _mdiProviderPrefix;

    /// <inheritdoc/>
    public IReadOnlyList<string> Keys => _lazyKeys.Value;

    /// <inheritdoc/>
    public IconModel GetIcon(string value)
    {
        if (_icons.TryGetValue(value, out var icon))
        {
            return icon;
        }

        icon = GetIconFromResource(value);
        return _icons[value] = icon;
    }

    private static IReadOnlyList<string> BuildKeys()
    {
        var assetsPrefix = $"{_assembly.GetName().Name}.Assets.";
        const string svgSuffix = ".svg";

        return _assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(assetsPrefix, StringComparison.Ordinal) && n.EndsWith(svgSuffix, StringComparison.Ordinal))
            .Select(n => $"{_mdiProviderPrefix}-{n.Substring(assetsPrefix.Length, n.Length - assetsPrefix.Length - svgSuffix.Length)}")
            .OrderBy(k => k, StringComparer.Ordinal)
            .ToList();
    }

    private static IconModel GetIconFromResource(string value)
    {
        using (Stream stream = GetIconResourceStream(value))
        using (TextReader textReader = new StreamReader(stream))
        {
            var svg = textReader.ReadToEnd();

            var viewBoxMatch = _viewBoxRegex.Match(svg);
            if (!viewBoxMatch.Success)
            {
                throw new KeyNotFoundException(
                    $"Material Design Icon \"{value}\": SVG has no valid viewBox attribute.");
            }

            var pathMatch = _pathRegex.Match(svg);
            if (!pathMatch.Success)
            {
                throw new KeyNotFoundException(
                    $"Material Design Icon \"{value}\": SVG has no valid path element.");
            }

            return new IconModel(
                ViewBoxModel.Parse(viewBoxMatch.Groups[1].Value),
                new PathModel(pathMatch.Groups[1].Value));
        }
    }

    private static Stream GetIconResourceStream(string value)
    {
        return TryGetIconResourceStream(value, out var stream)
            ? stream
            : throw new KeyNotFoundException($"Material Design Icon \"{value}\" not found!");
    }

    private static bool TryGetIconResourceStream(string value, out Stream stream)
    {
        stream = default;

        if (value.Length <= _mdiProviderPrefix.Length + 1)
        {
            return false;
        }

        var withoutPrefix = value.Substring(_mdiProviderPrefix.Length + 1);
        var resourceName = string.Format(_resourceNameTemplate, withoutPrefix);
        stream = _assembly.GetManifestResourceStream(resourceName);

        return stream != default;
    }
}
