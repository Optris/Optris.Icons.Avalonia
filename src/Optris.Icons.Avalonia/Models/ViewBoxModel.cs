using System;

namespace Optris.Icons.Avalonia.Models;

public record ViewBoxModel(int X, int Y, int Width, int Height)
{
    public static ViewBoxModel Parse(string viewBox)
    {
        var parts = viewBox.Split(' ');
        if (parts.Length != 4)
        {
            throw new FormatException(
                $"Invalid viewBox \"{viewBox}\": expected 4 space-separated integers.");
        }

        if (!int.TryParse(parts[0], out var x)
            || !int.TryParse(parts[1], out var y)
            || !int.TryParse(parts[2], out var width)
            || !int.TryParse(parts[3], out var height))
        {
            throw new FormatException(
                $"Invalid viewBox \"{viewBox}\": all parts must be valid integers.");
        }

        return new ViewBoxModel(x, y, width, height);
    }
}
