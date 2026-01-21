// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.Controls;

namespace Wpf.Ui.Gallery.ViewModels.Pages.Media;

public partial class MultiTrackTimelineViewModel : ViewModel
{
    private const double PlaybackInterval = 1.0 / 60.0; // 60 FPS
    private const double PlaybackSpeed = 1.0; // 1x speed

    private readonly DispatcherTimer _playbackTimer;

    [ObservableProperty]
    private double _duration = 3.0;

    [ObservableProperty]
    private double _currentTime = 0.0;

    [ObservableProperty]
    private ITimelineTrack? _selectedTrack;

    [ObservableProperty]
    private ITimelineKeyframe? _selectedKeyframe;

    [ObservableProperty]
    private ITimelineSegment? _selectedSegment;

    [ObservableProperty]
    private ObservableCollection<ITimelineTrackGroup> _trackGroups = new();

    [ObservableProperty]
    private bool _isPlaying = false;

    [ObservableProperty]
    private string _statusText = "Ready";

    // 초기값은 null!, 생성자에서 BrushPresets 인스턴스로 설정
    [ObservableProperty]
    private Brush _scrubberBrush = null!;

    [ObservableProperty]
    private Brush _segmentBrush = null!;

    [ObservableProperty]
    private Brush _segmentSelectedBrush = null!;

    [ObservableProperty]
    private Brush _keyframeBrush = null!;

    [ObservableProperty]
    private Brush _keyframeSelectedBrush = null!;

    [ObservableProperty]
    private Brush _waypointBrush = null!;

