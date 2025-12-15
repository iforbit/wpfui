// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// A toolbar-optimized combo box with minimal padding for compact height.
/// Designed for use within <see cref="ToolBarGroup"/>.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;ui:ToolBarGroup&gt;
///     &lt;ui:ToolBarComboBox MinWidth="100" SelectedIndex="0"&gt;
///         &lt;ComboBoxItem Content="Debug" /&gt;
///         &lt;ComboBoxItem Content="Release" /&gt;
///     &lt;/ui:ToolBarComboBox&gt;
/// &lt;/ui:ToolBarGroup&gt;
/// </code>
/// </example>
public class ToolBarComboBox : System.Windows.Controls.ComboBox
{
    static ToolBarComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToolBarComboBox),
            new FrameworkPropertyMetadata(typeof(ToolBarComboBox))
        );
    }
}
