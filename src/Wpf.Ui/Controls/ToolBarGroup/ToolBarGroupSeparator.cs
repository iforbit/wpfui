// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// A thin vertical separator used within a <see cref="ToolBarGroup"/> to divide items.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;ui:ToolBarGroup&gt;
///     &lt;ui:Button Icon="{ui:SymbolIcon Copy20}" /&gt;
///     &lt;ui:ToolBarGroupSeparator /&gt;
///     &lt;ui:Button Icon="{ui:SymbolIcon Paste20}" /&gt;
/// &lt;/ui:ToolBarGroup&gt;
/// </code>
/// </example>
public class ToolBarGroupSeparator : Separator
{
    static ToolBarGroupSeparator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToolBarGroupSeparator),
            new FrameworkPropertyMetadata(typeof(ToolBarGroupSeparator))
        );
    }
}
