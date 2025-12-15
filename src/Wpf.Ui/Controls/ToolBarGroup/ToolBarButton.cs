// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// A toolbar-optimized button with transparent background and no border.
/// Designed for use within <see cref="ToolBarGroup"/>.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;ui:ToolBarGroup&gt;
///     &lt;ui:ToolBarButton Icon="{ui:SymbolIcon Copy20}" ToolTip="Copy" /&gt;
///     &lt;ui:ToolBarButton Icon="{ui:SymbolIcon Paste20}" ToolTip="Paste" /&gt;
/// &lt;/ui:ToolBarGroup&gt;
/// </code>
/// </example>
public class ToolBarButton : Button
{
    static ToolBarButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToolBarButton),
            new FrameworkPropertyMetadata(typeof(ToolBarButton))
        );
    }
}
