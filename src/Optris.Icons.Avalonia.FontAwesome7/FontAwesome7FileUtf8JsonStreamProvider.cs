using System.IO;

namespace Optris.Icons.Avalonia.FontAwesome7;

/// <summary>
/// Loads the Font Awesome 7 icon collection from a file on disk.
/// </summary>
public sealed class FontAwesome7FileUtf8JsonStreamProvider : IFontAwesome7Utf8JsonStreamProvider
{
    private readonly string _path;

    /// <summary>
    /// Initializes a new instance pointing at <paramref name="path"/>.
    /// </summary>
    public FontAwesome7FileUtf8JsonStreamProvider(string path)
    {
        _path = path;
    }

    /// <inheritdoc/>
    public Stream GetUtf8JsonStream() => File.OpenRead(_path);
}
