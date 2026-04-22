using System.IO;

namespace Optris.Icons.Avalonia.FontAwesome7;

/// <summary>
/// Serves the Font Awesome 7 icon collection from an in-memory byte buffer.
/// Each call to <see cref="GetUtf8JsonStream"/> returns a fresh read-only <see cref="MemoryStream"/>.
/// </summary>
internal sealed class BufferedUtf8JsonStreamProvider : IFontAwesome7Utf8JsonStreamProvider
{
    private readonly byte[] _bytes;

    public BufferedUtf8JsonStreamProvider(byte[] bytes)
    {
        _bytes = bytes;
    }

    public Stream GetUtf8JsonStream() => new MemoryStream(_bytes, writable: false);
}
