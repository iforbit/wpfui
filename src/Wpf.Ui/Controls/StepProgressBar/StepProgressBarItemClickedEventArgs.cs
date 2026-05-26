// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// Provides data for the <see cref="StepProgressBar.StepClicked"/> event.
/// </summary>
public sealed class StepProgressBarItemClickedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Initializes a new instance of <see cref="StepProgressBarItemClickedEventArgs"/>.
    /// </summary>
    public StepProgressBarItemClickedEventArgs(RoutedEvent routedEvent, object source, object item, int index)
        : base(routedEvent, source)
    {
        Item = item;
        Index = index;
    }

    /// <summary>
    /// Gets the content of the <see cref="StepProgressBarItem"/> that was clicked.
    /// </summary>
    public object Item { get; }

    /// <summary>
    /// Gets the zero-based index of the item that was clicked.
    /// </summary>
    public int Index { get; }
}
