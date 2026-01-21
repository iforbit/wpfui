// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;

namespace Wpf.Ui.Controls;

/// <summary>
/// MultiTrackTimeline - Drawing, Refresh, Interpolation, Time Conversion
/// </summary>
public partial class MultiTrackTimeline
{
    /// <summary>
    /// 전체 타임라인 새로고침
    /// </summary>
    public void RefreshTimeline()
    {
        DrawTimeRuler();
        UpdateScrubberPosition();
        UpdateViewportThumb();
    }

    /// <summary>
    /// 모든 트랙 새로고침
    /// </summary>
    public void RefreshAllTracks()
    {
        if (_trackListPanel != null && TrackGroups != null)
        {
            _trackListPanel.SetCurrentValue(ItemsControl.ItemsSourceProperty, TrackGroups);
        }

        RefreshTimeline();
    }

    /// <summary>
    /// 현재 시간 기준 값 업데이트
    /// </summary>
    private void UpdateCurrentValues()
    {
        if (TrackGroups == null)
        {
            return;
        }

        foreach (ITimelineTrackGroup group in TrackGroups)
        {
            foreach (ITimelineTrack track in group.Tracks)
            {
                track.CurrentValue = GetInterpolatedValue(track, CurrentTime);
            }
        }
    }

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

        double majorInterval = CalculateMajorInterval(ViewportDuration);
        double minorInterval = majorInterval / 5;

        var tickBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
        var textBrush = new SolidColorBrush(Color.FromRgb(160, 160, 160));

        double viewportEnd = ViewportStart + ViewportDuration;
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
                var line = new System.Windows.Shapes.Line
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

        // Major ticks + labels
        double firstMajor = Math.Ceiling(ViewportStart / majorInterval) * majorInterval;

        for (double t = firstMajor; t <= viewportEnd; t += majorInterval)
        {
            double x = TimeToX(t, width);
            if (x < -20 || x > width + 20)
            {
                continue;
            }

            var line = new System.Windows.Shapes.Line
            {
                X1 = x,
                Y1 = height - 10,
                X2 = x,
                Y2 = height,
                Stroke = tickBrush,
                StrokeThickness = 1
            };
            _ = _timeRuler.Children.Add(line);

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
        if (_scrubber == null || _trackCanvas == null)
        {
            return;
        }

        double canvasWidth = _trackCanvas.ActualWidth;
        if (canvasWidth <= 0)
        {
            return;
        }

        double viewportEnd = ViewportStart + ViewportDuration;
        if (CurrentTime < ViewportStart || CurrentTime > viewportEnd)
        {
            _scrubber.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            return;
        }

        _scrubber.SetCurrentValue(VisibilityProperty, Visibility.Visible);
        double x = TimeToX(CurrentTime, canvasWidth);
        double scrubberWidth = _scrubber.Width > 0 ? _scrubber.Width : _scrubber.ActualWidth;
        if (scrubberWidth <= 0)
        {
            scrubberWidth = 18;
        }

        Canvas.SetLeft(_scrubber, x - (scrubberWidth / 2));
    }

    /// <summary>
    /// 특정 시간의 보간된 값 계산
    /// </summary>
    private object? GetInterpolatedValue(ITimelineTrack track, double time)
    {
        var keyframes = track.Keyframes.OrderBy(k => k.Time).ToList();
        if (keyframes.Count == 0)
        {
            return track.CurrentValue;
        }

        if (time <= keyframes[0].Time)
        {
            return keyframes[0].Value;
        }

        if (time >= keyframes[^1].Time)
        {
            return keyframes[^1].Value;
        }

        for (int i = 0; i < keyframes.Count - 1; i++)
        {
            ITimelineKeyframe kf1 = keyframes[i];
            ITimelineKeyframe kf2 = keyframes[i + 1];

            if (time >= kf1.Time && time <= kf2.Time)
            {
                if (kf1.Interpolation == InterpolationType.Hold)
                {
                    return kf1.Value;
                }

                double t = (time - kf1.Time) / (kf2.Time - kf1.Time);
                t = ApplyEasing(t, kf1.Interpolation);
                return InterpolateValue(kf1.Value, kf2.Value, t, track.ValueType);
            }
        }

        return track.CurrentValue;
    }

    /// <summary>
    /// Easing 적용
    /// </summary>
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

    /// <summary>
    /// 값 보간
    /// </summary>
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

                break;

            case TrackValueType.Color:
                // TODO: Color 보간
                return t < 0.5 ? v1 : v2;

            case TrackValueType.Enum:
            case TrackValueType.Boolean:
                return t < 1.0 ? v1 : v2;
        }

        return v1;
    }

    private double TimeToX(double time, double canvasWidth)
    {
        if (ViewportDuration <= 0)
        {
            return 0;
        }

        return ((time - ViewportStart) / ViewportDuration) * canvasWidth;
    }

    private double XToTime(double x, double canvasWidth)
    {
        if (canvasWidth <= 0)
        {
            return 0;
        }

        return ViewportStart + ((x / canvasWidth) * ViewportDuration);
    }
}
