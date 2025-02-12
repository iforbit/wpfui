// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls;

/// <summary>
/// Provides additional tooltip functionality.
/// </summary>
public static class ToolTipService
{
    /// <summary>
    /// Attach ooltip properties to control.
    /// </summary>
    /// <param name="type">Control type.</param>
    public static void Attach(Type type)
    {
        System.Windows.Controls.ToolTipService.ShowOnDisabledProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
        System.Windows.Controls.ToolTipService.InitialShowDelayProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(900));
        System.Windows.Controls.ToolTipService.BetweenShowDelayProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(IntBoxes.Zero));
        System.Windows.Controls.ToolTipService.ShowDurationProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(20000));
    }
}
