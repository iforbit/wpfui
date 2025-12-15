// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using Wpf.Ui.Controls;
using Wpf.Ui.Gallery.ControlsLookup;
using Wpf.Ui.Gallery.ViewModels.Pages.Media;

namespace Wpf.Ui.Gallery.Views.Pages.Media;

[GalleryPage("Keyframe timeline control for animation editing.", SymbolRegular.Timeline24)]
public partial class KeyframeTimelinePage : Page, INavigableView<KeyframeTimelineViewModel>
{
    public KeyframeTimelineViewModel ViewModel { get; }

    public KeyframeTimelinePage(KeyframeTimelineViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();

        // InteractiveTimeline uses ViewModel.KeyframePoints via binding
        // No manual keyframe addition needed

        // Add keyframes to preview timeline (not bound to ViewModel)
        _ = PreviewTimeline.AddKeyframePoint(0.0);
        _ = PreviewTimeline.AddKeyframePoint(1.0);
        _ = PreviewTimeline.AddKeyframePoint(2.0);
    }

    private void OnPreviewTimelineCurrentTimeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        // Animate preview shape based on current time
        // Example: Rotate 360 degrees over 2 seconds
        double progress = e.NewValue / PreviewTimeline.Duration;
        double angle = progress * 360;
        PreviewRotation.SetCurrentValue(System.Windows.Media.RotateTransform.AngleProperty, angle);
    }
}
