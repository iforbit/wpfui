// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;

using Wpf.Ui.Controls;
using Wpf.Ui.Gallery.Models;

namespace Wpf.Ui.Gallery.ViewModels.Pages.DesignGuidance;

public partial class IconsViewModel : ViewModel
{
    private int _selectedIconId = 0;

    private string _autoSuggestBoxText = string.Empty;

    [ObservableProperty]
    private string _selectedFontType = "FluentSystemIcons";

    [ObservableProperty]
    private SymbolRegular _selectedSymbol = SymbolRegular.Empty;

    [ObservableProperty]
    private MaterialSymbolRegular _selectedMaterialSymbol = MaterialSymbolRegular.Empty;

    [ObservableProperty]
    private SegoeFluentSymbol _selectedSegoeFluentSymbol = SegoeFluentSymbol.Empty;

    [ObservableProperty]
    private BootstrapSymbolRegular _selectedBootstrapSymbol = BootstrapSymbolRegular._123;

    [ObservableProperty]
    private string _selectedSymbolName = string.Empty;

    [ObservableProperty]
    private string _selectedSymbolUnicodePoint = string.Empty;

    [ObservableProperty]
    private string _selectedSymbolTextGlyph = string.Empty;

    [ObservableProperty]
    private string _selectedSymbolXaml = string.Empty;

    [ObservableProperty]
    private bool _isIconFilled = false;

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

    public IconsViewModel()
    {
        LoadIcons();
    }

    partial void OnSelectedFontTypeChanged(string value)
    {
        LoadIcons();
    }

