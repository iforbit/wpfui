// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

/// <summary>
/// 타임라인 키프레임 인터페이스
/// </summary>
public interface ITimelineKeyframe : INotifyPropertyChanged
{
    /// <summary>
    /// Gets or sets 키프레임 시간 (초)
    /// </summary>
    double Time { get; set; }

    /// <summary>
    /// Gets or sets 키프레임 값 (타입은 Track.ValueType에 따름)
    /// </summary>
    object? Value { get; set; }

    /// <summary>
    /// Gets or sets 보간 방식 (이 키프레임에서 다음 키프레임까지)
    /// </summary>
    InterpolationType Interpolation { get; set; }

    /// <summary>
    /// Gets or sets 커스텀 Bezier 제어점 (Interpolation이 Custom일 때)
    /// 형식: "x1,y1,x2,y2" (0~1 범위)
    /// </summary>
    string? BezierControlPoints { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether 선택 상태
    /// </summary>
    bool IsSelected { get; set; }

    /// <summary>
    /// Gets or sets canvas X 좌표 (내부 렌더링용)
    /// </summary>
    double CanvasX { get; set; }
}
