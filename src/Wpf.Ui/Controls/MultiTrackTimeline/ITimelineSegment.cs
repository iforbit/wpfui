// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Wpf.Ui.Controls;

/// <summary>
/// 타임라인 세그먼트 인터페이스 (키프레임 간 구간)
/// KeyframeTimeline과 동일한 패턴: 독립적인 StartTime/EndTime
/// </summary>
public interface ITimelineSegment : INotifyPropertyChanged
{
    /// <summary>
    /// Gets or sets 시작 시간 (초)
    /// </summary>
    double StartTime { get; set; }

    /// <summary>
    /// Gets or sets 종료 시간 (초)
    /// </summary>
    double EndTime { get; set; }

    /// <summary>
    /// Gets or sets 시작 값 (보간 계산용)
    /// </summary>
    object? StartValue { get; set; }

    /// <summary>
    /// Gets or sets 종료 값 (보간 계산용)
    /// </summary>
    object? EndValue { get; set; }

    /// <summary>
    /// Gets 구간 길이 (초)
    /// </summary>
    double Duration { get; }

    /// <summary>
    /// Gets or sets 보간 타입
    /// </summary>
    InterpolationType Interpolation { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether 선택 상태
    /// </summary>
    bool IsSelected { get; set; }

    /// <summary>
    /// Gets or sets canvas X 좌표 (렌더링용)
    /// </summary>
    double CanvasX { get; set; }

    /// <summary>
    /// Gets or sets canvas 너비 (렌더링용)
    /// </summary>
    double CanvasWidth { get; set; }

    /// <summary>
    /// Gets or sets 세그먼트 브러시 (null이면 기본 브러시 사용)
    /// </summary>
    Brush? SegmentBrush { get; set; }

    /// <summary>
    /// Gets or sets 세그먼트 선택 시 브러시 (null이면 기본 브러시 사용)
    /// </summary>
    Brush? SegmentSelectedBrush { get; set; }

    /// <summary>
    /// Gets 중간값 리스트 (1→9→5 에서 9가 Waypoint)
    /// </summary>
    ObservableCollection<SegmentWaypoint> Waypoints { get; }

    /// <summary>
    /// Gets 툴팁 텍스트
    /// </summary>
    string? TooltipText { get; }

    /// <summary>
    /// 특정 시간에서의 보간된 값 계산
    /// </summary>
    object? GetInterpolatedValue(double time, TrackValueType valueType);
}
