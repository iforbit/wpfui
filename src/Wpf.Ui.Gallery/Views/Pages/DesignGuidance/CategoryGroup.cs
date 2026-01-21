// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;

namespace Wpf.Ui.Gallery.Views.Pages.DesignGuidance;

public class CategoryGroup
{
    public string CategoryName { get; set; } = string.Empty;

    public ObservableCollection<GeometryItem> Symbols { get; set; } = [];
}
