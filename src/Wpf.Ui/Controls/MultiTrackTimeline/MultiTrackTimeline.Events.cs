// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Wpf.Ui.Controls;

/// <summary>
/// MultiTrackTimeline - Property Changed Callbacks, OnApplyTemplate, Scroll Synchronization
/// </summary>
public partial class MultiTrackTimeline
{
    private static void OnTrackGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MultiTrackTimeline timeline)
        {
            return;
        }

        if (e.OldValue is ObservableCollection<ITimelineTrackGroup> oldCollection)
        {
            oldCollection.CollectionChanged -= timeline.OnTrackGroupsCollectionChanged;
            foreach (ITimelineTrackGroup group in oldCollection)
            {
                timeline.UnsubscribeFromGroup(group);
            }
        }

        if (e.NewValue is ObservableCollection<ITimelineTrackGroup> newCollection)
        {
            newCollection.CollectionChanged += timeline.OnTrackGroupsCollectionChanged;
            foreach (ITimelineTrackGroup group in newCollection)
            {
                timeline.SubscribeToGroup(group);
            }
        }

        timeline.RefreshAllTracks();
    }

    private void OnTrackGroupsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (ITimelineTrackGroup group in e.OldItems)
            {
                UnsubscribeFromGroup(group);
            }
        }

        if (e.NewItems != null)
        {
            foreach (ITimelineTrackGroup group in e.NewItems)
            {
                SubscribeToGroup(group);
            }
        }

        RefreshAllTracks();
    }

    private void SubscribeToGroup(ITimelineTrackGroup group)
    {
        group.Tracks.CollectionChanged += OnTracksCollectionChanged;
        foreach (ITimelineTrack track in group.Tracks)
        {
            SubscribeToTrack(track);
        }
    }

    private void UnsubscribeFromGroup(ITimelineTrackGroup group)
    {
        group.Tracks.CollectionChanged -= OnTracksCollectionChanged;
        foreach (ITimelineTrack track in group.Tracks)
        {
            UnsubscribeFromTrack(track);
        }
    }

    private void OnTracksCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (ITimelineTrack track in e.OldItems)
            {
                UnsubscribeFromTrack(track);
            }
        }

        if (e.NewItems != null)
        {
            foreach (ITimelineTrack track in e.NewItems)
            {
                SubscribeToTrack(track);
            }
        }

        RefreshTimeline();
    }

    private void SubscribeToTrack(ITimelineTrack track)
    {
        track.Keyframes.CollectionChanged += OnKeyframesCollectionChanged;
        track.Segments.CollectionChanged += OnSegmentsCollectionChanged;
    }

    private void UnsubscribeFromTrack(ITimelineTrack track)
    {
        track.Keyframes.CollectionChanged -= OnKeyframesCollectionChanged;
        track.Segments.CollectionChanged -= OnSegmentsCollectionChanged;
    }

    private void OnKeyframesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Converter가 Time 바인딩을 통해 자동으로 위치 계산
    }

    private void OnSegmentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Converter가 StartTime/EndTime 바인딩을 통해 자동으로 위치/너비 계산
    }

    private static void OnTimeRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MultiTrackTimeline timeline)
        {
            return;
        }

        if (timeline.ViewportDuration > timeline.Duration)
        {
            timeline.SetCurrentValue(ViewportDurationProperty, timeline.Duration);
        }

        timeline.CoerceValue(ViewportStartProperty);
        timeline.CoerceValue(CurrentTimeProperty);
        timeline.RefreshTimeline();
    }

    private static void OnCurrentTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MultiTrackTimeline timeline)
        {
            return;
        }

        var oldValue = (double)e.OldValue;
        var newValue = (double)e.NewValue;

        timeline.UpdateScrubberPosition();
        timeline.UpdateCurrentValues();
        timeline.RaiseEvent(new RoutedPropertyChangedEventArgs<double>(oldValue, newValue, CurrentTimeChangedEvent));
    }

    private static object CoerceCurrentTime(DependencyObject d, object? value)
    {
        if (d is MultiTrackTimeline timeline && value is double time)
        {
            return Math.Clamp(time, timeline.StartTime, timeline.EndTime);
        }

        return value ?? 0.0;
    }

    private static void OnSelectedTrackChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MultiTrackTimeline timeline)
        {
            return;
        }

        if (e.OldValue is ITimelineTrack oldTrack)
        {
            oldTrack.IsSelected = false;
        }

        if (e.NewValue is ITimelineTrack newTrack)
        {
            newTrack.IsSelected = true;
            timeline.TrackSelected?.Invoke(timeline, new MultiTrackEventArgs(newTrack));

            if (timeline.TrackSelectedCommand?.CanExecute(newTrack) == true)
            {
                timeline.TrackSelectedCommand.Execute(newTrack);
            }
        }
    }

    private static void OnSelectedKeyframeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MultiTrackTimeline timeline)
        {
            return;
        }

        if (e.OldValue is ITimelineKeyframe oldKeyframe)
        {
            oldKeyframe.IsSelected = false;
        }

        if (e.NewValue is ITimelineKeyframe newKeyframe)
        {
            newKeyframe.IsSelected = true;
            timeline.KeyframeSelected?.Invoke(timeline, new MultiTrackKeyframeEventArgs(newKeyframe, null, newKeyframe.Time));

            if (timeline.KeyframeSelectedCommand?.CanExecute(newKeyframe) == true)
            {
                timeline.KeyframeSelectedCommand.Execute(newKeyframe);
            }

            // 키프레임 선택 시 세그먼트 선택 해제
            timeline.SetCurrentValue(SelectedSegmentProperty, null);
        }
    }

    private static void OnSelectedSegmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MultiTrackTimeline timeline)
        {
            return;
        }

        if (e.OldValue is ITimelineSegment oldSegment)
        {
            oldSegment.IsSelected = false;
        }

        if (e.NewValue is ITimelineSegment newSegment)
        {
            newSegment.IsSelected = true;

            // 세그먼트 선택 시 키프레임 선택 해제
            timeline.SetCurrentValue(SelectedKeyframeProperty, null);
        }
    }

    private static void OnViewportChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MultiTrackTimeline timeline)
        {
            timeline.RefreshTimeline();
        }
    }

    private static object CoerceViewportStart(DependencyObject d, object? value)
    {
        if (d is MultiTrackTimeline timeline && value is double start)
        {
            double maxStart = Math.Max(timeline.StartTime, timeline.EndTime - timeline.ViewportDuration);
            return Math.Clamp(start, timeline.StartTime, maxStart);
        }

        return value ?? 0.0;
    }

    private static object CoerceViewportDuration(DependencyObject d, object? value)
    {
        if (value is double duration)
        {
            return Math.Clamp(duration, 0.1, 60.0);
        }

        return value ?? 1.0;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // Unsubscribe from previous template parts
        UnsubscribeFromTemplateParts();

        // Get template parts
        _timeRuler = GetTemplateChild(PART_TimeRuler) as Canvas;
        _trackListPanel = GetTemplateChild(PART_TrackListPanel) as ItemsControl;
        _trackCanvas = GetTemplateChild(PART_TrackCanvas) as Canvas;
        _scrubber = GetTemplateChild(PART_Scrubber) as Thumb;
        _trackScrollViewer = GetTemplateChild(PART_TrackScrollViewer) as ScrollViewer;
        _keyframeScrollViewer = GetTemplateChild(PART_KeyframeScrollViewer) as DynamicScrollViewer;
        _viewportThumb = GetTemplateChild(PART_ViewportThumb) as Thumb;

        // Subscribe to template parts
        SubscribeToTemplateParts();

        // Initial rendering
        Loaded += (s, e) => RefreshTimeline();
        SizeChanged += (s, e) => RefreshTimeline();
    }

    private void UnsubscribeFromTemplateParts()
    {
        if (_scrubber != null)
        {
            _scrubber.DragStarted -= OnScrubberDragStarted;
            _scrubber.DragDelta -= OnScrubberDragDelta;
            _scrubber.DragCompleted -= OnScrubberDragCompleted;
        }

        if (_viewportThumb != null)
        {
            _viewportThumb.DragDelta -= OnViewportThumbDragDelta;
        }

        if (_trackScrollViewer != null)
        {
            _trackScrollViewer.ScrollChanged -= OnTrackScrollViewerScrollChanged;
            _trackScrollViewer.PreviewMouseLeftButtonDown -= OnTrackListPreviewMouseLeftButtonDown;
        }

        if (_keyframeScrollViewer != null)
        {
            _keyframeScrollViewer.ScrollChanged -= OnKeyframeScrollViewerScrollChanged;
        }
    }

    private void SubscribeToTemplateParts()
    {
        // Scrubber events
        if (_scrubber != null)
        {
            _scrubber.DragStarted += OnScrubberDragStarted;
            _scrubber.DragDelta += OnScrubberDragDelta;
            _scrubber.DragCompleted += OnScrubberDragCompleted;
        }

        // Viewport Thumb events
        if (_viewportThumb != null)
        {
            _viewportThumb.DragDelta += OnViewportThumbDragDelta;
            _viewportTrack = _viewportThumb.Parent as FrameworkElement;
        }

        // Scroll synchronization
        if (_trackScrollViewer != null)
        {
            _trackScrollViewer.ScrollChanged += OnTrackScrollViewerScrollChanged;
            _trackScrollViewer.PreviewMouseLeftButtonDown += OnTrackListPreviewMouseLeftButtonDown;
        }

        if (_keyframeScrollViewer != null)
        {
            _keyframeScrollViewer.ScrollChanged += OnKeyframeScrollViewerScrollChanged;
        }

        // Track Canvas events
        if (_trackCanvas != null)
        {
            _trackCanvas.MouseLeftButtonDown += OnTrackCanvasMouseLeftButtonDown;
            _trackCanvas.MouseRightButtonDown += OnTrackCanvasMouseRightButtonDown;
            _trackCanvas.MouseRightButtonUp += OnTrackCanvasMouseRightButtonUp;
            _trackCanvas.MouseMove += OnTrackCanvasMouseMove;
            _trackCanvas.SizeChanged += OnTrackCanvasSizeChanged;

            if (_trackCanvas.ActualWidth > 0)
            {
                SetValue(CanvasActualWidthPropertyKey, _trackCanvas.ActualWidth);
            }
        }

        // Keyframe/Segment Thumb drag events (bubbling)
        AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler(OnThumbDragStarted));
        AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnThumbDragDelta));
        AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnThumbDragCompleted));

        // Click events
        AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnPreviewMouseLeftButtonDown));

        // Time Ruler events
        if (_timeRuler != null)
        {
            _timeRuler.MouseWheel += OnTimeRulerMouseWheel;
            _timeRuler.MouseLeftButtonDown += OnTimeRulerMouseLeftButtonDown;
            _timeRuler.MouseLeftButtonUp += OnTimeRulerMouseLeftButtonUp;
            _timeRuler.MouseMove += OnTimeRulerMouseMove;
        }
    }

    private void OnTrackCanvasSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width > 0)
        {
            SetValue(CanvasActualWidthPropertyKey, e.NewSize.Width);
            UpdateScrubberPosition();
        }
    }

    private void OnScrubberDragStarted(object sender, DragStartedEventArgs e)
    {
        // Handler intentionally empty - drag delta handles movement
    }

    private void OnScrubberDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_trackCanvas == null || Duration <= 0)
        {
            return;
        }

        double canvasWidth = _trackCanvas.ActualWidth;
        if (canvasWidth <= 0)
        {
            return;
        }

        double currentX = TimeToX(CurrentTime, canvasWidth);
        double newX = Math.Clamp(currentX + e.HorizontalChange, 0, canvasWidth);
        SetCurrentValue(CurrentTimeProperty, XToTime(newX, canvasWidth));
    }

    private void OnScrubberDragCompleted(object sender, DragCompletedEventArgs e)
    {
        // Handler intentionally empty - kept for event subscription
    }

    private void OnViewportThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_viewportTrack == null || Duration <= 0)
        {
            return;
        }

        double trackWidth = _viewportTrack.ActualWidth;
        if (trackWidth <= 0)
        {
            return;
        }

        double currentThumbLeft = (ViewportStart / Duration) * trackWidth;
        double newThumbLeft = currentThumbLeft + e.HorizontalChange;

        double maxStart = Duration - ViewportDuration;
        double newViewportStart = (newThumbLeft / trackWidth) * Duration;
        newViewportStart = Math.Clamp(newViewportStart, 0, Math.Max(0, maxStart));

        SetCurrentValue(ViewportStartProperty, newViewportStart);
    }

    private void UpdateViewportThumb()
    {
        if (_viewportThumb == null || _viewportTrack == null || Duration <= 0)
        {
            return;
        }

        double trackWidth = _viewportTrack.ActualWidth;
        if (trackWidth <= 0)
        {
            return;
        }

        double thumbLeft = (ViewportStart / Duration) * trackWidth;
        double thumbWidth = Math.Max((ViewportDuration / Duration) * trackWidth, 10);

        _viewportThumb.SetCurrentValue(WidthProperty, thumbWidth);
        _viewportThumb.SetCurrentValue(MarginProperty, new Thickness(thumbLeft, 0, 0, 0));
    }

    private void OnTrackScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_isSyncingScroll || _keyframeScrollViewer == null)
        {
            return;
        }

        _isSyncingScroll = true;
        _keyframeScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
        _isSyncingScroll = false;
    }

    private void OnKeyframeScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_isSyncingScroll || _trackScrollViewer == null)
        {
            return;
        }

        _isSyncingScroll = true;
        _trackScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
        _isSyncingScroll = false;
    }
}
