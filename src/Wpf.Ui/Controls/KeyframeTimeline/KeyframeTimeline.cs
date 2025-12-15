// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Wpf.Ui.Controls;

/// <summary>
/// 키프레임 타임라인 컨트롤 v1.0
/// - X축: 시간 (0 ~ Duration)
/// - Y축: 1 depth (단일 라인)
/// - KeyframePoint: 같은 시간의 여러 Snapshot을 담는 Container
/// - Scrubber: 현재 시간 표시/드래그 조작
///
/// 참조: https://learn.microsoft.com/ko-kr/dotnet/desktop/wpf/graphics-multimedia/key-frame-animations-overview
/// </summary>
[TemplatePart(Name = PART_TimeRuler, Type = typeof(Canvas))]
[TemplatePart(Name = PART_KeyframeTrack, Type = typeof(Canvas))]
[TemplatePart(Name = PART_Scrubber, Type = typeof(Thumb))]
[TemplatePart(Name = PART_KeyframeItemsControl, Type = typeof(ItemsControl))]
[TemplatePart(Name = PART_SegmentItemsControl, Type = typeof(ItemsControl))]
public class KeyframeTimeline : Control
{
    private const string PART_TimeRuler = "PART_TimeRuler";
    private const string PART_KeyframeTrack = "PART_KeyframeTrack";
    private const string PART_Scrubber = "PART_Scrubber";
    private const string PART_KeyframeItemsControl = "PART_KeyframeItemsControl";
    private const string PART_SegmentItemsControl = "PART_SegmentItemsControl";

    private Canvas? _timeRuler;
    private Canvas? _keyframeTrack;
    private Thumb? _scrubber;
    private ItemsControl? _keyframeItemsControl;
    private ItemsControl? _segmentItemsControl;

    private bool _isDraggingScrubber;
    private bool _isPanning;
    private bool _isDraggingKeyframe;
    private KeyframePoint? _draggingKeyframe;
    private bool _isResizingSegment;
    private KeyframeSegment? _resizingSegment;
    private bool _isResizingLeft; // true: 왼쪽 핸들, false: 오른쪽 핸들
    private bool _isRulerPanning; // 타임 룰러 드래그 패닝
    private Point _rulerPanStartPoint;
    private double _rulerPanStartViewportStart;

