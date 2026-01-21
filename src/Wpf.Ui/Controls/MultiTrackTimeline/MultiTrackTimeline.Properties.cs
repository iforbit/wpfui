// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Wpf.Ui.Controls;

/// <summary>
/// MultiTrackTimeline - Dependency Properties, Property Wrappers, Events
/// </summary>
public partial class MultiTrackTimeline
{
    /// <summary>
    /// 트랙 그룹 컬렉션
    /// </summary>
    public static readonly DependencyProperty TrackGroupsProperty =
        DependencyProperty.Register(
            nameof(TrackGroups),
            typeof(ObservableCollection<ITimelineTrackGroup>),
            typeof(MultiTrackTimeline),
            new PropertyMetadata(null, OnTrackGroupsChanged));

    /// <summary>
    /// 타임라인 시작 시간 (초)
    /// </summary>
    public static readonly DependencyProperty StartTimeProperty =
        DependencyProperty.Register(
            nameof(StartTime),
            typeof(double),
            typeof(MultiTrackTimeline),
            new FrameworkPropertyMetadata(0.0, OnTimeRangeChanged));

    /// <summary>
    /// 타임라인 Duration (초)
    /// </summary>
    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(
            nameof(Duration),
            typeof(double),
            typeof(MultiTrackTimeline),
            new FrameworkPropertyMetadata(5.0, OnTimeRangeChanged));

