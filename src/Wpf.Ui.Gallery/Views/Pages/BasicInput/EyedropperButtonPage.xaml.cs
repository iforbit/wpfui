// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Media;
using Wpf.Ui.Controls;
using Wpf.Ui.Gallery.ControlsLookup;
using Wpf.Ui.Gallery.ViewModels.Pages.BasicInput;

namespace Wpf.Ui.Gallery.Views.Pages.BasicInput;

[GalleryPage("Pick colors from screen.", SymbolRegular.Eyedropper24)]
public partial class EyedropperButtonPage : INavigableView<EyedropperButtonViewModel>
{
    public EyedropperButtonViewModel ViewModel { get; }

    public EyedropperButtonPage(EyedropperButtonViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    private void OnColorPicked(object? sender, Color color)
    {
        ColorBoard.Background = new SolidColorBrush(color);
        ColorText.Text = $"Selected: #{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    private void OnPickingCancelled(object? sender, EventArgs e)
    {
        ColorText.Text = "Picking cancelled.";
    }

    private void OnColorPicked2(object? sender, Color color)
    {
        ColorBoard2.Background = new SolidColorBrush(color);
    }
}
