// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

/// <summary>
/// 키프레임 세그먼트 이벤트 인자
/// </summary>
public class KeyframeSegmentEventArgs : EventArgs
{
    /// <summary>
    /// Gets 선택된 세그먼트
    /// </summary>
    public KeyframeSegment Segment { get; }

    /// <summary>
    /// Gets 시작 시간 (초)
    /// </summary>
    public double StartTime => Segment.StartTime;

    /// <summary>
    /// Gets 종료 시간 (초)
    /// </summary>
    public double EndTime => Segment.EndTime;

    public KeyframeSegmentEventArgs(KeyframeSegment segment)
    {
        Segment = segment;
    }
}
