using System.Collections.Generic;

namespace Optris.Icons.Avalonia;

/// <summary>
/// Optional interface for icon providers that support enumerating all available icon keys.
/// </summary>
public interface IIconKeyProvider
{
    /// <summary>
    /// Gets all valid, fully-qualified icon key strings that can be passed to <see cref="IIconReader.GetIcon"/>.
    /// </summary>
    IReadOnlyList<string> Keys { get; }
}
