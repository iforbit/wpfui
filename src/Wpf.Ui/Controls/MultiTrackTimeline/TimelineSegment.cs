// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Wpf.Ui.Controls;

/// <summary>
/// 타임라인 세그먼트 기본 구현 (키프레임 간 구간)
/// KeyframeTimeline과 동일한 패턴: 독립적인 StartTime/EndTime
/// </summary>
public class TimelineSegment : ITimelineSegment
{
    private double _startTime;
    private double _endTime;
    private object? _startValue;
    private object? _endValue;
    private InterpolationType _interpolation = InterpolationType.Linear;
    private bool _isSelected;
    private double _canvasX;
    private double _canvasWidth;
    private Brush? _segmentBrush;
    private Brush? _segmentSelectedBrush;

    /// <summary>
    /// Gets 중간값 리스트 (1→9→5 에서 9가 Waypoint)
    /// </summary>
    public ObservableCollection<SegmentWaypoint> Waypoints { get; }

    private void InitializeWaypoints()
    {
        Waypoints.CollectionChanged += OnWaypointsCollectionChanged;
    }

    private void OnWaypointsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // 새로 추가된 Waypoint에 ParentSegment 설정
        if (e.NewItems != null)
        {
            foreach (SegmentWaypoint waypoint in e.NewItems)
            {
                waypoint.ParentSegment = this;
            }
        }

        // 제거된 Waypoint의 ParentSegment 해제
        if (e.OldItems != null)
        {
            foreach (SegmentWaypoint waypoint in e.OldItems)
            {
                waypoint.ParentSegment = null;
            }
        }
    }

    public double StartTime
    {
        get => _startTime;
        set
        {
            if (SetProperty(ref _startTime, value))
            {
                OnPropertyChanged(nameof(Duration));
                OnPropertyChanged(nameof(TooltipText));
            }
        }
    }

    public double EndTime
    {
        get => _endTime;
        set
        {
            if (SetProperty(ref _endTime, value))
            {
                OnPropertyChanged(nameof(Duration));
                OnPropertyChanged(nameof(TooltipText));
            }
        }
    }

    public object? StartValue
    {
        get => _startValue;
        set => SetProperty(ref _startValue, value);
    }

    public object? EndValue
    {
        get => _endValue;
        set => SetProperty(ref _endValue, value);
    }

    public double Duration => Math.Max(0, EndTime - StartTime);

    public InterpolationType Interpolation
    {
        get => _interpolation;
        set
        {
            if (SetProperty(ref _interpolation, value))
            {
                OnPropertyChanged(nameof(TooltipText));
            }
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public double CanvasX
    {
        get => _canvasX;
        set => SetProperty(ref _canvasX, value);
    }

    public double CanvasWidth
    {
        get => _canvasWidth;
        set => SetProperty(ref _canvasWidth, value);
    }

    public Brush? SegmentBrush
    {
        get => _segmentBrush;
        set => SetProperty(ref _segmentBrush, value);
    }

    public Brush? SegmentSelectedBrush
    {
        get => _segmentSelectedBrush;
        set => SetProperty(ref _segmentSelectedBrush, value);
    }

    public string? TooltipText => $"{StartTime:F2}s → {EndTime:F2}s ({Interpolation})";

    /// <summary>
    /// Initializes a new instance of the <see cref="TimelineSegment"/> class.
    /// 기본 생성자
    /// </summary>
    public TimelineSegment()
    {
        Waypoints = new ObservableCollection<SegmentWaypoint>();
        InitializeWaypoints();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimelineSegment"/> class.
    /// 시간 범위 지정 생성자
    /// </summary>
    public TimelineSegment(double startTime, double endTime, InterpolationType interpolation = InterpolationType.Linear)
    {
        Waypoints = new ObservableCollection<SegmentWaypoint>();
        InitializeWaypoints();
        _startTime = startTime;
        _endTime = endTime;
        _interpolation = interpolation;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimelineSegment"/> class.
    /// 키프레임 기반 생성자 (기존 호환성)
    /// </summary>
    public TimelineSegment(ITimelineKeyframe start, ITimelineKeyframe end)
    {
        Waypoints = new ObservableCollection<SegmentWaypoint>();
        InitializeWaypoints();
        _startTime = start.Time;
        _endTime = end.Time;
        _startValue = start.Value;
        _endValue = end.Value;
        _interpolation = start.Interpolation;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimelineSegment"/> class.
    /// 전체 값 지정 생성자
    /// </summary>
    public TimelineSegment(double startTime, double endTime, object? startValue, object? endValue, InterpolationType interpolation = InterpolationType.Linear)
    {
        Waypoints = new ObservableCollection<SegmentWaypoint>();
        InitializeWaypoints();
        _startTime = startTime;
        _endTime = endTime;
        _startValue = startValue;
        _endValue = endValue;
        _interpolation = interpolation;
    }

    /// <summary>
    /// 특정 시간에서의 보간된 값 계산
    /// </summary>
    public object? GetInterpolatedValue(double time, TrackValueType valueType)
    {
        if (StartValue == null || EndValue == null)
        {
            return time < EndTime ? StartValue : EndValue;
        }

        // 범위 체크
        if (time <= StartTime)
        {
            return StartValue;
        }

        if (time >= EndTime)
        {
            return EndValue;
        }

        // Hold 보간은 즉시 전환
        if (Interpolation == InterpolationType.Hold)
        {
            return StartValue;
        }

        // 보간 비율 계산
        double t = (time - StartTime) / Duration;
        t = ApplyEasing(t, Interpolation);

        return InterpolateValue(StartValue, EndValue, t, valueType);
    }

    private static double ApplyEasing(double t, InterpolationType interpolation)
    {
        return interpolation switch
        {
            InterpolationType.Linear => t,
            InterpolationType.EaseIn => t * t,
            InterpolationType.EaseOut => 1 - ((1 - t) * (1 - t)),
            InterpolationType.EaseInOut => t < 0.5 ? 2 * t * t : 1 - (Math.Pow((-2 * t) + 2, 2) / 2),
            InterpolationType.Hold => 0,
            _ => t
        };
    }

    private static object? InterpolateValue(object? v1, object? v2, double t, TrackValueType valueType)
    {
        if (v1 == null || v2 == null)
        {
            return v1;
        }

        switch (valueType)
        {
            case TrackValueType.Numeric:
                if (v1 is double d1 && v2 is double d2)
                {
                    return d1 + ((d2 - d1) * t);
                }

                // int, float 등 다른 숫자 타입 지원
                if (double.TryParse(v1.ToString(), out var n1) && double.TryParse(v2.ToString(), out var n2))
                {
                    return n1 + ((n2 - n1) * t);
                }

                break;

            case TrackValueType.Color:
                // TODO: Color 보간 구현
                return t < 0.5 ? v1 : v2;

            case TrackValueType.Enum:
            case TrackValueType.Boolean:
                // Step 보간 (값 변경 없음)
                return t < 1.0 ? v1 : v2;
        }

        return v1;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
