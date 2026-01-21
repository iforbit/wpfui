// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace Wpf.Ui.Controls;

/// <summary>
/// 트랙 기본 구현
/// </summary>
public class TimelineTrack : ITimelineTrack
{
    private string _propertyName = string.Empty;
    private string? _displayName;
    private TrackValueType _valueType = TrackValueType.Numeric;
    private object? _currentValue;
    private bool _isSelected;
    private bool _isLocked;
    private bool _isVisible = true;
    private bool _autoBuildSegments = false;
    private double? _minValue;
    private double? _maxValue;
    private IEnumerable<object>? _enumValues;

    public string PropertyName
    {
        get => _propertyName;
        set => SetProperty(ref _propertyName, value);
    }

    public string? DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    public TrackValueType ValueType
    {
        get => _valueType;
        set => SetProperty(ref _valueType, value);
    }

    public object? CurrentValue
    {
        get => _currentValue;
        set => SetProperty(ref _currentValue, value);
    }

    public IEnumerable<object>? EnumValues
    {
        get => _enumValues;
        set => SetProperty(ref _enumValues, value);
    }

    public double? MinValue
    {
        get => _minValue;
        set => SetProperty(ref _minValue, value);
    }

    public double? MaxValue
    {
        get => _maxValue;
        set => SetProperty(ref _maxValue, value);
    }

    public ObservableCollection<ITimelineKeyframe> Keyframes { get; } = new();

