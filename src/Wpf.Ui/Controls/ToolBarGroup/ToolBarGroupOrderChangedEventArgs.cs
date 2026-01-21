// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// Provides data for the <see cref="ToolBarPanel.GroupOrderChanged"/> event.
/// </summary>
public class ToolBarGroupOrderChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the new order of toolbar groups as an array of indices.
    /// </summary>
    public int[] NewOrder { get; }

    /// <summary>
    /// Gets the row assignments for each group when multi-row mode is enabled.
    /// Null if multi-row mode is disabled.
    /// </summary>
    public int[]? RowAssignments { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolBarGroupOrderChangedEventArgs"/> class.
    /// </summary>
    /// <param name="newOrder">The new order of toolbar groups.</param>
    /// <param name="rowAssignments">Optional row assignments for multi-row mode.</param>
    public ToolBarGroupOrderChangedEventArgs(int[] newOrder, int[]? rowAssignments = null)
    {
        NewOrder = newOrder;
        RowAssignments = rowAssignments;
    }
}
