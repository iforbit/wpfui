// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Markup;
using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents group separator menu item
/// </summary>
[ContentProperty(nameof(Header))]
public class GroupSeparatorMenuItem : MenuItem
{
    static GroupSeparatorMenuItem()
    {
        Type type = typeof(GroupSeparatorMenuItem);
        DefaultStyleKeyProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(type));
        IsEnabledProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, null, CoerceIsEnabledAndTabStop));
        IsTabStopProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, null, CoerceIsEnabledAndTabStop));
    }

    private static object CoerceIsEnabledAndTabStop(DependencyObject d, object? basevalue)
    {
        return BooleanBoxes.FalseBox;
    }
}