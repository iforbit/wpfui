// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

/// <summary>
/// MultiTrackTimeline 이벤트 인자
/// </summary>
public class MultiTrackEventArgs : EventArgs
{
    public ITimelineTrack Track { get; }

    public MultiTrackEventArgs(ITimelineTrack track)
    {
        Track = track;
    }
}