    private void LoadIcons()
    {
        _ = Task.Run(() =>
        {
            var icons = new List<DisplayableIcon>();
            var id = 0;

            if (SelectedFontType == "MaterialIcons")
            {
                var names = Enum.GetNames(typeof(MaterialSymbolRegular));
                names = names.OrderBy(n => n).ToArray();

                foreach (string iconName in names)
                {
                    if (!Enum.TryParse<MaterialSymbolRegular>(iconName, out MaterialSymbolRegular icon))
                    {
                        continue;
                    }

                    icons.Add(new DisplayableIcon
                    {
                        Id = id++,
                        Name = iconName,
                        MaterialIcon = icon,
                        Symbol = ((char)icon).ToString(),
                        Code = ((int)icon).ToString("X4"),
                        IsMaterialIcon = true,
                        IsSegoeFluentIcon = false,
                        IsBootstrapIcon = false
                    });
                }
            }
            else if (SelectedFontType == "SegoeFluentIcons")
            {
                var names = Enum.GetNames(typeof(SegoeFluentSymbol));
                names = names.OrderBy(n => n).ToArray();

                foreach (string iconName in names)
                {
                    if (!Enum.TryParse<SegoeFluentSymbol>(iconName, out SegoeFluentSymbol icon))
                    {
                        continue;
                    }

                    icons.Add(new DisplayableIcon
                    {
                        Id = id++,
                        Name = iconName,
                        SegoeFluentIcon = icon,
                        Symbol = ((char)icon).ToString(),
                        Code = ((int)icon).ToString("X4"),
                        IsMaterialIcon = false,
                        IsSegoeFluentIcon = true,
                        IsBootstrapIcon = false
                    });
                }
            }
            else if (SelectedFontType == "BootstrapIcons")
            {
                var names = Enum.GetNames(typeof(BootstrapSymbolRegular));
                names = names.OrderBy(n => n).ToArray();

                foreach (string iconName in names)
                {
                    if (!Enum.TryParse<BootstrapSymbolRegular>(iconName, out BootstrapSymbolRegular icon))
                    {
                        continue;
                    }

                    icons.Add(new DisplayableIcon
                    {
                        Id = id++,
                        Name = iconName,
                        BootstrapIcon = icon,
                        Symbol = ((char)icon).ToString(),
                        Code = ((int)icon).ToString("X4"),
                        IsMaterialIcon = false,
                        IsSegoeFluentIcon = false,
                        IsBootstrapIcon = true
                    });
                }
            }
            else // FluentSystemIcons (default)
            {
                var names = Enum.GetNames(typeof(SymbolRegular));
                names = names.OrderBy(n => n).ToArray();

                foreach (string iconName in names)
                {
                    SymbolRegular icon = SymbolGlyph.Parse(iconName);

                    icons.Add(new DisplayableIcon
                    {
                        Id = id++,
                        Name = iconName,
                        Icon = icon,
                        Symbol = ((char)icon).ToString(),
                        Code = ((int)icon).ToString("X4"),
                        IsMaterialIcon = false,
                        IsSegoeFluentIcon = false,
                        IsBootstrapIcon = false
                    });
                }
            }

            IconsCollection = icons;
            FilteredIconsCollection = icons;
            IconNames = icons.Select(icon => icon.Name).ToList();

            if (icons.Count > 4)
            {
                _selectedIconId = 4;
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

    [RelayCommand]
    public void OnCheckboxChecked(object sender)
    {
        if (sender is not CheckBox checkbox)
        {
            return;
        }

        IsIconFilled = checkbox?.IsChecked ?? false;

        UpdateSymbolData();
    }

    private void UpdateSymbolData()
    {
        if (IconsCollection.Count - 1 < _selectedIconId)
        {
            return;
        }

        DisplayableIcon selectedSymbol = IconsCollection.FirstOrDefault(sym => sym.Id == _selectedIconId);

        if (selectedSymbol.IsMaterialIcon)
        {
            SelectedMaterialSymbol = selectedSymbol.MaterialIcon;
            SelectedSymbol = SymbolRegular.Empty;
            SelectedSegoeFluentSymbol = SegoeFluentSymbol.Empty;
            SelectedBootstrapSymbol = BootstrapSymbolRegular._123;
            SelectedSymbolName = selectedSymbol.Name;
            SelectedSymbolUnicodePoint = selectedSymbol.Code;
            SelectedSymbolTextGlyph = $"&#x{selectedSymbol.Code};";
            SelectedSymbolXaml = $"<ui:MaterialIcon Symbol=\"{selectedSymbol.Name}\"/>";
        }
        else if (selectedSymbol.IsSegoeFluentIcon)
        {
            SelectedSegoeFluentSymbol = selectedSymbol.SegoeFluentIcon;
            SelectedSymbol = SymbolRegular.Empty;
            SelectedMaterialSymbol = MaterialSymbolRegular.Empty;
            SelectedBootstrapSymbol = BootstrapSymbolRegular.Empty;
            SelectedSymbolName = selectedSymbol.Name;
            SelectedSymbolUnicodePoint = selectedSymbol.Code;
            SelectedSymbolTextGlyph = $"&#x{selectedSymbol.Code};";
            SelectedSymbolXaml = $"<ui:SegoeFluentIcon Symbol=\"{selectedSymbol.Name}\"/>";
        }
        else if (selectedSymbol.IsBootstrapIcon)
        {
            SelectedBootstrapSymbol = selectedSymbol.BootstrapIcon;
            SelectedSymbol = SymbolRegular.Empty;
            SelectedMaterialSymbol = MaterialSymbolRegular.Empty;
            SelectedSegoeFluentSymbol = SegoeFluentSymbol.Empty;
            SelectedSymbolName = selectedSymbol.Name;
            SelectedSymbolUnicodePoint = selectedSymbol.Code;
            SelectedSymbolTextGlyph = $"&#x{selectedSymbol.Code};";
            SelectedSymbolXaml = $"<ui:BootstrapIcon Symbol=\"{selectedSymbol.Name}\"/>";
        }
        else
        {
            SelectedSymbol = selectedSymbol.Icon;
            SelectedMaterialSymbol = MaterialSymbolRegular.Empty;
            SelectedSegoeFluentSymbol = SegoeFluentSymbol.Empty;
            SelectedBootstrapSymbol = BootstrapSymbolRegular.Empty;
            SelectedSymbolName = selectedSymbol.Name;
            SelectedSymbolUnicodePoint = selectedSymbol.Code;
            SelectedSymbolTextGlyph = $"&#x{selectedSymbol.Code};";
            SelectedSymbolXaml =
                $"<ui:SymbolIcon Symbol=\"{selectedSymbol.Name}\"{(IsIconFilled ? " Filled=\"True\"" : string.Empty)}/>";
        }
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
