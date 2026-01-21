// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// A compact checkbox control for dense UI layouts.
/// Smaller than the default CheckBox (16x16 vs 22x22).
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;ui:CompactCheckBox Content="Compact option" /&gt;
/// </code>
/// </example>
public class CompactCheckBox : System.Windows.Controls.CheckBox
{
    static CompactCheckBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CompactCheckBox),
            new FrameworkPropertyMetadata(typeof(CompactCheckBox)));
    }
}
