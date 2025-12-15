// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Media;

namespace Wpf.Ui.Controls;

/// <summary>
/// Segment 브러시 선택 컨버터 - Segment.SegmentBrush 또는 Timeline.SegmentBrush 또는 기본 AccentBrush
/// </summary>
public class KeyframeSegmentBrushConverter : IMultiValueConverter
{
    public static readonly KeyframeSegmentBrushConverter Instance = new();

    private static readonly Brush DefaultBrush = new SolidColorBrush(Color.FromArgb(180, 0, 120, 212));

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // values[0]: Segment.SegmentBrush
        // values[1]: Timeline.SegmentBrush
        if (values.Length >= 1 && values[0] is Brush segmentBrush)
        {
            return segmentBrush;
        }

        if (values.Length >= 2 && values[1] is Brush timelineBrush)
        {
            return timelineBrush;
        }

        return DefaultBrush;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// 보간(Interpolation) 타입
/// </summary>
public enum InterpolationType
{
    /// <summary>선형 보간</summary>
    Linear,

    /// <summary>시작 시 가속</summary>
    EaseIn,

    /// <summary>끝에서 감속</summary>
    EaseOut,

    /// <summary>시작/끝 가감속</summary>
    EaseInOut,

    /// <summary>보간 없음 (즉시 전환)</summary>
    Hold,

    /// <summary>커스텀 (Bezier 등)</summary>
    Custom
}

/// <summary>
/// 키프레임 세그먼트 - 두 키프레임 사이의 구간을 나타냄
/// GanttView의 Task Bar와 유사하게 시간 범위를 시각적으로 표현
/// </summary>
public class KeyframeSegment : INotifyPropertyChanged
{
    private double _startTime;
    private double _endTime;
    private string? _label;
    private InterpolationType _interpolation = InterpolationType.Linear;
    private Brush? _segmentBrush;
    private bool _isSelected;
    private double _canvasX;
    private double _canvasWidth;
    private object? _tag;

    /// <summary>
    /// Gets or sets 시작 시간 (초)
    /// </summary>
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

    /// <summary>
    /// Gets or sets 종료 시간 (초)
    /// </summary>
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

    /// <summary>
    /// Gets 구간 길이 (초)
    /// </summary>
    public double Duration => Math.Max(0, EndTime - StartTime);

    /// <summary>
    /// Gets or sets 라벨 텍스트
    /// </summary>
    public string? Label
    {
        get => _label;
        set => SetProperty(ref _label, value);
    }

    /// <summary>
    /// Gets or sets 보간 타입
    /// </summary>
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

    /// <summary>
    /// Gets or sets 세그먼트 배경 브러시 (null이면 기본색 사용)
    /// </summary>
    public Brush? SegmentBrush
    {
        get => _segmentBrush;
        set => SetProperty(ref _segmentBrush, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether 선택 여부
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>
    /// Gets or sets canvas X 좌표 (내부 계산용)
    /// </summary>
    public double CanvasX
    {
        get => _canvasX;
        set => SetProperty(ref _canvasX, value);
    }

    /// <summary>
    /// Gets or sets canvas 너비 (내부 계산용)
    /// </summary>
    public double CanvasWidth
    {
        get => _canvasWidth;
        set => SetProperty(ref _canvasWidth, value);
    }

    /// <summary>
    /// Gets or sets 사용자 데이터 (Animation Clip 참조 등)
    /// </summary>
    public object? Tag
    {
        get => _tag;
        set => SetProperty(ref _tag, value);
    }

    /// <summary>
    /// Gets 툴팁 텍스트
    /// </summary>
    public string TooltipText
    {
        get
        {
            var label = string.IsNullOrEmpty(Label) ? "Segment" : Label;
            return $"{label}\nTime: {StartTime:F2}s ~ {EndTime:F2}s\nDuration: {Duration:F2}s\nInterpolation: {Interpolation}";
        }
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

/// <summary>
/// 키프레임 포인트 - 동일 시간에 여러 Snapshot을 담는 Container
/// X축의 특정 시간에 배치되며, 여러 종류의 Snapshot을 포함할 수 있음
/// </summary>
public class KeyframePoint : INotifyPropertyChanged
{
    private double _time;
    private bool _isSelected;
    private double _canvasX;
    private object? _tag;

    /// <summary>
    /// Gets or sets 키프레임 시간 (초)
    /// </summary>
    public double Time
    {
        get => _time;
        set => SetProperty(ref _time, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether 선택 여부
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>
    /// Gets or sets canvas X 좌표 (내부 계산용)
    /// </summary>
    public double CanvasX
    {
        get => _canvasX;
        set => SetProperty(ref _canvasX, value);
    }

    /// <summary>
    /// Gets or sets 사용자 데이터 (Keyframe DTO 참조 등)
    /// </summary>
    public object? Tag
    {
        get => _tag;
        set => SetProperty(ref _tag, value);
    }

    /// <summary>
    /// Gets 이 포인트에 포함된 Snapshot 이름들 (Transform, Visibility, Color)
    /// </summary>
    public ObservableCollection<string> SnapshotNames { get; } = new();

    /// <summary>
    /// Gets 툴팁 텍스트
    /// </summary>
    public string TooltipText
    {
        get
        {
            var snapshots = SnapshotNames.Count > 0 ? string.Join(", ", SnapshotNames) : "Empty";
            return $"Time: {Time:F2}s\nSnapshots: {snapshots}";
        }
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