    /// <summary>
    /// 현재 시간 (Scrubber 위치)
    /// </summary>
    public static readonly DependencyProperty CurrentTimeProperty =
        DependencyProperty.Register(
            nameof(CurrentTime),
            typeof(double),
            typeof(MultiTrackTimeline),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnCurrentTimeChanged,
                CoerceCurrentTime));

    /// <summary>
    /// 선택된 트랙
    /// </summary>
    public static readonly DependencyProperty SelectedTrackProperty =
        DependencyProperty.Register(
            nameof(SelectedTrack),
            typeof(ITimelineTrack),
            typeof(MultiTrackTimeline),
            new FrameworkPropertyMetadata(null, OnSelectedTrackChanged));

    /// <summary>
    /// 선택된 키프레임
    /// </summary>
    public static readonly DependencyProperty SelectedKeyframeProperty =
        DependencyProperty.Register(
            nameof(SelectedKeyframe),
            typeof(ITimelineKeyframe),
            typeof(MultiTrackTimeline),
            new FrameworkPropertyMetadata(null, OnSelectedKeyframeChanged));

    /// <summary>
    /// 선택된 세그먼트
    /// </summary>
    public static readonly DependencyProperty SelectedSegmentProperty =
        DependencyProperty.Register(
            nameof(SelectedSegment),
            typeof(ITimelineSegment),
            typeof(MultiTrackTimeline),
            new FrameworkPropertyMetadata(null, OnSelectedSegmentChanged));

    /// <summary>
    /// 세그먼트 클릭 시간 (보간 값 계산용)
    /// </summary>
    public static readonly DependencyProperty SegmentClickTimeProperty =
        DependencyProperty.Register(
            nameof(SegmentClickTime),
            typeof(double),
            typeof(MultiTrackTimeline),
            new FrameworkPropertyMetadata(0.0));

    /// <summary>
    /// 뷰포트 시작 시간 (줌/팬)
    /// </summary>
    public static readonly DependencyProperty ViewportStartProperty =
        DependencyProperty.Register(
            nameof(ViewportStart),
            typeof(double),
            typeof(MultiTrackTimeline),
            new FrameworkPropertyMetadata(0.0, OnViewportChanged, CoerceViewportStart));

    /// <summary>
    /// 뷰포트 Duration (줌 레벨)
    /// </summary>
    public static readonly DependencyProperty ViewportDurationProperty =
        DependencyProperty.Register(
            nameof(ViewportDuration),
            typeof(double),
            typeof(MultiTrackTimeline),
            new FrameworkPropertyMetadata(5.0, OnViewportChanged, CoerceViewportDuration));

    /// <summary>
    /// 트랙 높이
    /// </summary>
    public static readonly DependencyProperty TrackHeightProperty =
        DependencyProperty.Register(
            nameof(TrackHeight),
            typeof(double),
            typeof(MultiTrackTimeline),
            new PropertyMetadata(24.0));

    /// <summary>
    /// 트랙 목록 패널 너비
    /// </summary>
    public static readonly DependencyProperty TrackListWidthProperty =
        DependencyProperty.Register(
            nameof(TrackListWidth),
            typeof(double),
            typeof(MultiTrackTimeline),
            new PropertyMetadata(200.0));

    /// <summary>
    /// Scrubber 브러시
    /// </summary>
    public static readonly DependencyProperty ScrubberBrushProperty =
        DependencyProperty.Register(
            nameof(ScrubberBrush),
            typeof(Brush),
            typeof(MultiTrackTimeline),
            new PropertyMetadata(Brushes.Red));

    /// <summary>
    /// 키프레임 브러시
    /// </summary>
    public static readonly DependencyProperty KeyframeBrushProperty =
        DependencyProperty.Register(
            nameof(KeyframeBrush),
            typeof(Brush),
            typeof(MultiTrackTimeline),
            new PropertyMetadata(null));

    /// <summary>
    /// 줌 활성화 여부
    /// </summary>
    public static readonly DependencyProperty IsZoomEnabledProperty =
        DependencyProperty.Register(
            nameof(IsZoomEnabled),
            typeof(bool),
            typeof(MultiTrackTimeline),
            new PropertyMetadata(true));

    /// <summary>
    /// 키프레임 선택 Command
    /// </summary>
    public static readonly DependencyProperty KeyframeSelectedCommandProperty =
        DependencyProperty.Register(
            nameof(KeyframeSelectedCommand),
            typeof(ICommand),
            typeof(MultiTrackTimeline),
            new PropertyMetadata(null));

    /// <summary>
    /// 트랙 선택 Command
    /// </summary>
    public static readonly DependencyProperty TrackSelectedCommandProperty =
        DependencyProperty.Register(
            nameof(TrackSelectedCommand),
            typeof(ICommand),
            typeof(MultiTrackTimeline),
            new PropertyMetadata(null));

    /// <summary>
    /// 세그먼트 브러시
    /// </summary>
    public static readonly DependencyProperty SegmentBrushProperty =
        DependencyProperty.Register(
            nameof(SegmentBrush),
            typeof(Brush),
            typeof(MultiTrackTimeline),
            new PropertyMetadata(null));

    /// <summary>
    /// 웨이포인트 브러시
    /// </summary>
    public static readonly DependencyProperty WaypointBrushProperty =
        DependencyProperty.Register(
            nameof(WaypointBrush),
            typeof(Brush),
            typeof(MultiTrackTimeline),
            new PropertyMetadata(null));

    /// <summary>
    /// 선택된 키프레임 브러시
    /// </summary>
    public static readonly DependencyProperty KeyframeSelectedBrushProperty =
        DependencyProperty.Register(
            nameof(KeyframeSelectedBrush),
            typeof(Brush),
            typeof(MultiTrackTimeline),
            new PropertyMetadata(null));

    /// <summary>
    /// 선택된 세그먼트 테두리 브러시
    /// </summary>
    public static readonly DependencyProperty SegmentSelectedBrushProperty =
        DependencyProperty.Register(
            nameof(SegmentSelectedBrush),
            typeof(Brush),
            typeof(MultiTrackTimeline),
            new PropertyMetadata(null));

    /// <summary>
    /// 캔버스 실제 너비 (XAML 바인딩용 - 읽기 전용)
    /// </summary>
    private static readonly DependencyPropertyKey CanvasActualWidthPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(CanvasActualWidth),
            typeof(double),
            typeof(MultiTrackTimeline),
            new FrameworkPropertyMetadata(0.0));

    public static readonly DependencyProperty CanvasActualWidthProperty =
        CanvasActualWidthPropertyKey.DependencyProperty;

    public ObservableCollection<ITimelineTrackGroup>? TrackGroups
    {
        get => (ObservableCollection<ITimelineTrackGroup>?)GetValue(TrackGroupsProperty);
        set => SetValue(TrackGroupsProperty, value);
    }

    public double StartTime
    {
        get => (double)GetValue(StartTimeProperty);
        set => SetValue(StartTimeProperty, value);
    }

    public double Duration
    {
        get => (double)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public double EndTime => StartTime + Duration;

    public double CurrentTime
    {
        get => (double)GetValue(CurrentTimeProperty);
        set => SetValue(CurrentTimeProperty, value);
    }

    public ITimelineTrack? SelectedTrack
    {
        get => (ITimelineTrack?)GetValue(SelectedTrackProperty);
        set => SetValue(SelectedTrackProperty, value);
    }

    public ITimelineKeyframe? SelectedKeyframe
    {
        get => (ITimelineKeyframe?)GetValue(SelectedKeyframeProperty);
        set => SetValue(SelectedKeyframeProperty, value);
    }

    public ITimelineSegment? SelectedSegment
    {
        get => (ITimelineSegment?)GetValue(SelectedSegmentProperty);
        set => SetValue(SelectedSegmentProperty, value);
    }

    public double SegmentClickTime
    {
        get => (double)GetValue(SegmentClickTimeProperty);
        set => SetValue(SegmentClickTimeProperty, value);
    }

    public double ViewportStart
    {
        get => (double)GetValue(ViewportStartProperty);
        set => SetValue(ViewportStartProperty, value);
    }

    public double ViewportDuration
    {
        get => (double)GetValue(ViewportDurationProperty);
        set => SetValue(ViewportDurationProperty, value);
    }

    public double TrackHeight
    {
        get => (double)GetValue(TrackHeightProperty);
        set => SetValue(TrackHeightProperty, value);
    }

    public double TrackListWidth
    {
        get => (double)GetValue(TrackListWidthProperty);
        set => SetValue(TrackListWidthProperty, value);
    }

    public Brush ScrubberBrush
    {
        get => (Brush)GetValue(ScrubberBrushProperty);
        set => SetValue(ScrubberBrushProperty, value);
    }

    public Brush? KeyframeBrush
    {
        get => (Brush?)GetValue(KeyframeBrushProperty);
        set => SetValue(KeyframeBrushProperty, value);
    }

    public bool IsZoomEnabled
    {
        get => (bool)GetValue(IsZoomEnabledProperty);
        set => SetValue(IsZoomEnabledProperty, value);
    }

    public ICommand? KeyframeSelectedCommand
    {
        get => (ICommand?)GetValue(KeyframeSelectedCommandProperty);
        set => SetValue(KeyframeSelectedCommandProperty, value);
    }

    public ICommand? TrackSelectedCommand
    {
        get => (ICommand?)GetValue(TrackSelectedCommandProperty);
        set => SetValue(TrackSelectedCommandProperty, value);
    }

    public Brush? SegmentBrush
    {
        get => (Brush?)GetValue(SegmentBrushProperty);
        set => SetValue(SegmentBrushProperty, value);
    }

    public Brush? WaypointBrush
    {
        get => (Brush?)GetValue(WaypointBrushProperty);
        set => SetValue(WaypointBrushProperty, value);
    }

    public Brush? KeyframeSelectedBrush
    {
        get => (Brush?)GetValue(KeyframeSelectedBrushProperty);
        set => SetValue(KeyframeSelectedBrushProperty, value);
    }

    public Brush? SegmentSelectedBrush
    {
        get => (Brush?)GetValue(SegmentSelectedBrushProperty);
        set => SetValue(SegmentSelectedBrushProperty, value);
    }

    public double CanvasActualWidth => (double)GetValue(CanvasActualWidthProperty);

    /// <summary>
    /// 현재 시간 변경 이벤트
    /// </summary>
    public static readonly RoutedEvent CurrentTimeChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(CurrentTimeChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>),
            typeof(MultiTrackTimeline));

    public event RoutedPropertyChangedEventHandler<double> CurrentTimeChanged
    {
        add => AddHandler(CurrentTimeChangedEvent, value);
        remove => RemoveHandler(CurrentTimeChangedEvent, value);
    }

    /// <summary>
    /// 키프레임 선택 이벤트
    /// </summary>
    public event EventHandler<MultiTrackKeyframeEventArgs>? KeyframeSelected;

    /// <summary>
    /// 키프레임 드래그 이벤트
    /// </summary>
    public event EventHandler<MultiTrackKeyframeEventArgs>? KeyframeDragged;

    /// <summary>
    /// 트랙 선택 이벤트
    /// </summary>
    public event EventHandler<MultiTrackEventArgs>? TrackSelected;

    /// <summary>
    /// 세그먼트 선택 이벤트 (클릭 시간과 보간된 값 포함)
    /// </summary>
    public event EventHandler<MultiTrackSegmentEventArgs>? SegmentSelected;

    /// <summary>
    /// 세그먼트 드래그 이벤트
    /// </summary>
    public event EventHandler<MultiTrackSegmentEventArgs>? SegmentDragged;
}
