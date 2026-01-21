// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Runtime.CompilerServices;

namespace Wpf.Ui.Controls;

/// <summary>
/// 세그먼트 내 중간값 (Waypoint)
/// </summary>
public class SegmentWaypoint : INotifyPropertyChanged
{
    private double _relativePosition; // 0.0 ~ 1.0 (Segment 내 상대 위치)
    private object? _value;
    private double _canvasX; // UI 렌더링용
    private ITimelineSegment? _parentSegment; // 부모 세그먼트 참조

    /// <summary>
    /// Gets or sets 부모 Segment 참조 (AbsoluteTime 계산에 사용)
    /// </summary>
    public ITimelineSegment? ParentSegment
    {
        get => _parentSegment;
        set
        {
            if (_parentSegment != value)
            {
                _parentSegment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AbsoluteTime));
            }
        }
    }

    /// <summary>
    /// Gets 절대 시간 (ParentSegment.StartTime + RelativePosition * Duration)
    /// </summary>
    public double AbsoluteTime
    {
        get
        {
            if (_parentSegment == null)
            {
                return 0;
            }

            return _parentSegment.StartTime + (_relativePosition * _parentSegment.Duration);
        }
    }

    /// <summary>
    /// Gets or sets segment 시작(0.0) ~ 끝(1.0) 사이 상대 위치
    /// </summary>
    public double RelativePosition
    {
        get => _relativePosition;
        set
        {
            _relativePosition = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AbsoluteTime));
        }
    }

    public object? Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged();
        }
    }

    public double CanvasX
    {
        get => _canvasX;
        set
        {
            _canvasX = value;
            OnPropertyChanged();
        }
    }

    public SegmentWaypoint() { }

    public SegmentWaypoint(double relativePosition, object? value)
    {
        _relativePosition = relativePosition;
        _value = value;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
