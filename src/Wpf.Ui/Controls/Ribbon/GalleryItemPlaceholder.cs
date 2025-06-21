// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

internal class GalleryItemPlaceholder : UIElement
{
    /// <summary>
    /// Gets the target of the placeholder
    /// </summary>
    public UIElement Target { get; }

    public Size ArrangedSize { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GalleryItemPlaceholder"/> class.
    /// Constructor
    /// </summary>
    /// <param name="target">Target</param>
    public GalleryItemPlaceholder(UIElement target)
    {
        this.Target = target;
    }

    /// <inheritdoc />
    protected override Size MeasureCore(Size availableSize)
    {
        this.Target.Measure(availableSize);
        return this.Target.DesiredSize;
    }

    /// <inheritdoc />
    protected override void ArrangeCore(Rect finalRect)
    {
        base.ArrangeCore(finalRect);

        // Remember arranged size to arrange
        // targets in GalleryPanel lately
        this.ArrangedSize = finalRect.Size;
    }

    /* FOR DEGUG */

    // protected override void OnRender(DrawingContext drawingContext)
    // {
    //    drawingContext.DrawRectangle(null, new Pen(Brushes.Red, 1), new Rect(this.RenderSize));
    // }
}