    static KeyframeTimeline()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(KeyframeTimeline),
            new FrameworkPropertyMetadata(typeof(KeyframeTimeline)));
    }

    public KeyframeTimeline()
    {
        // 기본 컬렉션 생성 (바인딩 없이 사용할 때를 위해)
        // PropertyChanged 콜백에서 이벤트 핸들러를 자동으로 연결함
        KeyframePoints = new ObservableCollection<KeyframePoint>();
        KeyframeSegments = new ObservableCollection<KeyframeSegment>();
    }

    /// <summary>
    /// 타임라인 시작 시간 (초) - 기본 0
    /// </summary>
    public static readonly DependencyProperty StartTimeProperty =
        DependencyProperty.Register(
            nameof(StartTime),
            typeof(double),
            typeof(KeyframeTimeline),
            new FrameworkPropertyMetadata(0.0, OnTimeRangeChanged));

    /// <summary>
    /// 타임라인 총 Duration (초) - EndTime = StartTime + Duration
    /// </summary>
    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(
            nameof(Duration),
            typeof(double),
            typeof(KeyframeTimeline),
            new FrameworkPropertyMetadata(2.0, OnTimeRangeChanged));

    /// <summary>
    /// 현재 시간 (초) - Scrubber 위치
    /// </summary>
    public static readonly DependencyProperty CurrentTimeProperty =
        DependencyProperty.Register(
            nameof(CurrentTime),
            typeof(double),
            typeof(KeyframeTimeline),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnCurrentTimeChanged,
                CoerceCurrentTime));

    /// <summary>
    /// 선택된 키프레임 포인트
    /// </summary>
    public static readonly DependencyProperty SelectedPointProperty =
        DependencyProperty.Register(
            nameof(SelectedPoint),
            typeof(KeyframePoint),
            typeof(KeyframeTimeline),
            new FrameworkPropertyMetadata(null, OnSelectedPointChanged));

    /// <summary>
    /// 키프레임 포인트 컬렉션
    /// </summary>
    public static readonly DependencyProperty KeyframePointsProperty =
        DependencyProperty.Register(
            nameof(KeyframePoints),
            typeof(ObservableCollection<KeyframePoint>),
            typeof(KeyframeTimeline),
            new PropertyMetadata(null, OnKeyframePointsPropertyChanged));

    /// <summary>
    /// 키프레임 세그먼트 컬렉션 (범위/구간)
    /// </summary>
    public static readonly DependencyProperty KeyframeSegmentsProperty =
        DependencyProperty.Register(
            nameof(KeyframeSegments),
            typeof(ObservableCollection<KeyframeSegment>),
            typeof(KeyframeTimeline),
            new PropertyMetadata(null, OnKeyframeSegmentsPropertyChanged));

    /// <summary>
    /// 선택된 세그먼트
    /// </summary>
    public static readonly DependencyProperty SelectedSegmentProperty =
        DependencyProperty.Register(
            nameof(SelectedSegment),
            typeof(KeyframeSegment),
            typeof(KeyframeTimeline),
            new FrameworkPropertyMetadata(null, OnSelectedSegmentChanged));

    /// <summary>
    /// 세그먼트 기본 색상
    /// </summary>
    public static readonly DependencyProperty SegmentBrushProperty =
        DependencyProperty.Register(
            nameof(SegmentBrush),
            typeof(Brush),
            typeof(KeyframeTimeline),
            new PropertyMetadata(null)); // null이면 AccentFillColor 사용

    /// <summary>
    /// 세그먼트 높이
    /// </summary>
    public static readonly DependencyProperty SegmentHeightProperty =
        DependencyProperty.Register(
            nameof(SegmentHeight),
            typeof(double),
            typeof(KeyframeTimeline),
            new PropertyMetadata(8.0));

    /// <summary>
    /// Scrubber 색상
    /// </summary>
    public static readonly DependencyProperty ScrubberBrushProperty =
        DependencyProperty.Register(
            nameof(ScrubberBrush),
            typeof(Brush),
            typeof(KeyframeTimeline),
            new PropertyMetadata(Brushes.Red));

    /// <summary>
    /// 키프레임 포인트 색상
    /// </summary>
    public static readonly DependencyProperty KeyframeBrushProperty =
        DependencyProperty.Register(
            nameof(KeyframeBrush),
            typeof(Brush),
            typeof(KeyframeTimeline),
            new PropertyMetadata(null)); // null이면 AccentFillColor 사용

    /// <summary>
    /// 트랙 배경 색상
    /// </summary>
    public static readonly DependencyProperty TrackBackgroundProperty =
        DependencyProperty.Register(
            nameof(TrackBackground),
            typeof(Brush),
            typeof(KeyframeTimeline),
            new PropertyMetadata(null));

    /// <summary>
    /// 키프레임 클릭 Command
    /// </summary>
    public static readonly DependencyProperty KeyframeClickCommandProperty =
        DependencyProperty.Register(
            nameof(KeyframeClickCommand),
            typeof(ICommand),
            typeof(KeyframeTimeline),
            new PropertyMetadata(null));

    /// <summary>
    /// 뷰포트 시작 시간 (줌/팬용)
    /// </summary>
    public static readonly DependencyProperty ViewportStartProperty =
        DependencyProperty.Register(
            nameof(ViewportStart),
            typeof(double),
            typeof(KeyframeTimeline),
            new FrameworkPropertyMetadata(0.0, OnViewportChanged, CoerceViewportStart));

    /// <summary>
    /// 뷰포트 표시 Duration (줌 레벨)
    /// </summary>
    public static readonly DependencyProperty ViewportDurationProperty =
        DependencyProperty.Register(
            nameof(ViewportDuration),
            typeof(double),
            typeof(KeyframeTimeline),
            new FrameworkPropertyMetadata(2.0, OnViewportChanged, CoerceViewportDuration));

    /// <summary>
    /// 최소 뷰포트 Duration (최대 줌인)
    /// </summary>
    public static readonly DependencyProperty MinViewportDurationProperty =
        DependencyProperty.Register(
            nameof(MinViewportDuration),
            typeof(double),
            typeof(KeyframeTimeline),
            new PropertyMetadata(0.1));

    /// <summary>
    /// 최대 뷰포트 Duration (최대 줌아웃)
    /// </summary>
    public static readonly DependencyProperty MaxViewportDurationProperty =
        DependencyProperty.Register(
            nameof(MaxViewportDuration),
            typeof(double),
            typeof(KeyframeTimeline),
            new PropertyMetadata(60.0));

    /// <summary>
    /// 줌 활성화 여부
    /// </summary>
    public static readonly DependencyProperty IsZoomEnabledProperty =
        DependencyProperty.Register(
            nameof(IsZoomEnabled),
            typeof(bool),
            typeof(KeyframeTimeline),
            new PropertyMetadata(true));

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

    /// <summary>
    /// Gets 타임라인 끝 시간 (StartTime + Duration)
    /// </summary>
    public double EndTime => StartTime + Duration;

    public double CurrentTime
    {
        get => (double)GetValue(CurrentTimeProperty);
        set => SetValue(CurrentTimeProperty, value);
    }

    public KeyframePoint? SelectedPoint
    {
        get => (KeyframePoint?)GetValue(SelectedPointProperty);
        set => SetValue(SelectedPointProperty, value);
    }

    public ObservableCollection<KeyframePoint>? KeyframePoints
    {
        get => (ObservableCollection<KeyframePoint>?)GetValue(KeyframePointsProperty);
        set => SetValue(KeyframePointsProperty, value);
    }

    public ObservableCollection<KeyframeSegment>? KeyframeSegments
    {
        get => (ObservableCollection<KeyframeSegment>?)GetValue(KeyframeSegmentsProperty);
        set => SetValue(KeyframeSegmentsProperty, value);
    }

    public KeyframeSegment? SelectedSegment
    {
        get => (KeyframeSegment?)GetValue(SelectedSegmentProperty);
        set => SetValue(SelectedSegmentProperty, value);
    }

    public Brush? SegmentBrush
    {
        get => (Brush?)GetValue(SegmentBrushProperty);
        set => SetValue(SegmentBrushProperty, value);
    }

    public double SegmentHeight
    {
        get => (double)GetValue(SegmentHeightProperty);
        set => SetValue(SegmentHeightProperty, value);
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

    public Brush? TrackBackground
    {
        get => (Brush?)GetValue(TrackBackgroundProperty);
        set => SetValue(TrackBackgroundProperty, value);
    }

    public ICommand? KeyframeClickCommand
    {
        get => (ICommand?)GetValue(KeyframeClickCommandProperty);
        set => SetValue(KeyframeClickCommandProperty, value);
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

    public double MinViewportDuration
    {
        get => (double)GetValue(MinViewportDurationProperty);
        set => SetValue(MinViewportDurationProperty, value);
    }

    public double MaxViewportDuration
    {
        get => (double)GetValue(MaxViewportDurationProperty);
        set => SetValue(MaxViewportDurationProperty, value);
    }

    public bool IsZoomEnabled
    {
        get => (bool)GetValue(IsZoomEnabledProperty);
        set => SetValue(IsZoomEnabledProperty, value);
    }

    /// <summary>
    /// 현재 시간 변경 이벤트
    /// </summary>
    public static readonly RoutedEvent CurrentTimeChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(CurrentTimeChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>),
            typeof(KeyframeTimeline));

    public event RoutedPropertyChangedEventHandler<double> CurrentTimeChanged
    {
        add => AddHandler(CurrentTimeChangedEvent, value);
        remove => RemoveHandler(CurrentTimeChangedEvent, value);
    }

    /// <summary>
    /// 키프레임 선택 이벤트
    /// </summary>
    public event EventHandler<KeyframeTimelineEventArgs>? KeyframeSelected;

    /// <summary>
    /// 스크러버 드래그 시작 이벤트
    /// </summary>
    public event EventHandler<KeyframeTimelineEventArgs>? ScrubberDragStarted;

    /// <summary>
    /// 스크러버 드래그 완료 이벤트
    /// </summary>
    public event EventHandler<KeyframeTimelineEventArgs>? ScrubberDragCompleted;

    /// <summary>
    /// 키프레임 포인트 드래그 이벤트
    /// </summary>
    public event EventHandler<KeyframeTimelineEventArgs>? KeyframeDragged;

    /// <summary>
    /// 키프레임 포인트 드래그 완료 이벤트
    /// </summary>
    public event EventHandler<KeyframeTimelineEventArgs>? KeyframeDragCompleted;

    /// <summary>
    /// 세그먼트 선택 이벤트
    /// </summary>
    public event EventHandler<KeyframeSegmentEventArgs>? SegmentSelected;

    /// <summary>
    /// 세그먼트 리사이즈 중 이벤트
    /// </summary>
    public event EventHandler<KeyframeSegmentEventArgs>? SegmentResizing;

    /// <summary>
    /// 세그먼트 리사이즈 완료 이벤트
    /// </summary>
    public event EventHandler<KeyframeSegmentEventArgs>? SegmentResizeCompleted;

    private static void OnTimeRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KeyframeTimeline timeline)
        {
            // ViewportDuration이 Duration보다 크면 조정
            if (timeline.ViewportDuration > timeline.Duration)
            {
                timeline.SetCurrentValue(ViewportDurationProperty, timeline.Duration);
            }

            // ViewportStart + ViewportDuration이 Duration을 초과하면 조정
            timeline.CoerceValue(ViewportStartProperty);
            timeline.CoerceValue(CurrentTimeProperty);
            timeline.UpdateLayout();
            timeline.DrawTimeRuler();
            timeline.UpdateKeyframePositions();
            timeline.UpdateSegmentPositions();
            timeline.UpdateScrubberPosition();
        }
    }

    private static void OnCurrentTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KeyframeTimeline timeline)
        {
            var oldValue = (double)e.OldValue;
            var newValue = (double)e.NewValue;

            timeline.UpdateScrubberPosition();
            timeline.RaiseEvent(new RoutedPropertyChangedEventArgs<double>(oldValue, newValue, CurrentTimeChangedEvent));
        }
    }

    private static object CoerceCurrentTime(DependencyObject d, object value)
    {
        if (d is KeyframeTimeline timeline && value is double time)
        {
            return Math.Clamp(time, timeline.StartTime, timeline.EndTime);
        }

        return value;
    }

    private static void OnSelectedPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KeyframeTimeline timeline)
        {
            // 이전 선택 해제
            if (e.OldValue is KeyframePoint oldPoint)
            {
                oldPoint.IsSelected = false;
            }

            // 새 선택
            if (e.NewValue is KeyframePoint newPoint)
            {
                newPoint.IsSelected = true;
                timeline.KeyframeSelected?.Invoke(timeline, new KeyframeTimelineEventArgs(newPoint.Time, newPoint));
            }
        }
    }

    private static void OnSelectedSegmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KeyframeTimeline timeline)
        {
            // 이전 선택 해제
            if (e.OldValue is KeyframeSegment oldSegment)
            {
                oldSegment.IsSelected = false;
            }

            // 새 선택
            if (e.NewValue is KeyframeSegment newSegment)
            {
                newSegment.IsSelected = true;
                timeline.SegmentSelected?.Invoke(timeline, new KeyframeSegmentEventArgs(newSegment));
            }
        }
    }

    private static void OnViewportChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KeyframeTimeline timeline)
        {
            timeline.DrawTimeRuler();
            timeline.UpdateKeyframePositions();
            timeline.UpdateSegmentPositions();
            timeline.UpdateScrubberPosition();
        }
    }

    private static object CoerceViewportStart(DependencyObject d, object value)
    {
        if (d is KeyframeTimeline timeline && value is double start)
        {
            // ViewportStart는 StartTime~EndTime 범위 내에서 이동
            double maxStart = Math.Max(timeline.StartTime, timeline.EndTime - timeline.ViewportDuration);
            return Math.Clamp(start, timeline.StartTime, maxStart);
        }

        return value;
    }

    private static object CoerceViewportDuration(DependencyObject d, object value)
    {
        if (d is KeyframeTimeline timeline && value is double duration)
        {
            return Math.Clamp(duration, timeline.MinViewportDuration, timeline.MaxViewportDuration);
        }

        return value;
    }

    private static void OnKeyframePointsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KeyframeTimeline timeline)
        {
            // 이전 컬렉션의 이벤트 핸들러 해제
            if (e.OldValue is ObservableCollection<KeyframePoint> oldCollection)
            {
                oldCollection.CollectionChanged -= timeline.OnKeyframePointsChanged;
            }

            // 새 컬렉션의 이벤트 핸들러 등록
            if (e.NewValue is ObservableCollection<KeyframePoint> newCollection)
            {
                newCollection.CollectionChanged += timeline.OnKeyframePointsChanged;
            }

            // ItemsControl 업데이트
            if (timeline._keyframeItemsControl != null)
            {
                timeline._keyframeItemsControl.SetCurrentValue(ItemsControl.ItemsSourceProperty, e.NewValue);
            }

            timeline.UpdateKeyframePositions();
        }
    }

    private static void OnKeyframeSegmentsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KeyframeTimeline timeline)
        {
            // 이전 컬렉션의 이벤트 핸들러 해제
            if (e.OldValue is ObservableCollection<KeyframeSegment> oldCollection)
            {
                oldCollection.CollectionChanged -= timeline.OnKeyframeSegmentsChanged;
            }

            // 새 컬렉션의 이벤트 핸들러 등록
            if (e.NewValue is ObservableCollection<KeyframeSegment> newCollection)
            {
                newCollection.CollectionChanged += timeline.OnKeyframeSegmentsChanged;
            }

            // ItemsControl 업데이트
            if (timeline._segmentItemsControl != null)
            {
                timeline._segmentItemsControl.SetCurrentValue(ItemsControl.ItemsSourceProperty, e.NewValue);
            }

            timeline.UpdateSegmentPositions();
        }
    }

    private void OnKeyframePointsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateKeyframePositions();
    }

    private void OnKeyframeSegmentsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateSegmentPositions();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 기존 이벤트 해제
        if (_scrubber != null)
        {
            _scrubber.DragStarted -= OnScrubberDragStarted;
            _scrubber.DragDelta -= OnScrubberDragDelta;
            _scrubber.DragCompleted -= OnScrubberDragCompleted;
        }

        if (_keyframeTrack != null)
        {
            _keyframeTrack.MouseLeftButtonDown -= OnTrackMouseLeftButtonDown;
        }

        if (_timeRuler != null)
        {
            _timeRuler.MouseLeftButtonDown -= OnRulerMouseLeftButtonDown;
            _timeRuler.MouseLeftButtonUp -= OnRulerMouseLeftButtonUp;
            _timeRuler.MouseMove -= OnRulerMouseMove;
            _timeRuler.MouseWheel -= OnRulerMouseWheel;
        }

        // 템플릿 파트 가져오기
        _timeRuler = GetTemplateChild(PART_TimeRuler) as Canvas;
        _keyframeTrack = GetTemplateChild(PART_KeyframeTrack) as Canvas;
        _scrubber = GetTemplateChild(PART_Scrubber) as Thumb;
        _keyframeItemsControl = GetTemplateChild(PART_KeyframeItemsControl) as ItemsControl;
        _segmentItemsControl = GetTemplateChild(PART_SegmentItemsControl) as ItemsControl;

        // 이벤트 연결
        if (_scrubber != null)
        {
            _scrubber.DragStarted += OnScrubberDragStarted;
            _scrubber.DragDelta += OnScrubberDragDelta;
            _scrubber.DragCompleted += OnScrubberDragCompleted;
        }

        if (_keyframeTrack != null)
        {
            _keyframeTrack.MouseLeftButtonDown += OnTrackMouseLeftButtonDown;
            _keyframeTrack.MouseWheel += OnTrackMouseWheel;
            _keyframeTrack.MouseRightButtonDown += OnTrackMouseRightButtonDown;
            _keyframeTrack.MouseRightButtonUp += OnTrackMouseRightButtonUp;
            _keyframeTrack.MouseMove += OnTrackMouseMove;
        }

        // 타임 룰러 드래그 패닝 + 휠 줌 이벤트
        if (_timeRuler != null)
        {
            _timeRuler.MouseLeftButtonDown += OnRulerMouseLeftButtonDown;
            _timeRuler.MouseLeftButtonUp += OnRulerMouseLeftButtonUp;
            _timeRuler.MouseMove += OnRulerMouseMove;
            _timeRuler.MouseWheel += OnRulerMouseWheel; // 타임 룰러에서도 휠 줌
            _timeRuler.Cursor = Cursors.SizeWE; // 좌우 화살표 커서
        }

        // ItemsControl 바인딩 및 Keyframe Thumb 이벤트
        if (_keyframeItemsControl != null)
        {
            // 현재 KeyframePoints 값으로 ItemsSource 설정
            // (바인딩이 이미 해결되었으면 바인딩된 컬렉션, 아니면 기본 컬렉션)
            if (KeyframePoints != null)
            {
                _keyframeItemsControl.SetCurrentValue(ItemsControl.ItemsSourceProperty, KeyframePoints);
            }

            // Thumb 이벤트 핸들러 등록 (버블링 이벤트 사용)
            _keyframeItemsControl.AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler(OnKeyframeDragStarted));
            _keyframeItemsControl.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnKeyframeDragDelta));
            _keyframeItemsControl.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnKeyframeDragCompleted));
        }

        // Segment ItemsControl 바인딩 및 리사이즈 핸들 이벤트
        if (_segmentItemsControl != null)
        {
            // 현재 KeyframeSegments 값으로 ItemsSource 설정
            if (KeyframeSegments != null)
            {
                _segmentItemsControl.SetCurrentValue(ItemsControl.ItemsSourceProperty, KeyframeSegments);
            }

            // Segment 리사이즈 핸들 Thumb 이벤트 핸들러 등록 (버블링 이벤트 사용)
            _segmentItemsControl.AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler(OnSegmentResizeDragStarted));
            _segmentItemsControl.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnSegmentResizeDragDelta));
            _segmentItemsControl.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnSegmentResizeDragCompleted));

            // Segment 클릭 (선택)
            _segmentItemsControl.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnSegmentMouseLeftButtonDown));
        }

        // 초기 렌더링
        Loaded += (s, e) =>
        {
            DrawTimeRuler();
            UpdateKeyframePositions();
            UpdateSegmentPositions();
            UpdateScrubberPosition();
        };

        SizeChanged += (s, e) =>
        {
            DrawTimeRuler();
            UpdateKeyframePositions();
            UpdateSegmentPositions();
            UpdateScrubberPosition();
        };
    }

    private void OnScrubberDragStarted(object sender, DragStartedEventArgs e)
    {
        _isDraggingScrubber = true;
        ScrubberDragStarted?.Invoke(this, new KeyframeTimelineEventArgs(CurrentTime));
    }

    private void OnScrubberDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_keyframeTrack == null || Duration <= 0)
        {
            return;
        }

        double trackWidth = _keyframeTrack.ActualWidth;
        if (trackWidth <= 0)
        {
            return;
        }

        // 현재 X 위치 계산
        double currentX = TimeToX(CurrentTime, trackWidth);
        double newX = Math.Clamp(currentX + e.HorizontalChange, 0, trackWidth);

        // X → 시간 변환
        SetCurrentValue(CurrentTimeProperty, XToTime(newX, trackWidth));
    }

    private void OnScrubberDragCompleted(object sender, DragCompletedEventArgs e)
    {
        _isDraggingScrubber = false;
        ScrubberDragCompleted?.Invoke(this, new KeyframeTimelineEventArgs(CurrentTime));
    }

    private void OnKeyframeDragStarted(object sender, DragStartedEventArgs e)
    {
        if (e.OriginalSource is Thumb thumb && thumb.Tag is KeyframePoint point)
        {
            _isDraggingKeyframe = true;
            _draggingKeyframe = point;

            // 드래그 시작 시 선택
            SetCurrentValue(SelectedPointProperty, point);
        }
    }

    private void OnKeyframeDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (!_isDraggingKeyframe || _draggingKeyframe == null || _keyframeTrack == null)
        {
            return;
        }

        double trackWidth = _keyframeTrack.ActualWidth;
        if (trackWidth <= 0)
        {
            return;
        }

        // 현재 X 위치 계산
        double currentX = TimeToX(_draggingKeyframe.Time, trackWidth);
        double newX = currentX + e.HorizontalChange;

        // X → 시간 변환 (StartTime~EndTime 범위 내로 제한)
        double newTime = Math.Clamp(XToTime(newX, trackWidth), StartTime, EndTime);
        _draggingKeyframe.Time = newTime;

        // 위치 업데이트
        UpdateKeyframePositions();

        // 이벤트 발생
        KeyframeDragged?.Invoke(this, new KeyframeTimelineEventArgs(newTime, _draggingKeyframe));
    }

    private void OnKeyframeDragCompleted(object sender, DragCompletedEventArgs e)
    {
        if (_isDraggingKeyframe && _draggingKeyframe != null)
        {
            KeyframeDragCompleted?.Invoke(this, new KeyframeTimelineEventArgs(_draggingKeyframe.Time, _draggingKeyframe));
        }

        _isDraggingKeyframe = false;
        _draggingKeyframe = null;
    }

    private void OnSegmentMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Grid(Segment container)를 찾아서 선택
        if (e.OriginalSource is FrameworkElement element)
        {
            // Grid나 Border에서 KeyframeSegment 찾기
            KeyframeSegment? segment = FindSegmentFromElement(element);
            if (segment != null)
            {
                SetCurrentValue(SelectedSegmentProperty, segment);
            }
        }
    }

    private KeyframeSegment? FindSegmentFromElement(FrameworkElement element)
    {
        // Tag에서 직접 찾기
        if (element.Tag is KeyframeSegment segment)
        {
            return segment;
        }

        // DataContext에서 찾기
        if (element.DataContext is KeyframeSegment dcSegment)
        {
            return dcSegment;
        }

        // 부모에서 찾기
        if (element.Parent is FrameworkElement parent)
        {
            return FindSegmentFromElement(parent);
        }

        return null;
    }

    private void OnSegmentResizeDragStarted(object sender, DragStartedEventArgs e)
    {
        if (e.OriginalSource is Thumb thumb && thumb.Tag is string handleType)
        {
            // Thumb의 DataContext 또는 부모 Grid의 Tag에서 Segment 가져오기
            KeyframeSegment? segment = null;

            if (thumb.DataContext is KeyframeSegment dcSegment)
            {
                segment = dcSegment;
            }
            else if (thumb.Parent is FrameworkElement parent)
            {
                segment = FindSegmentFromElement(parent);
            }

            if (segment != null)
            {
                _isResizingSegment = true;
                _resizingSegment = segment;
                _isResizingLeft = handleType == "Left";

                // 리사이즈 시작 시 선택
                SetCurrentValue(SelectedSegmentProperty, segment);
            }
        }
    }

    private void OnSegmentResizeDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (!_isResizingSegment || _resizingSegment == null || _keyframeTrack == null)
        {
            return;
        }

        double trackWidth = _keyframeTrack.ActualWidth;
        if (trackWidth <= 0)
        {
            return;
        }

        // 시간 변화량 계산
        double deltaTime = (e.HorizontalChange / trackWidth) * ViewportDuration;

        if (_isResizingLeft)
        {
            // 왼쪽 핸들: StartTime 조정
            double newStartTime = _resizingSegment.StartTime + deltaTime;
            // EndTime보다 작고 타임라인 범위 내로 제한 (최소 0.01초 간격 유지)
            newStartTime = Math.Clamp(newStartTime, StartTime, _resizingSegment.EndTime - 0.01);
            _resizingSegment.StartTime = newStartTime;
        }
        else
        {
            // 오른쪽 핸들: EndTime 조정
            double newEndTime = _resizingSegment.EndTime + deltaTime;
            // StartTime보다 크고 타임라인 범위 내로 제한 (최소 0.01초 간격 유지)
            newEndTime = Math.Clamp(newEndTime, _resizingSegment.StartTime + 0.01, EndTime);
            _resizingSegment.EndTime = newEndTime;
        }

        // 위치 업데이트
        UpdateSegmentPositions();

        // 이벤트 발생
        SegmentResizing?.Invoke(this, new KeyframeSegmentEventArgs(_resizingSegment));
    }

    private void OnSegmentResizeDragCompleted(object sender, DragCompletedEventArgs e)
    {
        if (_isResizingSegment && _resizingSegment != null)
        {
            SegmentResizeCompleted?.Invoke(this, new KeyframeSegmentEventArgs(_resizingSegment));
        }

        _isResizingSegment = false;
        _resizingSegment = null;
    }

    private void OnTrackMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_keyframeTrack == null || Duration <= 0)
        {
            return;
        }

        // 트랙 클릭 시 해당 위치로 스크러버 이동
        Point pos = e.GetPosition(_keyframeTrack);
        double trackWidth = _keyframeTrack.ActualWidth;

        if (trackWidth > 0)
        {
            SetCurrentValue(CurrentTimeProperty, XToTime(pos.X, trackWidth));
        }
    }

    private Point _panStartPoint;
    private double _panStartViewportStart;

    private void OnTrackMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!IsZoomEnabled || _keyframeTrack == null)
        {
            return;
        }

        // 마우스 위치 기준으로 줌
        Point mousePos = e.GetPosition(_keyframeTrack);
        double trackWidth = _keyframeTrack.ActualWidth;
        double mouseTime = XToTime(mousePos.X, trackWidth);

        // 줌 비율 (20% 씩 변경)
        double zoomFactor = e.Delta > 0 ? 0.8 : 1.25;
        double newViewportDuration = Math.Clamp(
            ViewportDuration * zoomFactor,
            MinViewportDuration,
            MaxViewportDuration);

        // 마우스 위치가 같은 시간을 가리키도록 ViewportStart 조정
        double mouseRatio = mousePos.X / trackWidth;
        double newViewportStart = mouseTime - (newViewportDuration * mouseRatio);
        newViewportStart = Math.Clamp(newViewportStart, StartTime, Math.Max(StartTime, EndTime - newViewportDuration));

        SetCurrentValue(ViewportDurationProperty, newViewportDuration);
        SetCurrentValue(ViewportStartProperty, newViewportStart);

        e.Handled = true;
    }

    private void OnTrackMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_keyframeTrack == null)
        {
            return;
        }

        _isPanning = true;
        _panStartPoint = e.GetPosition(_keyframeTrack);
        _panStartViewportStart = ViewportStart;
        _ = _keyframeTrack.CaptureMouse();
        e.Handled = true;
    }

    private void OnTrackMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isPanning && _keyframeTrack != null)
        {
            _isPanning = false;
            _keyframeTrack.ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    private void OnTrackMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isPanning || _keyframeTrack == null)
        {
            return;
        }

        Point currentPos = e.GetPosition(_keyframeTrack);
        double trackWidth = _keyframeTrack.ActualWidth;

        // 이동 거리를 시간으로 변환
        double deltaX = _panStartPoint.X - currentPos.X;
        double deltaTime = (deltaX / trackWidth) * ViewportDuration;

        double newStart = Math.Clamp(
            _panStartViewportStart + deltaTime,
            StartTime,
            Math.Max(StartTime, EndTime - ViewportDuration));

        SetCurrentValue(ViewportStartProperty, newStart);
    }

    #region Time Ruler Panning (시간 눈금자 드래그 패닝)

    private void OnRulerMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

    private void OnRulerMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isRulerPanning && _timeRuler != null)
        {
            _isRulerPanning = false;
            _timeRuler.ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    private void OnRulerMouseMove(object sender, MouseEventArgs e)
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

        // 이동 거리를 시간으로 변환 (드래그 방향과 반대로 뷰포트 이동)
        double deltaX = _rulerPanStartPoint.X - currentPos.X;
        double deltaTime = (deltaX / rulerWidth) * ViewportDuration;

        double newStart = Math.Clamp(
            _rulerPanStartViewportStart + deltaTime,
            StartTime,
            Math.Max(StartTime, EndTime - ViewportDuration));

        SetCurrentValue(ViewportStartProperty, newStart);
    }

    private void OnRulerMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!IsZoomEnabled || _timeRuler == null)
        {
            return;
        }

        // 마우스 위치 기준으로 줌 (트랙 기준 좌표 사용)
        Point mousePos = e.GetPosition(_timeRuler);
        double rulerWidth = _timeRuler.ActualWidth;
        double mouseTime = XToTime(mousePos.X, rulerWidth);

        // 줌 비율 (20% 씩 변경)
        double zoomFactor = e.Delta > 0 ? 0.8 : 1.25;
        double newViewportDuration = Math.Clamp(
            ViewportDuration * zoomFactor,
            MinViewportDuration,
            MaxViewportDuration);

        // 마우스 위치가 같은 시간을 가리키도록 ViewportStart 조정
        double mouseRatio = mousePos.X / rulerWidth;
        double newViewportStart = mouseTime - (newViewportDuration * mouseRatio);
        newViewportStart = Math.Clamp(newViewportStart, StartTime, Math.Max(StartTime, EndTime - newViewportDuration));

        SetCurrentValue(ViewportDurationProperty, newViewportDuration);
        SetCurrentValue(ViewportStartProperty, newViewportStart);

        e.Handled = true;
    }

    #endregion

    /// <summary>
    /// 시간 눈금자 그리기
    /// </summary>
    private void DrawTimeRuler()
    {
        if (_timeRuler == null)
        {
            return;
        }

        _timeRuler.Children.Clear();

        double width = _timeRuler.ActualWidth;
        double height = _timeRuler.ActualHeight;

        if (width <= 0 || height <= 0 || ViewportDuration <= 0)
        {
            return;
        }

        // Viewport 기반 Major/Minor 간격 계산
        double majorInterval = CalculateMajorInterval(ViewportDuration);
        double minorInterval = majorInterval / 5;

        var tickBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
        var textBrush = new SolidColorBrush(Color.FromRgb(160, 160, 160));

        double viewportEnd = ViewportStart + ViewportDuration;

        // 첫 번째 minor tick 시작점
        double firstMinor = Math.Ceiling(ViewportStart / minorInterval) * minorInterval;

        // Minor ticks
        for (double t = firstMinor; t <= viewportEnd; t += minorInterval)
        {
            double x = TimeToX(t, width);
            if (x < 0 || x > width)
            {
                continue;
            }

            bool isMajor = Math.Abs(t % majorInterval) < 0.0001 || Math.Abs((t % majorInterval) - majorInterval) < 0.0001;

            if (!isMajor)
            {
                var line = new Line
                {
                    X1 = x,
                    Y1 = height - 5,
                    X2 = x,
                    Y2 = height,
                    Stroke = tickBrush,
                    StrokeThickness = 1
                };
                _ = _timeRuler.Children.Add(line);
            }
        }

        // 첫 번째 major tick 시작점
        double firstMajor = Math.Ceiling(ViewportStart / majorInterval) * majorInterval;

        // Major ticks + labels
        for (double t = firstMajor; t <= viewportEnd; t += majorInterval)
        {
            double x = TimeToX(t, width);
            if (x < -20 || x > width + 20)
            {
                continue;
            }

            // Tick line
            var line = new Line
            {
                X1 = x,
                Y1 = height - 10,
                X2 = x,
                Y2 = height,
                Stroke = tickBrush,
                StrokeThickness = 1
            };
            _ = _timeRuler.Children.Add(line);

            // Label
            var label = new TextBlock
            {
                Text = FormatTime(t, ViewportDuration),
                FontSize = 9,
                Foreground = textBrush
            };
            Canvas.SetLeft(label, x - 10);
            Canvas.SetTop(label, 0);
            _ = _timeRuler.Children.Add(label);
        }
    }

    private static double CalculateMajorInterval(double viewportDuration)
    {
        // 줌 레벨에 따른 적응형 간격
        if (viewportDuration <= 0.5)
        {
            return 0.1;
        }

        if (viewportDuration <= 1)
        {
            return 0.2;
        }

        if (viewportDuration <= 2)
        {
            return 0.5;
        }

        if (viewportDuration <= 5)
        {
            return 1.0;
        }

        if (viewportDuration <= 10)
        {
            return 2.0;
        }

        if (viewportDuration <= 30)
        {
            return 5.0;
        }

        if (viewportDuration <= 60)
        {
            return 10.0;
        }

        return 15.0;
    }

    private static string FormatTime(double seconds, double viewportDuration)
    {
        // 줌 레벨에 따른 포맷
        if (viewportDuration <= 1)
        {
            return $"{seconds:F2}s";
        }

        if (viewportDuration <= 5)
        {
            return $"{seconds:F1}s";
        }

        if (seconds >= 60)
        {
            int minutes = (int)(seconds / 60);
            int secs = (int)(seconds % 60);
            return $"{minutes}:{secs:D2}";
        }

        return $"{seconds:F0}s";
    }

    private void UpdateScrubberPosition()
    {
        if (_scrubber == null || _keyframeTrack == null)
        {
            return;
        }

        double trackWidth = _keyframeTrack.ActualWidth;
        if (trackWidth <= 0)
        {
            return;
        }

        // CurrentTime이 뷰포트 범위 밖이면 Scrubber 숨김
        double viewportEnd = ViewportStart + ViewportDuration;
        if (CurrentTime < ViewportStart || CurrentTime > viewportEnd)
        {
            _scrubber.Visibility = Visibility.Collapsed;
            return;
        }

        _scrubber.Visibility = Visibility.Visible;
        double x = TimeToX(CurrentTime, trackWidth);
        Canvas.SetLeft(_scrubber, x - (_scrubber.ActualWidth / 2));
    }

    private void UpdateKeyframePositions()
    {
        if (_keyframeTrack == null || KeyframePoints == null)
        {
            return;
        }

        double trackWidth = _keyframeTrack.ActualWidth;
        if (trackWidth <= 0)
        {
            return;
        }

        foreach (KeyframePoint point in KeyframePoints)
        {
            point.CanvasX = TimeToX(point.Time, trackWidth) - 7; // 포인트 중앙 정렬 (Width=14의 절반)
        }
    }

    private void UpdateSegmentPositions()
    {
        if (_keyframeTrack == null || KeyframeSegments == null)
        {
            return;
        }

        double trackWidth = _keyframeTrack.ActualWidth;
        if (trackWidth <= 0)
        {
            return;
        }

        foreach (KeyframeSegment segment in KeyframeSegments)
        {
            double startX = TimeToX(segment.StartTime, trackWidth);
            double endX = TimeToX(segment.EndTime, trackWidth);

            segment.CanvasX = startX;
            segment.CanvasWidth = Math.Max(0, endX - startX);
        }
    }

    private double TimeToX(double time, double trackWidth)
    {
        if (ViewportDuration <= 0)
        {
            return 0;
        }

        // Viewport 기준으로 변환
        return ((time - ViewportStart) / ViewportDuration) * trackWidth;
    }

    private double XToTime(double x, double trackWidth)
    {
        if (trackWidth <= 0)
        {
            return 0;
        }

        // Viewport 기준으로 변환
        return ViewportStart + ((x / trackWidth) * ViewportDuration);
    }

    /// <summary>
    /// 특정 시간에 키프레임 포인트 추가
    /// </summary>
    public KeyframePoint AddKeyframePoint(double time, object? tag = null)
    {
        var point = new KeyframePoint
        {
            Time = Math.Clamp(time, StartTime, EndTime),
            Tag = tag
        };

        KeyframePoints?.Add(point);
        UpdateKeyframePositions();

        return point;
    }

    /// <summary>
    /// 키프레임 포인트 제거
    /// </summary>
    public bool RemoveKeyframePoint(KeyframePoint point)
    {
        if (SelectedPoint == point)
        {
            SetCurrentValue(SelectedPointProperty, null);
        }

        return KeyframePoints?.Remove(point) ?? false;
    }

    /// <summary>
    /// 모든 키프레임 포인트 제거
    /// </summary>
    public void ClearKeyframePoints()
    {
        SetCurrentValue(SelectedPointProperty, null);
        KeyframePoints?.Clear();
    }

    /// <summary>
    /// 키프레임 포인트 선택
    /// </summary>
    public void SelectKeyframePoint(KeyframePoint? point)
    {
        SetCurrentValue(SelectedPointProperty, point);
    }

    /// <summary>
    /// 키프레임 세그먼트 추가
    /// </summary>
    /// <param name="startTime">시작 시간 (초)</param>
    /// <param name="endTime">종료 시간 (초)</param>
    /// <param name="label">라벨 (선택)</param>
    /// <param name="interpolation">보간 타입</param>
    /// <param name="brush">세그먼트 색상 (null이면 기본색)</param>
    /// <param name="tag">사용자 데이터</param>
    /// <returns>생성된 세그먼트</returns>
    public KeyframeSegment AddKeyframeSegment(
        double startTime,
        double endTime,
        string? label = null,
        InterpolationType interpolation = InterpolationType.Linear,
        Brush? brush = null,
        object? tag = null)
    {
        var segment = new KeyframeSegment
        {
            StartTime = Math.Clamp(startTime, StartTime, EndTime),
            EndTime = Math.Clamp(endTime, StartTime, EndTime),
            Label = label,
            Interpolation = interpolation,
            SegmentBrush = brush,
            Tag = tag
        };

        KeyframeSegments?.Add(segment);
        UpdateSegmentPositions();

        return segment;
    }

    /// <summary>
    /// 두 키프레임 포인트 사이에 세그먼트 생성
    /// </summary>
    public KeyframeSegment AddKeyframeSegmentBetween(
        KeyframePoint startPoint,
        KeyframePoint endPoint,
        string? label = null,
        InterpolationType interpolation = InterpolationType.Linear,
        Brush? brush = null,
        object? tag = null)
    {
        return AddKeyframeSegment(
            startPoint.Time,
            endPoint.Time,
            label,
            interpolation,
            brush,
            tag);
    }

    /// <summary>
    /// 키프레임 세그먼트 제거
    /// </summary>
    public bool RemoveKeyframeSegment(KeyframeSegment segment)
    {
        if (SelectedSegment == segment)
        {
            SetCurrentValue(SelectedSegmentProperty, null);
        }

        return KeyframeSegments?.Remove(segment) ?? false;
    }

    /// <summary>
    /// 모든 키프레임 세그먼트 제거
    /// </summary>
    public void ClearKeyframeSegments()
    {
        SetCurrentValue(SelectedSegmentProperty, null);
        KeyframeSegments?.Clear();
    }

    /// <summary>
    /// 키프레임 세그먼트 선택
    /// </summary>
    public void SelectKeyframeSegment(KeyframeSegment? segment)
    {
        SetCurrentValue(SelectedSegmentProperty, segment);
    }

    /// <summary>
    /// 연속된 키프레임 포인트들 사이에 자동으로 세그먼트 생성
    /// </summary>
    /// <param name="interpolation">보간 타입</param>
    /// <param name="brush">세그먼트 색상</param>
    public void GenerateSegmentsFromPoints(
        InterpolationType interpolation = InterpolationType.Linear,
        Brush? brush = null)
    {
        if (KeyframePoints == null || KeyframePoints.Count < 2)
        {
            return;
        }

        // 기존 세그먼트 제거
        ClearKeyframeSegments();

        // 시간순으로 정렬
        var sortedPoints = KeyframePoints.OrderBy(p => p.Time).ToList();

        // 연속된 포인트 사이에 세그먼트 생성
        for (int i = 0; i < sortedPoints.Count - 1; i++)
        {
            AddKeyframeSegmentBetween(
                sortedPoints[i],
                sortedPoints[i + 1],
                null,
                interpolation,
                brush);
        }
    }
}
