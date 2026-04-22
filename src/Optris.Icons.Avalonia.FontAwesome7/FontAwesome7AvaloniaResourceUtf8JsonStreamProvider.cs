using System;
using System.IO;
using Avalonia.Platform;

namespace Optris.Icons.Avalonia.FontAwesome7;

/// <summary>
/// Loads the Font Awesome 7 icon collection from an Avalonia asset URI (<c>avares://…</c>).
/// </summary>
public sealed class FontAwesome7AvaloniaResourceUtf8JsonStreamProvider : IFontAwesome7Utf8JsonStreamProvider
{
    private readonly Uri _uri;

    /// <summary>
    /// Initializes a new instance pointing at <paramref name="uri"/>, e.g. <c>avares://MyApp/Assets/icons.json</c>.
    /// </summary>
    public FontAwesome7AvaloniaResourceUtf8JsonStreamProvider(Uri uri)
    {
        _uri = uri;
    }

    /// <inheritdoc/>
    public Stream GetUtf8JsonStream() => AssetLoader.Open(_uri);
}
