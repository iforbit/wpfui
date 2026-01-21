// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

/// <summary>
/// MultiTrackTimeline 세그먼트 이벤트 인자
/// </summary>
public class MultiTrackSegmentEventArgs : EventArgs
{
    /// <summary>
    /// Gets 선택된 세그먼트
    /// </summary>
    public ITimelineSegment Segment { get; }

    /// <summary>
    /// Gets 소속 트랙
    /// </summary>
    public ITimelineTrack? Track { get; }

    /// <summary>
    /// Gets 클릭한 시간 (세그먼트 내 상대 위치)
    /// </summary>
    public double ClickTime { get; }

    /// <summary>
    /// Gets 클릭 시간에서의 보간된 값
    /// </summary>
    public object? InterpolatedValue { get; }

    public MultiTrackSegmentEventArgs(
        ITimelineSegment segment,
        ITimelineTrack? track,
        double clickTime,
        object? interpolatedValue)
    {
        Segment = segment;
        Track = track;
        ClickTime = clickTime;
        InterpolatedValue = interpolatedValue;
    }
}
