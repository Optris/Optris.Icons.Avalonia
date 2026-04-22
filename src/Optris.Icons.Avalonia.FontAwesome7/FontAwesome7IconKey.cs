using System;
using Optris.Icons.Avalonia.FontAwesome7.Models;

namespace Optris.Icons.Avalonia.FontAwesome7;

internal partial class FontAwesome7IconKey
{
    private const string _fa7KeyPrefix = "fa7-";
    public string Value { get; set; }
    public string Style { get; set; }

    public static bool TryParse(string value, out FontAwesome7IconKey key)
    {
        var parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 1)
        {
            key = new FontAwesome7IconKey
            {
                Value = GetValue(parts[0]),
            };
            return true;
        }

        if (parts.Length == 2)
        {
            key = new FontAwesome7IconKey
            {
                Style = GetValue(parts[0]),
                Value = GetValue(parts[1]),
            };

            return true;
        }

        key = null;
        return false;
    }

    private static string GetValue(string input)
    {
        var value = input.Length <= _fa7KeyPrefix.Length
            ? string.Empty
            : input.Substring(_fa7KeyPrefix.Length);

        return value;
    }
}
