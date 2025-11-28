// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.Controls;
using Wpf.Ui.Gallery.Models;

namespace Wpf.Ui.Gallery.ViewModels.Pages.DesignGuidance;

public partial class ElectricalSymbolsViewModel : ViewModel
{
    private int _selectedIconId = 0;

    private string _autoSuggestBoxText = string.Empty;

    [ObservableProperty]
    private ElectricalSymbolRegular _selectedElectricalSymbol = ElectricalSymbolRegular.Empty;

    [ObservableProperty]
    private string _selectedSymbolName = string.Empty;

    [ObservableProperty]
    private string _selectedSymbolUnicodePoint = string.Empty;

    [ObservableProperty]
    private string _selectedSymbolTextGlyph = string.Empty;

    [ObservableProperty]
    private string _selectedSymbolXaml = string.Empty;

    [ObservableProperty]
    private List<DisplayableIcon> _iconsCollection = [];

    [ObservableProperty]
    private List<DisplayableIcon> _filteredIconsCollection = [];

    [ObservableProperty]
    private List<string> _iconNames = [];

    public string AutoSuggestBoxText
    {
        get => _autoSuggestBoxText;
        set
        {
            _ = SetProperty(ref _autoSuggestBoxText, value);
            UpdateSearchResults(value);
        }
    }

    public ElectricalSymbolsViewModel()
    {
        LoadIcons();
    }

    private void LoadIcons()
    {
        _ = Task.Run(() =>
        {
            var icons = new List<DisplayableIcon>();
            var id = 0;

            var names = Enum.GetNames(typeof(ElectricalSymbolRegular));
            names = names.OrderBy(n => n).ToArray();

            foreach (string iconName in names)
            {
                if (!Enum.TryParse<ElectricalSymbolRegular>(iconName, out ElectricalSymbolRegular icon))
                {
                    continue;
                }

                if (icon == ElectricalSymbolRegular.Empty)
                {
                    continue;
                }

                icons.Add(new DisplayableIcon
                {
                    Id = id++,
                    Name = iconName,
                    ElectricalIcon = icon,
                    Symbol = ((char)icon).ToString(),
                    Code = ((int)icon).ToString("X4"),
                    IsMaterialIcon = false,
                    IsSegoeFluentIcon = false,
                    IsBootstrapIcon = false,
                    IsElectricalIcon = true
                });
            }

            IconsCollection = icons;
            FilteredIconsCollection = icons;
            IconNames = icons.Select(icon => icon.Name).ToList();

            if (icons.Count > 0)
            {
                _selectedIconId = 0;
                UpdateSymbolData();
            }
        });
    }

    [RelayCommand]
    public void OnIconSelected(int parameter)
    {
        _selectedIconId = parameter;

        UpdateSymbolData();
    }

    private void UpdateSymbolData()
    {
        if (IconsCollection.Count - 1 < _selectedIconId)
        {
            return;
        }

        DisplayableIcon selectedSymbol = IconsCollection.FirstOrDefault(sym => sym.Id == _selectedIconId);

        SelectedElectricalSymbol = selectedSymbol.ElectricalIcon;
        SelectedSymbolName = selectedSymbol.Name;
        SelectedSymbolUnicodePoint = selectedSymbol.Code;
        SelectedSymbolTextGlyph = $"&#x{selectedSymbol.Code};";
        SelectedSymbolXaml = $"<ui:ElectricalIcon Symbol=\"{selectedSymbol.Name}\"/>";
    }

    private void UpdateSearchResults(string searchedText)
    {
        _ = Task.Run(() =>
        {
            if (string.IsNullOrEmpty(searchedText))
            {
                FilteredIconsCollection = IconsCollection;

                return true;
            }

            var formattedText = searchedText.ToLower().Trim();

            FilteredIconsCollection = IconsCollection
                .Where(icon => icon.Name.Contains(formattedText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return true;
        });
    }
}
