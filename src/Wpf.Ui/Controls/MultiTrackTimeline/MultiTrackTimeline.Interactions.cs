// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Wpf.Ui.Controls;

/// <summary>
/// MultiTrackTimeline - Click Handling, Drag Handling, Zoom/Pan
/// </summary>
public partial class MultiTrackTimeline
{
    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not FrameworkElement element)
        {
            return;
        }

        // Scrubber/ViewportThumb이면 무시
        Thumb? thumb = FindParent<Thumb>(element);
        if (thumb == _scrubber || thumb == _viewportThumb)
        {
            return;
        }

        // 1. Keyframe Thumb 클릭 처리
        if (thumb != null && thumb.Tag is ITimelineKeyframe keyframe)
        {
            SetCurrentValue(SelectedKeyframeProperty, keyframe);

            ITimelineTrack? track = FindTrackForKeyframe(keyframe);
            if (track != null)
            {
                SetCurrentValue(SelectedTrackProperty, track);
            }

            return;
        }

        // 2. Segment Handle Thumb 클릭 (Tag="Left" 또는 "Right")
        if (thumb != null && thumb.Tag is string handleTag && (handleTag == "Left" || handleTag == "Right"))
        {
            Grid? parentGrid = FindParent<Grid>(thumb);
            if (parentGrid?.Tag is ITimelineSegment handleSegment)
            {
                Point clickPos = e.GetPosition(parentGrid);
                OnSegmentClicked(handleSegment, clickPos.X);
                return;
            }
        }

        // 3. Segment Body 클릭 처리 - Tag 또는 DataContext 체크
        if (element.Tag is ITimelineSegment directSegment)
        {
            OnSegmentClicked(directSegment, 0);
            return;
        }

        if (element.DataContext is ITimelineSegment dcSegment)
        {
            OnSegmentClicked(dcSegment, 0);
            return;
        }

        // 4. 비주얼 트리 탐색 (fallback)
        (ITimelineSegment? segment, Grid? segmentContainer) = FindSegmentFromVisualTree(element);
        if (segment != null)
        {
            Point clickPos = segmentContainer != null ? e.GetPosition(segmentContainer) : new Point(0, 0);
            OnSegmentClicked(segment, clickPos.X);
            return;
        }

        // 5. Track 선택 처리
        ITimelineTrack? trackFromClick = FindTrackFromElement(element);
        if (trackFromClick != null)
        {
            SetCurrentValue(SelectedTrackProperty, trackFromClick);
            SetCurrentValue(SelectedKeyframeProperty, null);
            SetCurrentValue(SelectedSegmentProperty, null);
        }
    }

    /// <summary>
    /// 좌측 트랙 목록 클릭 처리
    /// </summary>
    private void OnTrackListPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not FrameworkElement element)
        {
            return;
        }

        ITimelineTrack? track = FindTrackFromElement(element);
        if (track != null)
        {
            SetCurrentValue(SelectedTrackProperty, track);
            SetCurrentValue(SelectedKeyframeProperty, null);
            SetCurrentValue(SelectedSegmentProperty, null);
            return;
        }

        ITimelineTrackGroup? group = FindGroupFromElement(element);
        if (group != null)
        {
            group.IsExpanded = !group.IsExpanded;
        }
    }

    private void OnThumbDragStarted(object sender, DragStartedEventArgs e)
    {
        Thumb? thumb = e.OriginalSource as Thumb ?? FindParent<Thumb>(e.OriginalSource as DependencyObject);

        if (thumb == null || thumb == _scrubber || thumb == _viewportThumb)
        {
            return;
        }

        // Keyframe 드래그
        if (thumb.Tag is ITimelineKeyframe keyframe)
        {
            _isDraggingKeyframe = true;
            _draggingKeyframe = keyframe;
            _draggingTrack = FindTrackForKeyframe(keyframe);

            SetCurrentValue(SelectedKeyframeProperty, keyframe);
            if (_draggingTrack != null)
            {
                SetCurrentValue(SelectedTrackProperty, _draggingTrack);
            }

            return;
        }

        // Waypoint 드래그
        if (thumb.Tag is SegmentWaypoint waypoint)
        {
            _isDraggingWaypoint = true;
            _draggingWaypoint = waypoint;
            _draggingSegment = waypoint.ParentSegment ?? FindSegmentForWaypoint(waypoint);
            _draggingTrack = _draggingSegment != null ? FindTrackForSegment(_draggingSegment) : null;
            return;
        }

        // Segment Edge 드래그
        if (thumb.Tag is string handleType)
        {
            ITimelineSegment? segment = thumb.DataContext as ITimelineSegment;
            if (segment == null && thumb.Parent is FrameworkElement parent)
            {
                (ITimelineSegment? foundSegment, _) = FindSegmentFromVisualTree(parent);
                segment = foundSegment;
            }

            if (segment != null)
            {
                _draggingSegment = segment;
                _draggingTrack = FindTrackForSegment(segment);

                if (handleType == "Left")
                {
                    _isDraggingSegmentLeft = true;
                }
                else if (handleType == "Right")
                {
                    _isDraggingSegmentRight = true;
                }

                SetCurrentValue(SelectedSegmentProperty, segment);
                if (_draggingTrack != null)
                {
                    SetCurrentValue(SelectedTrackProperty, _draggingTrack);
                }
            }
        }
    }

    private void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        Thumb? thumb = e.OriginalSource as Thumb ?? FindParent<Thumb>(e.OriginalSource as DependencyObject);

        if (thumb == _scrubber || thumb == _viewportThumb || _trackCanvas == null)
        {
            return;
        }

        double canvasWidth = _trackCanvas.ActualWidth;
        if (canvasWidth <= 0)
        {
            return;
        }

        // Keyframe 드래그
        if (_isDraggingKeyframe && _draggingKeyframe != null)
        {
            HandleKeyframeDrag(e, canvasWidth);
            return;
        }

        // Waypoint 드래그
        if (_isDraggingWaypoint && _draggingWaypoint?.ParentSegment != null)
        {
            HandleWaypointDrag(e, canvasWidth);
            return;
        }

        // Segment Left Edge 드래그
        if (_isDraggingSegmentLeft && _draggingSegment != null)
        {
            HandleSegmentLeftDrag(e, canvasWidth);
            return;
        }

        // Segment Right Edge 드래그
        if (_isDraggingSegmentRight && _draggingSegment != null)
        {
            HandleSegmentRightDrag(e, canvasWidth);
        }
    }

    private void HandleKeyframeDrag(DragDeltaEventArgs e, double canvasWidth)
    {
        double currentX = TimeToX(_draggingKeyframe!.Time, canvasWidth);
        double newX = Math.Clamp(currentX + e.HorizontalChange, 0, canvasWidth);
        double newTime = Math.Clamp(XToTime(newX, canvasWidth), StartTime, EndTime);

        if (_draggingTrack != null)
        {
            (double minTime, double maxTime) = GetKeyframeMoveBounds(_draggingTrack, _draggingKeyframe);
            newTime = Math.Clamp(newTime, minTime, maxTime);
        }

        _draggingKeyframe.Time = newTime;
        _draggingKeyframe.CanvasX = TimeToX(newTime, canvasWidth);

        // 세그먼트 위치 업데이트
        if (_draggingTrack != null)
        {
            foreach (ITimelineSegment segment in _draggingTrack.Segments)
            {
                double startX = TimeToX(segment.StartTime, canvasWidth);
                double endX = TimeToX(segment.EndTime, canvasWidth);
                segment.CanvasX = startX;
                segment.CanvasWidth = Math.Max(0, endX - startX);

                foreach (SegmentWaypoint waypoint in segment.Waypoints)
                {
                    waypoint.CanvasX = waypoint.RelativePosition * segment.CanvasWidth;
                }
            }
        }

        KeyframeDragged?.Invoke(this, new MultiTrackKeyframeEventArgs(_draggingKeyframe, _draggingTrack, newTime));
    }

    private void HandleWaypointDrag(DragDeltaEventArgs e, double canvasWidth)
    {
        ITimelineSegment segment = _draggingWaypoint!.ParentSegment!;

        double deltaTime = (e.HorizontalChange / canvasWidth) * ViewportDuration;
        double newAbsTime = _draggingWaypoint.AbsoluteTime + deltaTime;

        const double margin = 0.02;
        newAbsTime = Math.Clamp(newAbsTime, segment.StartTime + margin, segment.EndTime - margin);

        double segmentDuration = segment.Duration;
        if (segmentDuration > 0)
        {
            double newRelPos = (newAbsTime - segment.StartTime) / segmentDuration;
            _draggingWaypoint.RelativePosition = Math.Clamp(newRelPos, 0.01, 0.99);
        }
    }

    private void HandleSegmentLeftDrag(DragDeltaEventArgs e, double canvasWidth)
    {
        double deltaTime = (e.HorizontalChange / canvasWidth) * ViewportDuration;
        double newStartTime = _draggingSegment!.StartTime + deltaTime;

        double minStart = StartTime;
        double maxStart = _draggingSegment.EndTime - 0.01;

        if (_draggingTrack != null)
        {
            (double minFromKeyframes, double maxFromKeyframes) = GetSegmentStartBounds(_draggingTrack, _draggingSegment);
            minStart = Math.Max(minStart, minFromKeyframes);
            maxStart = Math.Min(maxStart, maxFromKeyframes);
        }

        newStartTime = Math.Clamp(newStartTime, minStart, maxStart);
        PushWaypointsFromStart(_draggingSegment, newStartTime);

        _draggingSegment.StartTime = newStartTime;

        double startX = TimeToX(newStartTime, canvasWidth);
        double endX = TimeToX(_draggingSegment.EndTime, canvasWidth);
        _draggingSegment.CanvasX = startX;
        _draggingSegment.CanvasWidth = Math.Max(0, endX - startX);

        SegmentDragged?.Invoke(this, new MultiTrackSegmentEventArgs(_draggingSegment, _draggingTrack, newStartTime, null));
    }

    private void HandleSegmentRightDrag(DragDeltaEventArgs e, double canvasWidth)
    {
        double deltaTime = (e.HorizontalChange / canvasWidth) * ViewportDuration;
        double newEndTime = _draggingSegment!.EndTime + deltaTime;

        double minEnd = _draggingSegment.StartTime + 0.01;
        double maxEnd = EndTime;

        if (_draggingTrack != null)
        {
            (double minFromKeyframes, double maxFromKeyframes) = GetSegmentEndBounds(_draggingTrack, _draggingSegment);
            minEnd = Math.Max(minEnd, minFromKeyframes);
            maxEnd = Math.Min(maxEnd, maxFromKeyframes);
        }

        newEndTime = Math.Clamp(newEndTime, minEnd, maxEnd);
        PushWaypointsFromEnd(_draggingSegment, newEndTime);

        _draggingSegment.EndTime = newEndTime;

        double startX = TimeToX(_draggingSegment.StartTime, canvasWidth);
        double endX = TimeToX(newEndTime, canvasWidth);
        _draggingSegment.CanvasX = startX;
        _draggingSegment.CanvasWidth = Math.Max(0, endX - startX);

        SegmentDragged?.Invoke(this, new MultiTrackSegmentEventArgs(_draggingSegment, _draggingTrack, newEndTime, null));
    }

    private void OnThumbDragCompleted(object sender, DragCompletedEventArgs e)
    {
        Thumb? thumb = e.OriginalSource as Thumb ?? FindParent<Thumb>(e.OriginalSource as DependencyObject);

        if (thumb == _scrubber || thumb == _viewportThumb)
        {
            return;
        }

        _isDraggingKeyframe = false;
        _isDraggingSegmentLeft = false;
        _isDraggingSegmentRight = false;
        _isDraggingWaypoint = false;
        _draggingKeyframe = null;
        _draggingSegment = null;
        _draggingWaypoint = null;
        _draggingTrack = null;
    }

    private void OnTrackCanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_trackCanvas == null || Duration <= 0)
        {
            return;
        }

        Point pos = e.GetPosition(_trackCanvas);
        double canvasWidth = _trackCanvas.ActualWidth;

        if (canvasWidth > 0)
        {
            SetCurrentValue(CurrentTimeProperty, XToTime(pos.X, canvasWidth));
        }
    }

    private void OnTimeRulerMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!IsZoomEnabled || _trackCanvas == null)
        {
            return;
        }

        Point mousePos = e.GetPosition(_trackCanvas);
        double canvasWidth = _trackCanvas.ActualWidth;
        double mouseTime = XToTime(mousePos.X, canvasWidth);

        double zoomFactor = e.Delta > 0 ? 0.8 : 1.25;
        double newViewportDuration = Math.Clamp(ViewportDuration * zoomFactor, 0.1, 60.0);

        double mouseRatio = mousePos.X / canvasWidth;
        double newViewportStart = mouseTime - (newViewportDuration * mouseRatio);
        newViewportStart = Math.Clamp(newViewportStart, StartTime, Math.Max(StartTime, EndTime - newViewportDuration));

        SetCurrentValue(ViewportDurationProperty, newViewportDuration);
        SetCurrentValue(ViewportStartProperty, newViewportStart);

        e.Handled = true;
    }

    private void OnTrackCanvasMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_trackCanvas == null)
        {
            return;
        }

        _isPanning = true;
        _panStartPoint = e.GetPosition(_trackCanvas);
        _panStartViewportStart = ViewportStart;
        _ = _trackCanvas.CaptureMouse();
        e.Handled = true;
    }

    private void OnTrackCanvasMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isPanning && _trackCanvas != null)
        {
            _isPanning = false;
            _trackCanvas.ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    private void OnTrackCanvasMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isPanning || _trackCanvas == null)
        {
            return;
        }

        Point currentPos = e.GetPosition(_trackCanvas);
        double canvasWidth = _trackCanvas.ActualWidth;

        if (canvasWidth <= 0)
        {
            return;
        }

        double deltaX = _panStartPoint.X - currentPos.X;
        double deltaTime = (deltaX / canvasWidth) * ViewportDuration;

        double newStart = Math.Clamp(
            _panStartViewportStart + deltaTime,
            StartTime,
            Math.Max(StartTime, EndTime - ViewportDuration));

        SetCurrentValue(ViewportStartProperty, newStart);
    }

    private void OnTimeRulerMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_timeRuler == null)
        {
            return;
        }

        _isRulerPanning = true;
        _rulerPanStartPoint = e.GetPosition(_timeRuler);
        _rulerPanStartViewportStart = ViewportStart;
        _ = _timeRuler.CaptureMouse();
        e.Handled = true;
    }

    private void OnTimeRulerMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isRulerPanning && _timeRuler != null)
        {
            _isRulerPanning = false;
            _timeRuler.ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    private void OnTimeRulerMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isRulerPanning || _timeRuler == null)
        {
            return;
        }

        Point currentPos = e.GetPosition(_timeRuler);
        double rulerWidth = _timeRuler.ActualWidth;

        if (rulerWidth <= 0)
        {
            return;
        }

        double deltaX = _rulerPanStartPoint.X - currentPos.X;
        double deltaTime = (deltaX / rulerWidth) * ViewportDuration;

        double newStart = Math.Clamp(
            _rulerPanStartViewportStart + deltaTime,
            StartTime,
            Math.Max(StartTime, EndTime - ViewportDuration));

        SetCurrentValue(ViewportStartProperty, newStart);
    }

    private static T? FindParent<T>(DependencyObject? child)
        where T : DependencyObject
    {
        DependencyObject? parent = child != null ? VisualTreeHelper.GetParent(child) : null;
        while (parent != null)
        {
            if (parent is T typedParent)
            {
                return typedParent;
            }

            parent = VisualTreeHelper.GetParent(parent);
        }

        return null;
    }

    private (ITimelineSegment? Segment, Grid? Container) FindSegmentFromVisualTree(FrameworkElement element)
    {
        DependencyObject? current = element;
        ITimelineSegment? foundSegment = null;
        Grid? foundGrid = null;

        while (current != null)
        {
            if (current is FrameworkElement fe)
            {
                if (fe == _trackCanvas)
                {
                    break;
                }

                if (fe is Grid g)
                {
                    if (g.Tag is ITimelineSegment tagSegment)
                    {
                        return (tagSegment, g);
                    }

                    if (g.DataContext is ITimelineSegment dcSegment)
                    {
                        return (dcSegment, g);
                    }
                }

                if (foundSegment == null && fe.DataContext is ITimelineSegment segment)
                {
                    foundSegment = segment;
                }
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return (foundSegment, foundGrid);
    }

    private ITimelineTrack? FindTrackFromElement(FrameworkElement element)
    {
        if (element.Tag is ITimelineTrack tagTrack)
        {
            return tagTrack;
        }

        if (element.DataContext is ITimelineTrack track && IsValidTrack(track))
        {
            return track;
        }

        DependencyObject parent = VisualTreeHelper.GetParent(element);
        while (parent != null)
        {
            if (parent is FrameworkElement fe)
            {
                if (fe.Tag is ITimelineTrack parentTagTrack)
                {
                    return parentTagTrack;
                }

                if (fe.DataContext is ITimelineTrack parentTrack && IsValidTrack(parentTrack))
                {
                    return parentTrack;
                }
            }

            parent = VisualTreeHelper.GetParent(parent);
        }

        return null;
    }

    private ITimelineTrackGroup? FindGroupFromElement(FrameworkElement element)
    {
        if (element.DataContext is ITimelineTrackGroup group && TrackGroups?.Contains(group) == true)
        {
            return group;
        }

        DependencyObject parent = VisualTreeHelper.GetParent(element);
        while (parent != null)
        {
            if (parent is FrameworkElement fe && fe.DataContext is ITimelineTrackGroup parentGroup)
            {
                if (TrackGroups?.Contains(parentGroup) == true)
                {
                    return parentGroup;
                }
            }

            parent = VisualTreeHelper.GetParent(parent);
        }

        return null;
    }

    private bool IsValidTrack(ITimelineTrack track)
    {
        if (TrackGroups == null)
        {
            return false;
        }

        foreach (ITimelineTrackGroup group in TrackGroups)
        {
            if (group.Tracks.Contains(track))
            {
                return true;
            }
        }

        return false;
    }

    private ITimelineTrack? FindTrackForKeyframe(ITimelineKeyframe keyframe)
    {
        if (TrackGroups == null)
        {
            return null;
        }

        foreach (ITimelineTrackGroup group in TrackGroups)
        {
            foreach (ITimelineTrack track in group.Tracks)
            {
                if (track.Keyframes.Contains(keyframe))
                {
                    return track;
                }
            }
        }

        return null;
    }

    private ITimelineTrack? FindTrackForSegment(ITimelineSegment segment)
    {
        if (TrackGroups == null)
        {
            return null;
        }

        foreach (ITimelineTrackGroup group in TrackGroups)
        {
            foreach (ITimelineTrack track in group.Tracks)
            {
                if (track.Segments.Contains(segment))
                {
                    return track;
                }
            }
        }

        return null;
    }

    private ITimelineSegment? FindSegmentForWaypoint(SegmentWaypoint waypoint)
    {
        if (TrackGroups == null)
        {
            return null;
        }

        foreach (ITimelineTrackGroup group in TrackGroups)
        {
            foreach (ITimelineTrack track in group.Tracks)
            {
                foreach (ITimelineSegment segment in track.Segments)
                {
                    if (segment.Waypoints.Contains(waypoint))
                    {
                        return segment;
                    }
                }
            }
        }

        return null;
    }
}
