// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Runtime.CompilerServices;

namespace Wpf.Ui.Controls;

/// <summary>
/// 키프레임 기본 구현
/// </summary>
public class TimelineKeyframe : ITimelineKeyframe
{
    private double _time;
    private object? _value;
    private InterpolationType _interpolation = InterpolationType.Linear;
    private string? _bezierControlPoints;
    private bool _isSelected;
    private double _canvasX;

    public double Time
    {
        get => _time;
        set => SetProperty(ref _time, value);
    }

    public object? Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public InterpolationType Interpolation
    {
        get => _interpolation;
        set => SetProperty(ref _interpolation, value);
    }

    public string? BezierControlPoints
    {
        get => _bezierControlPoints;
        set => SetProperty(ref _bezierControlPoints, value);
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

    public TimelineKeyframe()
    {
    }

    public TimelineKeyframe(double time, object? value, InterpolationType interpolation = InterpolationType.Linear)
    {
        _time = time;
        _value = value;
        _interpolation = interpolation;
    }

    /// <summary>
    /// Numeric 키프레임 생성
    /// </summary>
    public static TimelineKeyframe CreateNumeric(double time, double value, InterpolationType interpolation = InterpolationType.Linear)
    {
        return new TimelineKeyframe(time, value, interpolation);
    }

    /// <summary>
    /// Color 키프레임 생성
    /// </summary>
    public static TimelineKeyframe CreateColor(double time, string colorHex, InterpolationType interpolation = InterpolationType.Linear)
    {
        return new TimelineKeyframe(time, colorHex, interpolation);
    }

    /// <summary>
    /// Enum 키프레임 생성 (Step 보간 기본)
    /// </summary>
    /// <typeparam name="TEnum">The enum type for the keyframe value.</typeparam>
    public static TimelineKeyframe CreateEnum<TEnum>(double time, TEnum value)
        where TEnum : struct, Enum
    {
        return new TimelineKeyframe(time, value, InterpolationType.Hold);
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
