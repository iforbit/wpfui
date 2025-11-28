// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.Controls;

namespace Wpf.Ui.Gallery.Views.Pages.DesignGuidance;

public partial class ElectricalTTFPage : INavigableView<ViewModels.Pages.DesignGuidance.ElectricalSymbolsViewModel>
{
    public ViewModels.Pages.DesignGuidance.ElectricalSymbolsViewModel ViewModel { get; }

    public ElectricalTTFPage(ViewModels.Pages.DesignGuidance.ElectricalSymbolsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}
