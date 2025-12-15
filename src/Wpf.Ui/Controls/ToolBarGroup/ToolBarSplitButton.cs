// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// A toolbar-optimized split button with compact height.
/// Designed for use within <see cref="ToolBarGroup"/>.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;ui:ToolBarGroup&gt;
///     &lt;ui:ToolBarSplitButton Icon="{ui:SymbolIcon Play20}" ToolTip="Run"&gt;
///         &lt;ui:ToolBarSplitButton.Flyout&gt;
///             &lt;ContextMenu&gt;
///                 &lt;MenuItem Header="Run Without Debugging" /&gt;
///                 &lt;MenuItem Header="Start Performance Profiler" /&gt;
///             &lt;/ContextMenu&gt;
///         &lt;/ui:ToolBarSplitButton.Flyout&gt;
///     &lt;/ui:ToolBarSplitButton&gt;
/// &lt;/ui:ToolBarGroup&gt;
/// </code>
/// </example>
public class ToolBarSplitButton : SplitButton
{
    static ToolBarSplitButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToolBarSplitButton),
            new FrameworkPropertyMetadata(typeof(ToolBarSplitButton))
        );
    }
}
