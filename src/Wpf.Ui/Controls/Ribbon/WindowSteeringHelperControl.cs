// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Controls.Helpers;
using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls;

/// <summary>
/// Helper control which enables easy embedding of window steering functions.
/// </summary>
public class WindowSteeringHelperControl : Border
{
    /// <summary>
    /// Initializes static members of the <see cref="WindowSteeringHelperControl"/> class.
    /// Static constructor
    /// </summary>
    static WindowSteeringHelperControl()
    {
        BackgroundProperty.OverrideMetadata(typeof(WindowSteeringHelperControl), new FrameworkPropertyMetadata(Brushes.Transparent));
        IsHitTestVisibleProperty.OverrideMetadata(typeof(WindowSteeringHelperControl), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
        HorizontalAlignmentProperty.OverrideMetadata(typeof(WindowSteeringHelperControl), new FrameworkPropertyMetadata(HorizontalAlignment.Stretch));
        VerticalAlignmentProperty.OverrideMetadata(typeof(WindowSteeringHelperControl), new FrameworkPropertyMetadata(VerticalAlignment.Stretch));
    }

    /// <inheritdoc />
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (this.IsEnabled)
        {
            WindowSteeringHelper.HandleMouseLeftButtonDown(e, true, true);
        }
    }

    /// <inheritdoc />
    protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseRightButtonUp(e);

        if (this.IsEnabled)
        {
            WindowSteeringHelper.ShowSystemMenu(this, e);
        }
    }
}