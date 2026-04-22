using System;
using System.IO;
using System.Reflection;

namespace Optris.Icons.Avalonia.FontAwesome;

/// <summary>
/// Loads the Font Awesome icon collection from an embedded resource in the given assembly.
/// </summary>
public sealed class FontAwesomeEmbeddedResourceUtf8JsonStreamProvider : IFontAwesomeUtf8JsonStreamProvider
{
    private readonly Assembly _assembly;
    private readonly string _resourceName;

    /// <summary>
    /// Initializes a new instance that reads <paramref name="resourceName"/> from <paramref name="assembly"/>.
    /// </summary>
    public FontAwesomeEmbeddedResourceUtf8JsonStreamProvider(Assembly assembly, string resourceName)
    {
        _assembly = assembly;
        _resourceName = resourceName;
    }

    /// <inheritdoc/>
    public Stream GetUtf8JsonStream()
    {
        var stream = _assembly.GetManifestResourceStream(_resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException(
                $"Embedded resource \"{_resourceName}\" was not found in assembly \"{_assembly.GetName().Name}\"."
            );
        }
        return stream;
    }
}
