// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

/// <summary>
/// Helper functions for classes implementing <see cref="ILogicalChildSupport"/>.
/// </summary>
public static class LogicalChildSupportHelper
{
    /// <summary>
    /// Called when <see cref="RibbonControl.IconProperty"/> changes.
    /// </summary>
    public static void OnLogicalChildPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        ILogicalChildSupport logicalChildSupport = d as ILogicalChildSupport ?? throw new ArgumentException("Argument must be of type ILogicalChildSupport.", nameof(d));

        if (e.OldValue is DependencyObject oldValue)
        {
            logicalChildSupport.RemoveLogicalChild(oldValue);
        }

        if (e.NewValue is DependencyObject newValue
            && LogicalTreeHelper.GetParent(newValue) is null)
        {
            logicalChildSupport.AddLogicalChild(newValue);
        }
    }
}