// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

/// <summary>
/// MultiTrackTimeline 키프레임 이벤트 인자
/// </summary>
public class MultiTrackKeyframeEventArgs : EventArgs
{
    public ITimelineKeyframe Keyframe { get; }

    public ITimelineTrack? Track { get; }

    public double Time { get; }

    public MultiTrackKeyframeEventArgs(ITimelineKeyframe keyframe, ITimelineTrack? track, double time)
    {
        Keyframe = keyframe;
        Track = track;
        Time = time;
    }
}
