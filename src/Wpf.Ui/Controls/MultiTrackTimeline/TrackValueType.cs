// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

/// <summary>
/// Track의 값 유형 (UI 렌더링 방식 결정)
/// </summary>
public enum TrackValueType
{
    /// <summary>double 값 - TextBox/Slider</summary>
    Numeric,

    /// <summary>색상 (#RRGGBB) - ColorPicker</summary>
    Color,

    /// <summary>Enum 값 - ComboBox (Step 보간)</summary>
    Enum,

    /// <summary>bool 값 - Toggle</summary>
    Boolean,

    /// <summary>문자열 값 - TextBox</summary>
    String
}
