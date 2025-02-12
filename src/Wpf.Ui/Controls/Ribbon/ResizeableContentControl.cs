// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls;

/// <inheritdoc />
[TemplatePart(Name = "PART_ResizeVerticalThumb", Type = typeof(Thumb))]
[TemplatePart(Name = "PART_ResizeBothThumb", Type = typeof(Thumb))]
public class ResizeableContentControl : ContentControl
{
    // Thumb to resize in both directions
    private Thumb? resizeBothThumb;

    // Thumb to resize vertical
    private Thumb? resizeVerticalThumb;

    static ResizeableContentControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeableContentControl), new FrameworkPropertyMetadata(typeof(ResizeableContentControl)));

        FocusableProperty.OverrideMetadata(typeof(ResizeableContentControl), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
        KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(ResizeableContentControl), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
        KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(ResizeableContentControl), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
    }

    /// <summary>
    /// Gets or sets context menu resize mode
    /// </summary>
    public ContextMenuResizeMode ResizeMode
    {
        get => (ContextMenuResizeMode)this.GetValue(ResizeModeProperty);
        set => this.SetValue(ResizeModeProperty, value);
    }

    /// <summary>Identifies the <see cref="ResizeMode"/> dependency property.</summary>
    public static readonly DependencyProperty ResizeModeProperty =
        DependencyProperty.Register(
            nameof(ResizeMode),
            typeof(ContextMenuResizeMode),
            typeof(ResizeableContentControl),
            new PropertyMetadata(ContextMenuResizeMode.None));

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (this.resizeVerticalThumb is not null)
        {
            this.resizeVerticalThumb.DragDelta -= this.OnResizeVerticalDelta;
        }

        if (this.resizeBothThumb is not null)
        {
            this.resizeBothThumb.DragDelta -= this.OnResizeBothDelta;
        }

        this.resizeVerticalThumb = this.GetTemplateChild("PART_ResizeVerticalThumb") as Thumb;

        this.resizeBothThumb = this.GetTemplateChild("PART_ResizeBothThumb") as Thumb;

        if (this.resizeVerticalThumb is not null)
        {
            this.resizeVerticalThumb.DragDelta += this.OnResizeVerticalDelta;
        }

        if (this.resizeBothThumb is not null)
        {
            this.resizeBothThumb.DragDelta += this.OnResizeBothDelta;
        }
    }

    private double GetResizeThumbHeight()
    {
        var height = this.ResizeMode switch
        {
            ContextMenuResizeMode.None => 0,
            ContextMenuResizeMode.Vertical => this.resizeVerticalThumb?.ActualHeight,
            ContextMenuResizeMode.Both => this.resizeBothThumb?.ActualHeight,
            _ => throw new ArgumentOutOfRangeException()
        };

        return height ?? 0;
    }

    // Handles resize both drag
    private void OnResizeBothDelta(object sender, DragDeltaEventArgs e)
    {
        if (double.IsNaN(this.Width))
        {
            this.SetCurrentValue(WidthProperty, this.ActualWidth);
        }

        if (double.IsNaN(this.Height))
        {
            this.SetCurrentValue(HeightProperty, this.ActualHeight);
        }

        this.SetCurrentValue(WidthProperty, Math.Max(this.MinWidth, Math.Min(this.MaxWidth, this.Width + e.HorizontalChange)));
        this.SetCurrentValue(HeightProperty, Math.Max(this.MinHeight, Math.Min(this.MaxHeight + this.GetResizeThumbHeight(), this.Height + e.VerticalChange)));
    }

    // Handles resize vertical drag
    private void OnResizeVerticalDelta(object sender, DragDeltaEventArgs e)
    {
        if (double.IsNaN(this.Height))
        {
            this.SetCurrentValue(HeightProperty, this.ActualHeight);
        }

        this.SetCurrentValue(HeightProperty, Math.Max(this.MinHeight, Math.Min(this.MaxHeight + this.GetResizeThumbHeight(), this.Height + e.VerticalChange)));
    }

    /// <summary>
    /// Gets a value indicating whether gets whether the mouse is over any of the resize thumbs.
    /// </summary>
    public bool IsMouseOverResizeThumbs => (this.resizeBothThumb?.IsMouseOver ?? false)
                                           || (this.resizeVerticalThumb?.IsMouseOver ?? false);
}