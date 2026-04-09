using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Threading;
using Optris.Icons.Avalonia;

namespace Demo;

public record IconEntry(string Key, string Name, string Provider);

public class IconBrowserViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private readonly List<IconEntry> _allIcons;
    private readonly DispatcherTimer _debounceTimer;

    private string _searchText = string.Empty;
    private int _selectedProviderIndex;
    private IReadOnlyList<IconEntry> _filteredIcons;
    private IconEntry _selectedIcon;

    public IconBrowserViewModel()
    {
        var providers = IconProvider.Current.Providers;

        ProviderOptions = ["All", .. providers.Select(GetProviderDisplayName)];

        _allIcons = [];
        foreach (var provider in providers)
        {
            if (provider is IIconKeyProvider keyProvider)
            {
                var displayName = GetProviderDisplayName(provider);
                foreach (var key in keyProvider.Keys)
                {
                    var name = ExtractName(key, provider.Prefix);
                    _allIcons.Add(new IconEntry(key, name, displayName));
                }
            }
        }

        _filteredIcons = _allIcons;

        _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        _debounceTimer.Tick += (_, _) =>
        {
            _debounceTimer.Stop();
            ApplyFilter();
        };
    }

    public IReadOnlyList<string> ProviderOptions { get; }

    public int TotalCount => _allIcons.Count;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value) return;
            _searchText = value;
            PropertyChanged?.Invoke(this, new(nameof(SearchText)));
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }
    }

    public int SelectedProviderIndex
    {
        get => _selectedProviderIndex;
        set
        {
            if (_selectedProviderIndex == value) return;
            _selectedProviderIndex = value;
            PropertyChanged?.Invoke(this, new(nameof(SelectedProviderIndex)));
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }
    }

    public IReadOnlyList<IconEntry> FilteredIcons
    {
        get => _filteredIcons;
        private set
        {
            _filteredIcons = value;
            PropertyChanged?.Invoke(this, new(nameof(FilteredIcons)));
            PropertyChanged?.Invoke(this, new(nameof(FilteredCount)));
        }
    }

    public int FilteredCount => _filteredIcons.Count;

    public IconEntry SelectedIcon
    {
        get => _selectedIcon;
        set
        {
            if (_selectedIcon == value) return;
            _selectedIcon = value;
            PropertyChanged?.Invoke(this, new(nameof(SelectedIcon)));
        }
    }

    private void ApplyFilter()
    {
        var search = _searchText;
        var providerPrefix = GetSelectedProviderPrefix();

        IEnumerable<IconEntry> result = _allIcons;

        if (providerPrefix is not null)
        {
            result = result.Where(e => e.Key.StartsWith(providerPrefix + "-", StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            result = result.Where(e => e.Key.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        FilteredIcons = result.ToList();
    }

    private string GetSelectedProviderPrefix()
    {
        if (_selectedProviderIndex <= 0)
            return null;

        var providers = IconProvider.Current.Providers;
        var index = _selectedProviderIndex - 1; // skip "All"
        return index < providers.Count ? providers[index].Prefix : null;
    }

    private static string GetProviderDisplayName(IIconProvider provider)
    {
        return provider.Prefix switch
        {
            "fa" => "FontAwesome",
            "fa7" => "FontAwesome 7",
            "mdi" => "Material Design",
            _ => provider.Prefix,
        };
    }

    private static string ExtractName(string key, string prefix)
    {
        // "fa-solid fa-github" -> "github"
        // "mdi-account" -> "account"
        var parts = key.Split(' ');
        var last = parts[^1];
        return last.Length > prefix.Length + 1
            ? last[(prefix.Length + 1)..]
            : last;
    }
}
