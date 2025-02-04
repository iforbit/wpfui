// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using Wpf.Ui.Internal;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents contextual groups container
/// </summary>
public class RibbonContextualGroupsContainer : Panel
{
    private readonly List<Size> sizes = new();

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var finalRect = new Rect(finalSize);
        var index = 0;

        foreach (UIElement? item in this.InternalChildren)
        {
            finalRect.Width = this.sizes[index].Width; // item.DesiredSize.Width;
            finalRect.Height = Math.Max(finalSize.Height, this.sizes[index].Height); // Math.Max(finalSize.Height, item.DesiredSize.Height);
            item?.Arrange(finalRect);
            finalRect.X += this.sizes[index].Width; // item.DesiredSize.Width;
            index++;
        }

        return finalSize;
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var allGroupsWidth = 0D;
        this.sizes.Clear();

        var availableSizeHeight = availableSize.Height;

        if (double.IsPositiveInfinity(availableSizeHeight))
        {
            availableSizeHeight = 0;
        }

        foreach (RibbonContextualTabGroup? contextualGroup in this.InternalChildren)
        {
            if (contextualGroup is null)
            {
                continue;
            }

            // Calculate width of tab items of the group
            var tabsWidth = 0D;

            // We have to look at visible and items which already got measured only
            var visibleItems = contextualGroup.Items.Where(item => item.Visibility == Visibility.Visible && DoubleUtil.AreClose(item.DesiredSize.Width, 0) == false).ToList();

            foreach (RibbonTabItem? item in visibleItems)
            {
                tabsWidth += item.DesiredSize.Width;
            }

            contextualGroup.Measure(new Size(tabsWidth, availableSizeHeight));

            // Calc final width and measure the group using it
            var finalWidth = tabsWidth;
            allGroupsWidth += finalWidth;

            if (allGroupsWidth > availableSize.Width)
            {
                finalWidth -= allGroupsWidth - availableSize.Width;
                allGroupsWidth = availableSize.Width;
            }

            contextualGroup.Measure(new Size(Math.Max(0, finalWidth), availableSizeHeight));
            this.sizes.Add(new Size(Math.Max(0, finalWidth), availableSizeHeight));
        }

        return new Size(allGroupsWidth, availableSizeHeight);
    }
}