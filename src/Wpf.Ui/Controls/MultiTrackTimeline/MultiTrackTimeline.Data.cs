// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

/// <summary>
/// MultiTrackTimeline - CRUD Operations, Collision Detection, Waypoint Pushing
/// </summary>
public partial class MultiTrackTimeline
{
    /// <summary>
    /// 특정 트랙에 키프레임 추가
    /// </summary>
    public ITimelineKeyframe AddKeyframe(ITimelineTrack track, double time, object? value)
    {
        var keyframe = new TimelineKeyframe(Math.Clamp(time, StartTime, EndTime), value);
        track.Keyframes.Add(keyframe);
        return keyframe;
    }

    /// <summary>
    /// 현재 시간에 키프레임 추가 (현재 값 사용)
    /// </summary>
    public ITimelineKeyframe AddKeyframeAtCurrentTime(ITimelineTrack track)
    {
        return AddKeyframe(track, CurrentTime, track.CurrentValue);
    }

    /// <summary>
    /// 키프레임 제거
    /// </summary>
    public bool RemoveKeyframe(ITimelineTrack track, ITimelineKeyframe keyframe)
    {
        if (SelectedKeyframe == keyframe)
        {
            SetCurrentValue(SelectedKeyframeProperty, null);
        }

        return track.Keyframes.Remove(keyframe);
    }

    /// <summary>
    /// 트랙의 모든 키프레임 제거
    /// </summary>
    public void ClearKeyframes(ITimelineTrack track)
    {
        SetCurrentValue(SelectedKeyframeProperty, null);
        track.Keyframes.Clear();
    }

    /// <summary>
    /// 특정 트랙에 세그먼트 추가
    /// </summary>
    public ITimelineSegment AddSegment(
        ITimelineTrack track,
        double startTime,
        double endTime,
        object? startValue = null,
        object? endValue = null,
        InterpolationType interpolation = InterpolationType.Linear)
    {
        startTime = Math.Clamp(startTime, StartTime, EndTime);
        endTime = Math.Clamp(endTime, StartTime, EndTime);

        if (endTime <= startTime)
        {
            endTime = startTime + 0.1;
        }

        var segment = new TimelineSegment(
            startTime,
            endTime,
            startValue ?? track.CurrentValue,
            endValue ?? track.CurrentValue,
            interpolation);

        track.Segments.Add(segment);
        return segment;
    }

    /// <summary>
    /// 현재 시간 기준으로 세그먼트 추가
    /// </summary>
    public ITimelineSegment AddSegmentAtCurrentTime(ITimelineTrack track, double duration = 1.0)
    {
        return AddSegment(track, CurrentTime, CurrentTime + duration, track.CurrentValue, track.CurrentValue);
    }

    /// <summary>
    /// 세그먼트 제거
    /// </summary>
    public bool RemoveSegment(ITimelineTrack track, ITimelineSegment segment)
    {
        if (SelectedSegment == segment)
        {
            SetCurrentValue(SelectedSegmentProperty, null);
        }

        return track.Segments.Remove(segment);
    }

    /// <summary>
    /// 트랙의 모든 세그먼트 제거
    /// </summary>
    public void ClearSegments(ITimelineTrack track)
    {
        SetCurrentValue(SelectedSegmentProperty, null);
        track.Segments.Clear();
    }

    /// <summary>
    /// 세그먼트에 웨이포인트 추가 (상대 위치 기준)
    /// </summary>
    public SegmentWaypoint AddWaypoint(ITimelineSegment segment, double relativePosition, object? value)
    {
        relativePosition = Math.Clamp(relativePosition, 0.01, 0.99);

        var waypoint = new SegmentWaypoint(relativePosition, value)
        {
            ParentSegment = segment
        };

        segment.Waypoints.Add(waypoint);
        return waypoint;
    }

    /// <summary>
    /// 세그먼트에 웨이포인트 추가 (절대 시간 기준)
    /// </summary>
    public SegmentWaypoint? AddWaypointAtTime(ITimelineSegment segment, double absoluteTime, object? value)
    {
        if (absoluteTime <= segment.StartTime || absoluteTime >= segment.EndTime)
        {
            return null;
        }

        double relativePosition = (absoluteTime - segment.StartTime) / segment.Duration;
        return AddWaypoint(segment, relativePosition, value);
    }

    /// <summary>
    /// 현재 시간에 웨이포인트 추가
    /// </summary>
    public SegmentWaypoint? AddWaypointAtCurrentTime(ITimelineSegment segment, object? value)
    {
        return AddWaypointAtTime(segment, CurrentTime, value);
    }

    /// <summary>
    /// 웨이포인트 제거
    /// </summary>
    public bool RemoveWaypoint(ITimelineSegment segment, SegmentWaypoint waypoint)
    {
        return segment.Waypoints.Remove(waypoint);
    }

    /// <summary>
    /// 세그먼트의 모든 웨이포인트 제거
    /// </summary>
    public void ClearWaypoints(ITimelineSegment segment)
    {
        segment.Waypoints.Clear();
    }

    /// <summary>
    /// Keyframe이 이동할 수 있는 시간 범위 계산 (Segment 경계 고려)
    /// </summary>
    private (double MinTime, double MaxTime) GetKeyframeMoveBounds(ITimelineTrack track, ITimelineKeyframe keyframe)
    {
        double minTime = StartTime;
        double maxTime = EndTime;

        foreach (ITimelineSegment segment in track.Segments)
        {
            if (keyframe.Time < segment.StartTime)
            {
                maxTime = Math.Min(maxTime, segment.StartTime - CollisionMargin);
            }
            else if (keyframe.Time > segment.EndTime)
            {
                minTime = Math.Max(minTime, segment.EndTime + CollisionMargin);
            }
            else
            {
                // Keyframe이 Segment 내부에 있는 경우 (비정상 상태)
                double distToStart = keyframe.Time - segment.StartTime;
                double distToEnd = segment.EndTime - keyframe.Time;
                if (distToStart <= distToEnd)
                {
                    maxTime = Math.Min(maxTime, segment.StartTime - CollisionMargin);
                }
                else
                {
                    minTime = Math.Max(minTime, segment.EndTime + CollisionMargin);
                }
            }
        }

        return (minTime, maxTime);
    }

