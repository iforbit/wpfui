// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Gallery.ViewModels.Pages.Media;

public partial class KeyframeTimelineViewModel : ViewModel
{
    private const double PlaybackInterval = 1.0 / 60.0; // 60 FPS
    private const double PlaybackSpeed = 1.0; // 1x speed

    private readonly DispatcherTimer _playbackTimer;

    [ObservableProperty]
    private double _duration = 2.0;

    [ObservableProperty]
    private double _currentTime = 0.0;

    [ObservableProperty]
    private KeyframePoint? _selectedKeyframe;

    [ObservableProperty]
    private KeyframeSegment? _selectedSegment;

    [ObservableProperty]
    private ObservableCollection<KeyframePoint> _keyframePoints = new();

    [ObservableProperty]
    private ObservableCollection<KeyframeSegment> _keyframeSegments = new();

    [ObservableProperty]
    private bool _isPlaying = false;

    [ObservableProperty]
    private string _statusText = "Ready";

    public KeyframeTimelineViewModel()
    {
        // Setup playback timer
        _playbackTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(PlaybackInterval)
        };
        _playbackTimer.Tick += OnPlaybackTimerTick;

        // 샘플 Keyframe 포인트 추가
        KeyframePoints.Add(new KeyframePoint { Time = 0.0 });
        KeyframePoints.Add(new KeyframePoint { Time = 0.5 });
        KeyframePoints.Add(new KeyframePoint { Time = 1.0 });
        KeyframePoints.Add(new KeyframePoint { Time = 1.5 });
        KeyframePoints.Add(new KeyframePoint { Time = 2.0 });

        // 샘플 Segment 추가 (포인트 간 연결)
        KeyframeSegments.Add(new KeyframeSegment
        {
            StartTime = 0.0,
            EndTime = 0.5,
            Label = "Intro",
            Interpolation = InterpolationType.EaseOut,
            SegmentBrush = new SolidColorBrush(Color.FromArgb(180, 0, 150, 136))
        });
        KeyframeSegments.Add(new KeyframeSegment
        {
            StartTime = 0.5,
            EndTime = 1.0,
            Label = "Main",
            Interpolation = InterpolationType.Linear,
            SegmentBrush = new SolidColorBrush(Color.FromArgb(180, 33, 150, 243))
        });
        KeyframeSegments.Add(new KeyframeSegment
        {
            StartTime = 1.0,
            EndTime = 1.5,
            Label = "Transition",
            Interpolation = InterpolationType.EaseInOut,
            SegmentBrush = new SolidColorBrush(Color.FromArgb(180, 156, 39, 176))
        });
        KeyframeSegments.Add(new KeyframeSegment
        {
            StartTime = 1.5,
            EndTime = 2.0,
            Label = "Outro",
            Interpolation = InterpolationType.EaseIn,
            SegmentBrush = new SolidColorBrush(Color.FromArgb(180, 255, 152, 0))
        });
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
    private void AddKeyframe()
    {
        var newPoint = new KeyframePoint { Time = CurrentTime };
        KeyframePoints.Add(newPoint);
        SelectedKeyframe = newPoint;
        StatusText = $"Added keyframe at {CurrentTime:F2}s";
    }

    [RelayCommand]
    private void RemoveKeyframe()
    {
        if (SelectedKeyframe != null)
        {
            _ = KeyframePoints.Remove(SelectedKeyframe);
            StatusText = "Keyframe removed";
            SelectedKeyframe = null;
        }
    }

    [RelayCommand]
    private void ClearKeyframes()
    {
        KeyframePoints.Clear();
        SelectedKeyframe = null;
        StatusText = "All keyframes cleared";
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

    partial void OnCurrentTimeChanged(double value)
    {
        if (!IsPlaying)
        {
            StatusText = $"Current time: {value:F2}s";
        }
    }

    partial void OnSelectedKeyframeChanged(KeyframePoint? value)
    {
        if (value != null)
        {
            StatusText = $"Selected keyframe at {value.Time:F2}s";
        }
    }

    partial void OnSelectedSegmentChanged(KeyframeSegment? value)
    {
        if (value != null)
        {
            var label = string.IsNullOrEmpty(value.Label) ? "Segment" : value.Label;
            StatusText = $"Selected {label}: {value.StartTime:F2}s ~ {value.EndTime:F2}s ({value.Interpolation})";
        }
    }
}
