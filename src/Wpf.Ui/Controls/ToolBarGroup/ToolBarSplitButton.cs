// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// A toolbar-optimized split button with compact height.
/// Designed for use within <see cref="ToolBarGroup"/>.
/// The main button area and dropdown arrow have separate click behaviors.
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
    // SplitButton의 스타일을 상속받아 사용 (별도 DefaultStyleKey 없음)

    /// <inheritdoc />
    protected override void OnClick()
    {
        // ToggleButton 영역 클릭 시 Click 이벤트 발생 안함
        if (SplitButtonToggleButton != null && SplitButtonToggleButton.IsMouseOver)
        {
            return;
        }

        base.OnClick();
    }
}