    public ObservableCollection<ITimelineSegment> Segments { get; } = new();

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public bool IsLocked
    {
        get => _isLocked;
        set => SetProperty(ref _isLocked, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether 키프레임 변경 시 세그먼트 자동 재구성 여부
    /// false로 설정하면 KeyframeTimeline처럼 독립적인 세그먼트로 동작
    /// </summary>
    public bool AutoBuildSegments
    {
        get => _autoBuildSegments;
        set => SetProperty(ref _autoBuildSegments, value);
    }

    public TimelineTrack()
    {
        Keyframes.CollectionChanged += OnKeyframesCollectionChanged;
    }

    public TimelineTrack(string propertyName, TrackValueType valueType)
    {
        _propertyName = propertyName;
        _valueType = valueType;
        Keyframes.CollectionChanged += OnKeyframesCollectionChanged;
    }

    private void OnKeyframesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (AutoBuildSegments)
        {
            RebuildSegments();
        }
    }

    /// <summary>
    /// 키프레임 기반으로 세그먼트 재구성
    /// </summary>
    public void RebuildSegments()
    {
        Segments.Clear();

        List<ITimelineKeyframe> sortedKeyframes = GetSortedKeyframes();
        for (int i = 0; i < sortedKeyframes.Count - 1; i++)
        {
            var segment = new TimelineSegment(sortedKeyframes[i], sortedKeyframes[i + 1]);
            Segments.Add(segment);
        }
    }

    /// <summary>
    /// Keyframes를 Time 기준 정렬된 리스트로 반환 (LINQ 없이)
    /// </summary>
    private List<ITimelineKeyframe> GetSortedKeyframes()
    {
        var list = new List<ITimelineKeyframe>(Keyframes);
        list.Sort((a, b) => a.Time.CompareTo(b.Time));
        return list;
    }

    /// <summary>
    /// Numeric 트랙 생성
    /// </summary>
    public static TimelineTrack CreateNumeric(string propertyName, double? min = null, double? max = null, string? displayName = null)
    {
        return new TimelineTrack
        {
            PropertyName = propertyName,
            DisplayName = displayName,
            ValueType = TrackValueType.Numeric,
            MinValue = min,
            MaxValue = max
        };
    }

    /// <summary>
    /// Color 트랙 생성
    /// </summary>
    public static TimelineTrack CreateColor(string propertyName, string? displayName = null)
    {
        return new TimelineTrack
        {
            PropertyName = propertyName,
            DisplayName = displayName,
            ValueType = TrackValueType.Color
        };
    }

    /// <summary>
    /// Enum 트랙 생성
    /// </summary>
    /// <typeparam name="TEnum">The enum type for the track values.</typeparam>
    public static TimelineTrack CreateEnum<TEnum>(string propertyName, string? displayName = null)
        where TEnum : struct, Enum
    {
        // LINQ 없이 Enum 값들을 object[]로 변환
        Array enumArray = Enum.GetValues(typeof(TEnum));
        var boxedValues = new object[enumArray.Length];
        for (int i = 0; i < enumArray.Length; i++)
        {
            boxedValues[i] = enumArray.GetValue(i)!;
        }

        return new TimelineTrack
        {
            PropertyName = propertyName,
            DisplayName = displayName,
            ValueType = TrackValueType.Enum,
            EnumValues = boxedValues
        };
    }

    /// <summary>
    /// Boolean 트랙 생성
    /// </summary>
    public static TimelineTrack CreateBoolean(string propertyName, string? displayName = null)
    {
        return new TimelineTrack
        {
            PropertyName = propertyName,
            DisplayName = displayName,
            ValueType = TrackValueType.Boolean
        };
    }

    /// <summary>
    /// 배열 기반 Numeric 트랙 생성 (Keyframe + Segment 자동 생성)
    /// </summary>
    /// <param name="propertyName">속성 이름</param>
    /// <param name="times">시간 배열</param>
    /// <param name="values">값 배열 (times와 동일한 길이)</param>
    /// <param name="interpolation">보간 타입</param>
    /// <param name="displayName">표시 이름</param>
    public static TimelineTrack CreateNumericFromArrays(
        string propertyName,
        double[] times,
        double[] values,
        InterpolationType interpolation = InterpolationType.Linear,
        string? displayName = null)
    {
        if (times.Length != values.Length)
        {
            throw new ArgumentException("times와 values 배열의 길이가 같아야 합니다.");
        }

        // Min/Max 계산 (LINQ 없이)
        double? minVal = null;
        double? maxVal = null;
        if (values.Length > 0)
        {
            minVal = values[0];
            maxVal = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] < minVal)
                {
                    minVal = values[i];
                }

                if (values[i] > maxVal)
                {
                    maxVal = values[i];
                }
            }
        }

        var track = new TimelineTrack
        {
            PropertyName = propertyName,
            DisplayName = displayName,
            ValueType = TrackValueType.Numeric,
            MinValue = minVal,
            MaxValue = maxVal
        };

        // double[] → object[] 변환 (LINQ 없이)
        var boxedValues = new object[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            boxedValues[i] = values[i];
        }

        track.BuildFromArrays(times, boxedValues, interpolation);
        return track;
    }

    /// <summary>
    /// 배열로부터 1개 Segment + Waypoints 생성
    /// [1, 9, 5] → 1개 Segment (1→5) + 1개 Waypoint (9)
    /// </summary>
    /// <param name="times">시간 배열</param>
    /// <param name="values">값 배열</param>
    /// <param name="interpolation">보간 타입</param>
    public void BuildFromArrays(double[] times, object[] values, InterpolationType interpolation = InterpolationType.Linear)
    {
        if (times.Length != values.Length)
        {
            throw new ArgumentException("times와 values 배열의 길이가 같아야 합니다.");
        }

        if (times.Length < 2)
        {
            return; // 최소 2개 필요 (시작, 끝)
        }

        Keyframes.Clear();
        Segments.Clear();

        // Keyframes 생성 (시작, 끝만)
        Keyframes.Add(new TimelineKeyframe(times[0], values[0], interpolation));
        Keyframes.Add(new TimelineKeyframe(times[times.Length - 1], values[values.Length - 1], interpolation));

        // 1개 Segment 생성 (전체 구간)
        double startTime = times[0];
        double endTime = times[times.Length - 1];
        double totalDuration = endTime - startTime;

        var segment = new TimelineSegment(
            startTime,
            endTime,
            values[0],
            values[values.Length - 1],
            interpolation);

        // 중간값들은 Waypoints로
        for (int i = 1; i < times.Length - 1; i++)
        {
            double relativePos = (times[i] - startTime) / totalDuration;
            segment.Waypoints.Add(new SegmentWaypoint(relativePos, values[i]));
        }

        Segments.Add(segment);
    }

    /// <summary>
    /// 특정 위치에 새 값 삽입 (Keyframe + Segment 자동 갱신)
    /// </summary>
    /// <param name="time">삽입할 시간</param>
    /// <param name="value">삽입할 값</param>
    /// <param name="interpolation">보간 타입</param>
    public void InsertValue(double time, object value, InterpolationType interpolation = InterpolationType.Linear)
    {
        // 이미 같은 시간에 Keyframe이 있는지 확인
        ITimelineKeyframe? existingKeyframe = null;
        foreach (ITimelineKeyframe kf in Keyframes)
        {
            if (Math.Abs(kf.Time - time) < 0.001)
            {
                existingKeyframe = kf;
                break;
            }
        }

        if (existingKeyframe != null)
        {
            existingKeyframe.Value = value;
            RebuildSegmentsWithInterpolation(interpolation);
            return;
        }

        // 새 Keyframe 추가
        var newKeyframe = new TimelineKeyframe(time, value, interpolation);
        Keyframes.Add(newKeyframe);

        // Segment 재구성
        RebuildSegmentsWithInterpolation(interpolation);
    }

    /// <summary>
    /// Keyframes로부터 Segments 재구성 (보간 타입 지정)
    /// </summary>
    private void RebuildSegmentsWithInterpolation(InterpolationType interpolation = InterpolationType.Linear)
    {
        Segments.Clear();
        List<ITimelineKeyframe> sortedKeyframes = GetSortedKeyframes();

        for (int i = 0; i < sortedKeyframes.Count - 1; i++)
        {
            ITimelineKeyframe start = sortedKeyframes[i];
            ITimelineKeyframe end = sortedKeyframes[i + 1];
            Segments.Add(new TimelineSegment(
                start.Time,
                end.Time,
                start.Value,
                end.Value,
                start.Interpolation));
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
