// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;

namespace Wpf.Ui.Controls;

public class DockedTabItem : TabItem
{
    public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
    nameof(Placement),
    typeof(Dock),
    typeof(DockedTabItem),
    new PropertyMetadata(Dock.Top));

    /// <summary>
    /// Gets or sets 탭 헤더의 위치를 나타냅니다. TabControl에서 설정됩니다.
    /// </summary>
    public Dock Placement
    {
        get => (Dock)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public static readonly DependencyProperty IsVisibleInStripProperty =
        DependencyProperty.Register(
            nameof(IsVisibleInStrip),
            typeof(bool),
            typeof(DockedTabItem),
            new PropertyMetadata(true));

    /// <summary>
    /// Gets or sets a value indicating whether 탭이 TabStrip에 보일지 여부
    /// </summary>
    public bool IsVisibleInStrip
    {
        get => (bool)GetValue(IsVisibleInStripProperty);
        set => SetValue(IsVisibleInStripProperty, value);
    }
}
