// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Internal;

namespace Wpf.Ui.Controls.Ribbon;

public class RibbonScrollViewer : ScrollViewer
{
    protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
    {
        if (this.VisualChildrenCount > 0
            && this.GetVisualChild(0) is { } firstVisualChild)
        {
            return VisualTreeHelper.HitTest(firstVisualChild, hitTestParameters.HitPoint);
        }

        return base.HitTestCore(hitTestParameters);
    }

    /// <inheritdoc />
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        if (this.ScrollInfo != null)
        {
            var horizontalOffsetBefore = this.ScrollInfo.HorizontalOffset;
            var verticalOffsetBefore = this.ScrollInfo.VerticalOffset;

            if (e.Delta < 0)
            {
                this.ScrollInfo.MouseWheelDown();
            }
            else
            {
                this.ScrollInfo.MouseWheelUp();
            }

            e.Handled = DoubleUtil.AreClose(horizontalOffsetBefore, this.ScrollInfo.HorizontalOffset) == false
                        || DoubleUtil.AreClose(verticalOffsetBefore, this.ScrollInfo.VerticalOffset) == false;
        }
    }
}
