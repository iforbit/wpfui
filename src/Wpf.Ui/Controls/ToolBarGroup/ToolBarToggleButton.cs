// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls.Primitives;

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// A toolbar-optimized toggle button with transparent background and no border.
/// Designed for use within <see cref="ToolBarGroup"/>.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;ui:ToolBarGroup&gt;
///     &lt;ui:ToolBarToggleButton ToolTip="Bold"&gt;
///         &lt;ui:SymbolIcon Symbol="TextBold20" /&gt;
///     &lt;/ui:ToolBarToggleButton&gt;
/// &lt;/ui:ToolBarGroup&gt;
/// </code>
/// </example>
public class ToolBarToggleButton : ToggleButton
{
    static ToolBarToggleButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToolBarToggleButton),
            new FrameworkPropertyMetadata(typeof(ToolBarToggleButton))
        );
    }
}