    /// <summary>
    /// Gets 프리셋 색상 목록 (Palette.xaml 기반)
    /// </summary>
    public List<BrushPreset> BrushPresets { get; } =
    [
        new("Red", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"))),
        new("Pink", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E91E63"))),
        new("Purple", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9C27B0"))),
        new("DeepPurple", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#673AB7"))),
        new("Indigo", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3F51B5"))),
        new("Blue", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"))),
        new("LightBlue", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#03A9F4"))),
        new("Cyan", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00BCD4"))),
        new("Teal", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#009688"))),
        new("Green", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"))),
        new("LightGreen", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8BC34A"))),
        new("Lime", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CDDC39"))),
        new("Yellow", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEB3B"))),
        new("Amber", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC107"))),
        new("Orange", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"))),
        new("DeepOrange", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5722"))),
    ];

    public MultiTrackTimelineViewModel()
    {
        // BrushPresets에서 초기 Brush 값 설정 (ComboBox SelectedValue 매칭용)
        _scrubberBrush = BrushPresets.First(p => p.Name == "Red").Brush;
        _segmentBrush = BrushPresets.First(p => p.Name == "Blue").Brush;
        _segmentSelectedBrush = BrushPresets.First(p => p.Name == "Yellow").Brush;  // 눈에 띄는 색상
        _keyframeBrush = BrushPresets.First(p => p.Name == "Blue").Brush;
        _keyframeSelectedBrush = BrushPresets.First(p => p.Name == "Green").Brush;
        _waypointBrush = BrushPresets.First(p => p.Name == "DeepOrange").Brush;

        // Setup playback timer
        _playbackTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(PlaybackInterval)
        };
        _playbackTimer.Tick += OnPlaybackTimerTick;

        // Create sample track groups
        InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        // Transform Group - Keyframe/Segment 충돌 테스트용
        var transformGroup = new TimelineTrackGroup("Transform");

        // Rotation: Keyframe과 Segment가 분리됨 (충돌 테스트용)
        // Keyframes: 0.0s, 0.4s (왼쪽) | Segment: 1.2s~2.5s (오른쪽) + Waypoints
        var rotationTrack = TimelineTrack.CreateNumeric("Rotation", -360, 360, "Rotation");
        rotationTrack.Keyframes.Add(new TimelineKeyframe(0.0, 0.0, InterpolationType.EaseOut));
        rotationTrack.Keyframes.Add(new TimelineKeyframe(0.4, 45.0, InterpolationType.EaseOut));

        // Segment는 1.2s부터 시작 - Keyframe(0.4s)과 분리됨
        var rotationSegment = new TimelineSegment(1.2, 2.5, 90.0, 270.0, InterpolationType.EaseInOut);

        // Waypoints: 중간값들 (Diamond 마커로 표시)
        rotationSegment.Waypoints.Add(new SegmentWaypoint(0.3, 150.0));  // 30% 위치, 150도
        rotationSegment.Waypoints.Add(new SegmentWaypoint(0.7, 220.0));  // 70% 위치, 220도
        rotationTrack.Segments.Add(rotationSegment);
        transformGroup.Tracks.Add(rotationTrack);

        // Scale: Segment만 있음 (Keyframe 없음)
        var scaleTrack = TimelineTrack.CreateNumeric("Scale", 0.1, 3.0, "Scale");
        scaleTrack.Segments.Add(new TimelineSegment(0.5, 1.5, 1.0, 2.0, InterpolationType.EaseIn));
        scaleTrack.Segments.Add(new TimelineSegment(2.0, 2.8, 1.5, 1.0, InterpolationType.EaseOut));
        transformGroup.Tracks.Add(scaleTrack);

        TrackGroups.Add(transformGroup);

        // Appearance Group - Keyframe만 있음 (기존 패턴)
        var appearanceGroup = new TimelineTrackGroup("Appearance");

        var opacityTrack = TimelineTrack.CreateNumeric("Opacity", 0, 1, "Opacity");
        opacityTrack.Keyframes.Add(new TimelineKeyframe(0.0, 1.0, InterpolationType.Linear));
        opacityTrack.Keyframes.Add(new TimelineKeyframe(1.0, 0.5, InterpolationType.EaseOut));
        opacityTrack.Keyframes.Add(new TimelineKeyframe(2.0, 1.0, InterpolationType.EaseIn));
        appearanceGroup.Tracks.Add(opacityTrack);

        var colorTrack = TimelineTrack.CreateColor("Fill", "Fill Color");
        colorTrack.Keyframes.Add(new TimelineKeyframe(0.0, "#FF0078D4", InterpolationType.Linear));
        colorTrack.Keyframes.Add(new TimelineKeyframe(1.5, "#FFFF6B00", InterpolationType.Linear));
        colorTrack.Keyframes.Add(new TimelineKeyframe(3.0, "#FF00CC6A", InterpolationType.Linear));
        appearanceGroup.Tracks.Add(colorTrack);

        TrackGroups.Add(appearanceGroup);

        // State Group - Keyframe + Segment 혼합 (분리됨)
        var stateGroup = new TimelineTrackGroup("State");

        // Visibility: Keyframe(0.0s) | Segment(1.0s~2.0s) | Keyframe(2.8s)
        var visibilityTrack = TimelineTrack.CreateBoolean("IsVisible", "Visibility");
        visibilityTrack.Keyframes.Add(new TimelineKeyframe(0.0, true, InterpolationType.Hold));
        visibilityTrack.Segments.Add(new TimelineSegment(1.0, 2.0, false, false, InterpolationType.Hold));
        visibilityTrack.Keyframes.Add(new TimelineKeyframe(2.8, true, InterpolationType.Hold));
        stateGroup.Tracks.Add(visibilityTrack);

        TrackGroups.Add(stateGroup);
    }

    private void OnPlaybackTimerTick(object? sender, EventArgs e)
    {
        CurrentTime += PlaybackInterval * PlaybackSpeed;

        // Loop back to start when reaching end
        if (CurrentTime >= Duration)
        {
            CurrentTime = 0.0;
        }
    }

    [RelayCommand]
    private void Play()
    {
        IsPlaying = true;
        _playbackTimer.Start();
        StatusText = "Playing...";
    }

    [RelayCommand]
    private void Stop()
    {
        IsPlaying = false;
        _playbackTimer.Stop();
        CurrentTime = 0;
        StatusText = "Stopped";
    }

    [RelayCommand]
    private void Pause()
    {
        IsPlaying = false;
        _playbackTimer.Stop();
        StatusText = $"Paused at {CurrentTime:F2}s";
    }

    [RelayCommand]
    private void AddKeyframe()
    {
        if (SelectedTrack != null)
        {
            var newKeyframe = new TimelineKeyframe(CurrentTime, SelectedTrack.CurrentValue);
            SelectedTrack.Keyframes.Add(newKeyframe);
            SelectedKeyframe = newKeyframe;
            StatusText = $"Added keyframe to {SelectedTrack.PropertyName} at {CurrentTime:F2}s";
        }
        else
        {
            StatusText = "Select a track first";
        }
    }

    [RelayCommand]
    private void RemoveKeyframe()
    {
        if (SelectedKeyframe != null && SelectedTrack != null)
        {
            _ = SelectedTrack.Keyframes.Remove(SelectedKeyframe);
            StatusText = "Keyframe removed";
            SelectedKeyframe = null;
        }
    }

    [RelayCommand]
    private void AddWaypoint()
    {
        if (SelectedSegment == null)
        {
            StatusText = "Select a segment first";
            return;
        }

        // CurrentTime이 Segment 범위 내에 있는지 확인
        if (CurrentTime < SelectedSegment.StartTime || CurrentTime > SelectedSegment.EndTime)
        {
            StatusText = $"Scrubber must be within segment ({SelectedSegment.StartTime:F2}s ~ {SelectedSegment.EndTime:F2}s)";
            return;
        }

        // CurrentTime을 Segment 내 상대 위치로 변환
        double relativePos = (CurrentTime - SelectedSegment.StartTime) / SelectedSegment.Duration;
        relativePos = Math.Clamp(relativePos, 0.01, 0.99);

        // 보간된 값 계산
        object? interpolatedValue = SelectedSegment.GetInterpolatedValue(
            CurrentTime,
            SelectedTrack?.ValueType ?? TrackValueType.Numeric);

        var newWaypoint = new SegmentWaypoint(relativePos, interpolatedValue ?? 0.0)
        {
            ParentSegment = SelectedSegment
        };
        SelectedSegment.Waypoints.Add(newWaypoint);
        StatusText = $"Added waypoint at {CurrentTime:F2}s ({relativePos:P0})";
    }

    [RelayCommand]
    private void RemoveWaypoint()
    {
        if (SelectedSegment != null && SelectedSegment.Waypoints.Count > 0)
        {
            // 마지막 Waypoint 제거
            SelectedSegment.Waypoints.RemoveAt(SelectedSegment.Waypoints.Count - 1);
            StatusText = "Waypoint removed";
        }
        else
        {
            StatusText = "No waypoint to remove";
        }
    }

    [RelayCommand]
    private void ClearAllKeyframes()
    {
        foreach (ITimelineTrackGroup group in TrackGroups)
        {
            foreach (ITimelineTrack track in group.Tracks)
            {
                track.Keyframes.Clear();
            }
        }

        SelectedKeyframe = null;
        StatusText = "All keyframes cleared";
    }

    [RelayCommand]
    private void ResetToSample()
    {
        TrackGroups.Clear();
        InitializeSampleData();
        SelectedTrack = null;
        SelectedKeyframe = null;
        CurrentTime = 0;
        StatusText = "Reset to sample data";
    }

    partial void OnCurrentTimeChanged(double value)
    {
        if (!IsPlaying)
        {
            StatusText = $"Current time: {value:F2}s";
        }
    }

    partial void OnSelectedTrackChanged(ITimelineTrack? value)
    {
        if (value != null)
        {
            StatusText = $"Selected track: {value.DisplayName ?? value.PropertyName}";
        }
    }

    partial void OnSelectedKeyframeChanged(ITimelineKeyframe? value)
    {
        if (value != null)
        {
            StatusText = $"Selected keyframe at {value.Time:F2}s (Value: {value.Value})";
        }
    }
}
