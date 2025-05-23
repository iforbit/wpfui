// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;

namespace Wpf.Ui.Controls;

public class DockedTabControl : TabControl
{
    static DockedTabControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DockedTabControl),
            new FrameworkPropertyMetadata(typeof(DockedTabControl)));
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is DockedTabItem tabItem)
        {
            // TabStripPlacement 값을 Dock enum 으로 변환하여 전달
            tabItem.Placement = TabStripPlacement switch
            {
                Dock.Top => Dock.Top,
                Dock.Bottom => Dock.Bottom,
                Dock.Left => Dock.Left,
                Dock.Right => Dock.Right,
                _ => Dock.Top
            };
        }
    }
}
