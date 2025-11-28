// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using Wpf.Ui.Internal;
using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents separator to use in the TabControl
/// </summary>
public class SeparatorTabItem : TabItem
{
    /// <summary>
    /// Initializes static members of the <see cref="SeparatorTabItem"/> class.
    /// Static constructor
    /// </summary>
    static SeparatorTabItem()
    {
        Type type = typeof(SeparatorTabItem);

        DefaultStyleKeyProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(type));
        IsEnabledProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, null, CoerceIsEnabledAndTabStop));
        IsTabStopProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, null, CoerceIsEnabledAndTabStop));
        IsSelectedProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, OnIsSelectedChanged));
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue == false)
        {
            return;
        }

        var separatorTabItem = (SeparatorTabItem)d;
        TabControl? tabControl = UIHelper.GetParent<TabControl>(separatorTabItem);

        if (tabControl is null
            || tabControl.Items.Count <= 1)
        {
            return;
        }

        tabControl.SelectedIndex = tabControl.SelectedIndex == tabControl.Items.Count - 1
            ? tabControl.SelectedIndex - 1
            : tabControl.SelectedIndex + 1;
    }

    private static object CoerceIsEnabledAndTabStop(DependencyObject d, object? basevalue)
    {
        return BooleanBoxes.FalseBox;
    }
}