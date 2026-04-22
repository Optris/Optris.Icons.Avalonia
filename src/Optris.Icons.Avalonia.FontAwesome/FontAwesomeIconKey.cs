using System;
using Optris.Icons.Avalonia.FontAwesome.Models;

namespace Optris.Icons.Avalonia.FontAwesome;

internal partial class FontAwesomeIconKey
{
    private const string _faKeyPrefix = "fa-";
    public string Value { get; set; }
    public string Style { get; set; }

    public static bool TryParse(string value, out FontAwesomeIconKey key)
    {
        var parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 1)
        {
            key = new FontAwesomeIconKey
            {
                Value = GetValue(parts[0]),
            };
            return true;
        }

        if (parts.Length == 2)
        {
            key = new FontAwesomeIconKey
            {
                Style = GetStyle(parts[0]),
                Value = GetValue(parts[1]),
            };

            return true;
        }

        key = null;
        return false;
    }

    private static string GetStyle(string value)
    {
        return value.ToUpperInvariant() switch
        {
            "FA-SOLID" or "FAS" => "solid",
            "FA-REGULAR" or "FAR" => "regular",
            "FA-BRANDS" or "FAB" => "brands",
            _ => value.Length <= _faKeyPrefix.Length
                ? string.Empty
                : value.Substring(_faKeyPrefix.Length),
        };
    }

    private static string GetValue(string input)
    {
        var value = input.Length <= _faKeyPrefix.Length
            ? string.Empty
            : input.Substring(_faKeyPrefix.Length);

        return SupportLegacy(value);
    }
}
