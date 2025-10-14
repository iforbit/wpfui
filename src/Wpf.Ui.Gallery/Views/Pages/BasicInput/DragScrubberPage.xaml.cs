// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using Wpf.Ui.Controls;
using Wpf.Ui.Gallery.ControlsLookup;
using Wpf.Ui.Gallery.ViewModels.Pages.BasicInput;

namespace Wpf.Ui.Gallery.Views.Pages.BasicInput;

[GalleryPage("Unity Inspector style drag scrubber for numeric input.", SymbolRegular.Drag24)]
public partial class DragScrubberPage : Page, INavigableView<DragScrubberViewModel>
{
    public DragScrubberViewModel ViewModel { get; }

    public DragScrubberPage(DragScrubberViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();

        // DragScrubber 이벤트 핸들러 예제
        BasicDragScrubber.ValueChanged += OnDragScrubberValueChanged;
    }

    private void OnDragScrubberValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        // ValueChanged 이벤트 처리 예제
        System.Diagnostics.Debug.WriteLine($"DragScrubber value changed from {e.OldValue} to {e.NewValue}");
    }
}