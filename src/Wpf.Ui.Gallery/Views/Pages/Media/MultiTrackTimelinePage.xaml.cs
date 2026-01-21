// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using Wpf.Ui.Controls;
using Wpf.Ui.Gallery.ControlsLookup;
using Wpf.Ui.Gallery.ViewModels.Pages.Media;

namespace Wpf.Ui.Gallery.Views.Pages.Media;

[GalleryPage("Multi-track timeline for property-based animation editing.", SymbolRegular.Timeline24)]
public partial class MultiTrackTimelinePage : Page, INavigableView<MultiTrackTimelineViewModel>
{
    public MultiTrackTimelineViewModel ViewModel { get; }

    public MultiTrackTimelinePage(MultiTrackTimelineViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}
