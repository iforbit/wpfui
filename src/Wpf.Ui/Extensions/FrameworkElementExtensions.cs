// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Extensions;

/// <summary>
/// Class with extension methods for <see cref="FrameworkElement"/>.
/// </summary>
internal static class FrameworkElementExtensions
{
    public static void ForceMeasureImmediate( this FrameworkElement element )
    {
        // Calling anything on not loaded elements makes no sense
        if (element.IsLoaded is false)
        {
            return;
        }

        element.InvalidateMeasure();

        element.UpdateLayout();
    }

    public static void InvalidateMeasureAndArrange( this FrameworkElement element )
    {
        // Calling anything on not loaded elements makes no sense
        if (element.IsLoaded is false)
        {
            return;
        }

        element.InvalidateMeasure();
        element.InvalidateArrange();
    }

    public static void ForceMeasureAndArrangeImmediate( this FrameworkElement element )
    {
        // Calling anything on not loaded elements makes no sense
        if (element.IsLoaded is false)
        {
            return;
        }

        element.InvalidateMeasure();
        element.InvalidateArrange();

        element.UpdateLayout();
    }
}