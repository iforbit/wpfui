// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls.Primitives;

namespace Wpf.Ui.Controls.Ribbon;

/// <summary>
/// Represents context menu resize mode
/// </summary>
public enum ContextMenuResizeMode
{
    /// <summary>
    /// Context menu can not be resized
    /// </summary>
    None = 0,

    /// <summary>
    /// Context menu can be only resized vertically
    /// </summary>
    Vertical,

    /// <summary>
    /// Context menu can be resized vertically and horizontally
    /// </summary>
    Both
}

/// <summary>
/// Represents a pop-up menu that enables a control
/// to expose functionality that is specific to the context of the control
/// </summary>
[TemplatePart(Name = "PART_ResizeVerticalThumb", Type = typeof(Thumb))]
[TemplatePart(Name = "PART_ResizeBothThumb", Type = typeof(Thumb))]
public class ContextMenu : System.Windows.Controls.ContextMenu
{
    // Thumb to resize in both directions
    private Thumb? resizeBothThumb;

    // Thumb to resize vertical
    private Thumb? resizeVerticalThumb;

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
        DependencyProperty.Register(nameof(ResizeMode), typeof(ContextMenuResizeMode),
            typeof(ContextMenu), new PropertyMetadata(ContextMenuResizeMode.None));

    /// <summary>
    /// Initializes static members of the <see cref="ContextMenu"/> class.
    /// Static constructor
    /// </summary>]
    static ContextMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ContextMenu), new FrameworkPropertyMetadata(typeof(ContextMenu)));
        FocusVisualStyleProperty.OverrideMetadata(typeof(ContextMenu), new FrameworkPropertyMetadata());
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        if (this.resizeVerticalThumb is not null)
        {
            this.resizeVerticalThumb.DragDelta -= this.OnResizeVerticalDelta;
        }

        this.resizeVerticalThumb = this.GetTemplateChild("PART_ResizeVerticalThumb") as Thumb;
        if (this.resizeVerticalThumb is not null)
        {
            this.resizeVerticalThumb.DragDelta += this.OnResizeVerticalDelta;
        }

        if (this.resizeBothThumb is not null)
        {
            this.resizeBothThumb.DragDelta -= this.OnResizeBothDelta;
        }

        this.resizeBothThumb = this.GetTemplateChild("PART_ResizeBothThumb") as Thumb;
        if (this.resizeBothThumb is not null)
        {
            this.resizeBothThumb.DragDelta += this.OnResizeBothDelta;
        }
    }

    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new MenuItem();
    }

    /// <inheritdoc />
    protected override bool IsItemItsOwnContainerOverride( object item )
    {
        return item is FrameworkElement;
    }

    // Handles resize both drag
    private void OnResizeBothDelta( object sender, DragDeltaEventArgs e )
    {
        if (double.IsNaN(this.Width))
        {
            this.SetCurrentValue(WidthProperty, this.ActualWidth);
        }

        if (double.IsNaN(this.Height))
        {
            this.SetCurrentValue(HeightProperty, this.ActualHeight);
        }

        this.SetCurrentValue(WidthProperty, Math.Max(this.MinWidth, this.Width + e.HorizontalChange));
        this.SetCurrentValue(HeightProperty, Math.Max(this.MinHeight, this.Height + e.VerticalChange));
    }

    // Handles resize vertical drag
    private void OnResizeVerticalDelta( object sender, DragDeltaEventArgs e )
    {
        if (double.IsNaN(this.Height))
        {
            this.SetCurrentValue(HeightProperty, this.ActualHeight);
        }

        this.SetCurrentValue(HeightProperty, Math.Max(this.MinHeight, this.Height + e.VerticalChange));
    }
}