    /// <summary>
    /// Segment의 StartTime이 이동할 수 있는 범위 계산
    /// </summary>
    private (double MinStart, double MaxStart) GetSegmentStartBounds(ITimelineTrack track, ITimelineSegment segment)
    {
        double minStart = StartTime;
        double maxStart = segment.EndTime - CollisionMargin;

        foreach (ITimelineKeyframe keyframe in track.Keyframes)
        {
            if (keyframe.Time < segment.StartTime)
            {
                double boundary = keyframe.Time + CollisionMargin;
                if (boundary > minStart)
                {
                    minStart = boundary;
                }
            }
            else if (keyframe.Time > segment.StartTime && keyframe.Time < segment.EndTime)
            {
                double boundary = keyframe.Time - CollisionMargin;
                if (boundary < maxStart)
                {
                    maxStart = boundary;
                }
            }
        }

        return (minStart, maxStart);
    }

    /// <summary>
    /// Segment의 EndTime이 이동할 수 있는 범위 계산
    /// </summary>
    private (double MinEnd, double MaxEnd) GetSegmentEndBounds(ITimelineTrack track, ITimelineSegment segment)
    {
        double minEnd = segment.StartTime + CollisionMargin;
        double maxEnd = EndTime;

        foreach (ITimelineKeyframe keyframe in track.Keyframes)
        {
            if (keyframe.Time > segment.EndTime)
            {
                double boundary = keyframe.Time - CollisionMargin;
                if (boundary < maxEnd)
                {
                    maxEnd = boundary;
                }
            }
            else if (keyframe.Time > segment.StartTime && keyframe.Time < segment.EndTime)
            {
                double boundary = keyframe.Time + CollisionMargin;
                if (boundary > minEnd)
                {
                    minEnd = boundary;
                }
            }
        }

        return (minEnd, maxEnd);
    }

    /// <summary>
    /// Segment StartTime이 오른쪽으로 이동할 때 Waypoints를 밀어냄
    /// </summary>
    private void PushWaypointsFromStart(ITimelineSegment segment, double newStartTime)
    {
        const double margin = 0.02;
        double segmentDuration = segment.EndTime - newStartTime;

        if (segmentDuration <= 0)
        {
            return;
        }

        foreach (SegmentWaypoint waypoint in segment.Waypoints)
        {
            double waypointAbsTime = segment.StartTime + (waypoint.RelativePosition * segment.Duration);

            if (waypointAbsTime < newStartTime + margin)
            {
                double newRelPos = margin / segmentDuration;
                waypoint.RelativePosition = Math.Clamp(newRelPos, 0.01, 0.99);
            }
            else
            {
                double newRelPos = (waypointAbsTime - newStartTime) / segmentDuration;
                waypoint.RelativePosition = Math.Clamp(newRelPos, 0.01, 0.99);
            }
        }
    }

    /// <summary>
    /// Segment EndTime이 왼쪽으로 이동할 때 Waypoints를 밀어냄
    /// </summary>
    private void PushWaypointsFromEnd(ITimelineSegment segment, double newEndTime)
    {
        const double margin = 0.02;
        double segmentDuration = newEndTime - segment.StartTime;

        if (segmentDuration <= 0)
        {
            return;
        }

        foreach (SegmentWaypoint waypoint in segment.Waypoints)
        {
            double waypointAbsTime = segment.StartTime + (waypoint.RelativePosition * segment.Duration);

            if (waypointAbsTime > newEndTime - margin)
            {
                double newRelPos = (segmentDuration - margin) / segmentDuration;
                waypoint.RelativePosition = Math.Clamp(newRelPos, 0.01, 0.99);
            }
            else
            {
                double newRelPos = (waypointAbsTime - segment.StartTime) / segmentDuration;
                waypoint.RelativePosition = Math.Clamp(newRelPos, 0.01, 0.99);
            }
        }
    }

    /// <summary>
    /// 세그먼트 클릭 처리 (외부 호출용)
    /// </summary>
    public void OnSegmentClicked(ITimelineSegment segment, double clickX)
    {
        if (_trackCanvas == null)
        {
            return;
        }

        ITimelineTrack? track = FindTrackForSegment(segment);

        // 클릭 시간 계산
        double clickRatio = segment.CanvasWidth > 0 ? clickX / segment.CanvasWidth : 0;
        double clickTime = segment.StartTime + (segment.Duration * clickRatio);

        // 보간된 값 계산
        TrackValueType valueType = track?.ValueType ?? TrackValueType.Numeric;
        var interpolatedValue = segment.GetInterpolatedValue(clickTime, valueType);

        // 선택 및 클릭 시간 설정
        SetCurrentValue(SelectedSegmentProperty, segment);
        SetCurrentValue(SegmentClickTimeProperty, clickTime);

        // Track의 CurrentValue 업데이트
        if (track != null)
        {
            track.CurrentValue = interpolatedValue;
            SetCurrentValue(SelectedTrackProperty, track);
        }

        // 이벤트 발생
        SegmentSelected?.Invoke(this, new MultiTrackSegmentEventArgs(segment, track, clickTime, interpolatedValue));
    }
}
