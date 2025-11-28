// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Gallery.Views.Pages.DesignGuidance;

public partial class ElectricalGeometryPage
{
    public ObservableCollection<GeometryItem> ResistorSymbols { get; } = [];

    public ObservableCollection<GeometryItem> CapacitorSymbols { get; } = [];

    public ObservableCollection<GeometryItem> TransistorSymbols { get; } = [];

    public ObservableCollection<GeometryItem> DiodeSymbols { get; } = [];

    public ObservableCollection<CategoryGroup> AllCategories { get; } = [];

    public ElectricalGeometryPage()
    {
        DataContext = this;
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("=== ElectricalGeometryPage Loaded ===");
        System.Diagnostics.Debug.WriteLine($"App.Resources.MergedDictionaries.Count: {Application.Current.Resources.MergedDictionaries.Count}");

        // Test if we can find a specific geometry
        var testGeometry = TryFindResource("Resistors.Attenuator");
        System.Diagnostics.Debug.WriteLine($"Test Geometry (Resistors.Attenuator): {testGeometry?.GetType().Name ?? "NULL"}");

        LoadAllGeometries();

        System.Diagnostics.Debug.WriteLine($"ResistorSymbols.Count: {ResistorSymbols.Count}");
        System.Diagnostics.Debug.WriteLine($"CapacitorSymbols.Count: {CapacitorSymbols.Count}");
    }

    private void LoadAllGeometries()
    {
        // Load specific categories
        LoadCategory("Resistors", ResistorSymbols);
        LoadCategory("Capacitors", CapacitorSymbols);
        LoadCategory("Transistors", TransistorSymbols);
        LoadCategory("Diodes", DiodeSymbols);

        // Load all categories for "All" tab
        (string, int)[] categoryNames = new[]
        {
            ("Abstract", 21), ("Capacitors", 16), ("Diodes", 13),
            ("Electro-Mechanical", 68), ("Iec417", 45), ("Iec Logic Gates", 9),
            ("Inductors", 39), ("Instruments", 5), ("Logic Gates", 25),
            ("Miscellaneous", 67), ("Mosfets1", 19), ("Mosfets2", 15),
            ("Op Amps", 10), ("Opto Electronics", 15), ("Plc Ladder", 6),
            ("Power Semiconductors", 21), ("Radio", 11), ("Resistors", 23),
            ("Rot Mech", 15), ("Signal Sources", 18), ("Thermionic Devices", 7),
            ("Transistors", 23), ("Transmission", 13), ("Waveforms", 23)
        };
        foreach ((string categoryName, _) in categoryNames)
        {
            var symbols = new ObservableCollection<GeometryItem>();
            LoadCategory(categoryName, symbols);

            if (symbols.Count > 0)
            {
                AllCategories.Add(new CategoryGroup
                {
                    CategoryName = $"{categoryName} ({symbols.Count})",
                    Symbols = symbols
                });
            }
        }
    }

    private void LoadCategory(string categoryPrefix, ObservableCollection<GeometryItem> collection)
    {
        // Search through Application.Resources recursively
        ResourceDictionary appResources = Application.Current.Resources;
        SearchDictionary(appResources, categoryPrefix, collection);
    }

    private void SearchDictionary(ResourceDictionary dict, string categoryPrefix, ObservableCollection<GeometryItem> collection)
    {
        // Search direct keys in this dictionary
        foreach (var key in dict.Keys)
        {
            if (key is string keyStr && keyStr.StartsWith(categoryPrefix + ".", StringComparison.OrdinalIgnoreCase))
            {
                if (dict[key] is Geometry geometry)
                {
                    var name = keyStr.Substring(categoryPrefix.Length + 1)
                        .Replace('_', ' ')
                        .Replace(',', ' ');

                    collection.Add(new GeometryItem
                    {
                        Name = name,
                        Geometry = geometry
                    });
                }
            }
        }

        // Recursively search merged dictionaries
        foreach (ResourceDictionary? mergedDict in dict.MergedDictionaries)
        {
            if (mergedDict != null)
            {
                SearchDictionary(mergedDict, categoryPrefix, collection);
            }
        }
    }
}

public class GeometryItem
{
    public string Name { get; set; } = string.Empty;

    public Geometry? Geometry { get; set; }
}

public class CategoryGroup
{
    public string CategoryName { get; set; } = string.Empty;

    public ObservableCollection<GeometryItem> Symbols { get; set; } = [];
}
