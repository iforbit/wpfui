// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// Custom layout panel for <see cref="StepProgressBar"/> that arranges step items
/// and draws the connecting track lines between them.
/// </summary>
public class StepProgressBarPanel : Panel
{
    // Center points of each item's indicator, computed during Arrange and used in OnRender
    private readonly List<Point> _indicatorCenters = [];

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        StepProgressBar? owner = GetOwner();
        bool isHorizontal = GetIsHorizontal(owner);
        double spacing = owner?.StepSpacing ?? 24.0;

        double totalWidth = 0;
        double totalHeight = 0;
        double maxCross = 0;

        foreach (UIElement child in InternalChildren)
        {
            child.Measure(availableSize);
            Size desired = child.DesiredSize;

            if (isHorizontal)
            {
                totalWidth += desired.Width + spacing;
                maxCross = Math.Max(maxCross, desired.Height);
            }
            else
            {
                totalHeight += desired.Height + spacing;
                maxCross = Math.Max(maxCross, desired.Width);
            }
        }

        if (InternalChildren.Count > 0)
        {
            // Remove the last extra spacing
            if (isHorizontal)
                totalWidth -= spacing;
            else
                totalHeight -= spacing;
        }

        return isHorizontal
            ? new Size(totalWidth, maxCross)
            : new Size(maxCross, totalHeight);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        StepProgressBar? owner = GetOwner();
        bool isHorizontal = GetIsHorizontal(owner);
        double spacing = owner?.StepSpacing ?? 24.0;
        double indicatorSize = owner?.IndicatorSize ?? 28.0;

        _indicatorCenters.Clear();

        double offset = 0;

        foreach (UIElement child in InternalChildren)
        {
            Size desired = child.DesiredSize;

            Rect rect;
            if (isHorizontal)
            {
                double crossOffset = (finalSize.Height - desired.Height) / 2.0;
                rect = new Rect(offset, crossOffset, desired.Width, desired.Height);

                // The indicator center is at the middle of the indicator horizontally,
                // and at indicatorSize/2 from the top of the item
                double centerX = offset + desired.Width / 2.0;
                double centerY = crossOffset + indicatorSize / 2.0;
                _indicatorCenters.Add(new Point(centerX, centerY));

                offset += desired.Width + spacing;
            }
            else
            {
                double crossOffset = (finalSize.Width - desired.Width) / 2.0;
                rect = new Rect(crossOffset, offset, desired.Width, desired.Height);

                double centerX = crossOffset + desired.Width / 2.0;
                double centerY = offset + indicatorSize / 2.0;
                _indicatorCenters.Add(new Point(centerX, centerY));

                offset += desired.Height + spacing;
            }

            child.Arrange(rect);
        }

        return finalSize;
    }

    /// <inheritdoc/>
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (_indicatorCenters.Count < 2)
        {
            return;
        }

        StepProgressBar? owner = GetOwner();
        double trackThickness = owner?.TrackThickness ?? 2.0;
        int selectedIndex = owner?.SelectedIndex ?? -1;
        double indicatorRadius = (owner?.IndicatorSize ?? 28.0) / 2.0;
        bool isHorizontal = GetIsHorizontal(owner);

        // Small gap between line endpoint and indicator edge for visual clarity
        const double Gap = 4.0;
        double offset = indicatorRadius + Gap;

        Brush trackBrush = TryFindBrush("StepProgressBarTrackBrush") ?? Brushes.LightGray;
        Brush fillBrush = TryFindBrush("StepProgressBarActiveBrush") ?? SystemColors.HighlightBrush;

        var trackPen = new Pen(trackBrush, trackThickness) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var fillPen = new Pen(fillBrush, trackThickness) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };

        for (int i = 0; i < _indicatorCenters.Count - 1; i++)
        {
            Point from = _indicatorCenters[i];
            Point to = _indicatorCenters[i + 1];

            // Pull endpoints inward so the line runs between indicator edges, not through them
            if (isHorizontal)
            {
                from.X += offset;
                to.X -= offset;

                // Skip if there is no room (items too close together)
                if (from.X >= to.X)
                {
                    continue;
                }
            }
            else
            {
                from.Y += offset;
                to.Y -= offset;

                if (from.Y >= to.Y)
                {
                    continue;
                }
            }

            // Segment i is filled when selectedIndex > i (item i is Completed)
            Pen pen = selectedIndex > i ? fillPen : trackPen;
            drawingContext.DrawLine(pen, from, to);
        }
    }

    private StepProgressBar? GetOwner()
    {
        DependencyObject? parent = VisualTreeHelper.GetParent(this);
        while (parent is not null and not StepProgressBar)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }

        return parent as StepProgressBar;
    }

    private static bool GetIsHorizontal(StepProgressBar? owner) =>
        owner?.Orientation != System.Windows.Controls.Orientation.Vertical;

    private Brush? TryFindBrush(string key)
    {
        try
        {
            return FindResource(key) as Brush;
        }
        catch
        {
            return null;
        }
    }
}
