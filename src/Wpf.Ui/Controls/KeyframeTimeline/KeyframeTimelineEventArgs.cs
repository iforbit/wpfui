// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

/// <summary>
/// 키프레임 타임라인 이벤트 인자
/// </summary>
public class KeyframeTimelineEventArgs : EventArgs
{
    /// <summary>
    /// Gets 현재 시간 (초)
    /// </summary>
    public double Time { get; }

    /// <summary>
    /// Gets 선택된 키프레임 포인트 (없으면 null)
    /// </summary>
    public KeyframePoint? SelectedPoint { get; }

    public KeyframeTimelineEventArgs(double time, KeyframePoint? selectedPoint = null)
    {
        Time = time;
        SelectedPoint = selectedPoint;
    }
}
