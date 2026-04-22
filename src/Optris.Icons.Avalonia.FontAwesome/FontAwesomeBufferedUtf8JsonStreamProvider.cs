using System.IO;

namespace Optris.Icons.Avalonia.FontAwesome;

/// <summary>
/// Serves the Font Awesome icon collection from an in-memory byte buffer.
/// Each call to <see cref="GetUtf8JsonStream"/> returns a fresh read-only <see cref="MemoryStream"/>.
/// </summary>
internal sealed class BufferedUtf8JsonStreamProvider : IFontAwesomeUtf8JsonStreamProvider
{
    private readonly byte[] _bytes;

    public BufferedUtf8JsonStreamProvider(byte[] bytes)
    {
        _bytes = bytes;
    }

    public Stream GetUtf8JsonStream() => new MemoryStream(_bytes, writable: false);
}
