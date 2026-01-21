// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Runtime.CompilerServices;

namespace Wpf.Ui.Controls;

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
