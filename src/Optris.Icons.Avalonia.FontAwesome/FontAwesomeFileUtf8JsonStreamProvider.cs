using System.IO;

namespace Optris.Icons.Avalonia.FontAwesome;

/// <summary>
/// Loads the Font Awesome icon collection from a file on disk.
/// </summary>
public sealed class FontAwesomeFileUtf8JsonStreamProvider : IFontAwesomeUtf8JsonStreamProvider
{
    private readonly string _path;

    /// <summary>
    /// Initializes a new instance pointing at <paramref name="path"/>.
    /// </summary>
    public FontAwesomeFileUtf8JsonStreamProvider(string path)
    {
        _path = path;
    }

    /// <inheritdoc/>
    public Stream GetUtf8JsonStream() => File.OpenRead(_path);
}
