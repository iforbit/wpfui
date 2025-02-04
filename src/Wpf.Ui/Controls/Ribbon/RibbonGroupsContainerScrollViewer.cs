// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using System.Windows.Input;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents a <see cref="ScrollViewer" /> specific to <see cref="RibbonGroupsContainer" />.
/// </summary>
public class RibbonGroupsContainerScrollViewer : ScrollViewer
{
    static RibbonGroupsContainerScrollViewer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonGroupsContainerScrollViewer), new FrameworkPropertyMetadata(typeof(RibbonGroupsContainerScrollViewer)));
        VerticalScrollBarVisibilityProperty.OverrideMetadata(typeof(RibbonGroupsContainerScrollViewer), new FrameworkPropertyMetadata(ScrollBarVisibility.Disabled));
    }

    /// <inheritdoc />
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        if (this.ScrollInfo is null)
        {
            return;
        }

        // Prevent scrolling when a popup is open
        if (Mouse.Captured is IDropDownControl { IsDropDownOpen: true, DropDownPopup: not null } and not RibbonTabControl)
        {
            return;
        }

        if (e.Delta < 0)
        {
            this.LineRight();
        }
        else
        {
            this.LineLeft();
        }

        e.Handled = true;
    }
}