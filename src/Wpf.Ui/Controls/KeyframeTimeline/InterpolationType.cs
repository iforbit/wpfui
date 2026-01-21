// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

/// <summary>
/// 보간(Interpolation) 타입
/// </summary>
public enum InterpolationType
{
    /// <summary>선형 보간</summary>
    Linear,

    /// <summary>시작 시 가속</summary>
    EaseIn,

    /// <summary>끝에서 감속</summary>
    EaseOut,

    /// <summary>시작/끝 가감속</summary>
    EaseInOut,

    /// <summary>보간 없음 (즉시 전환)</summary>
    Hold,

    /// <summary>커스텀 (Bezier 등)</summary>
    Custom
}
