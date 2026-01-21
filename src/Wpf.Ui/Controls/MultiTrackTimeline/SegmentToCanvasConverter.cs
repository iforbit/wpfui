// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Data;

namespace Wpf.Ui.Controls;

/// <summary>
/// Segment StartTime/EndTime을 Canvas 위치/너비로 변환
/// MultiBinding: [0]=StartTime, [1]=EndTime, [2]=ViewportStart, [3]=ViewportDuration, [4]=CanvasWidth
/// Returns: Double (CanvasX when parameter="X", CanvasWidth when parameter="Width")
/// </summary>
public class SegmentToCanvasConverter : IMultiValueConverter
{
    public static readonly SegmentToCanvasConverter Instance = new();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 5)
        {
            return 0.0;
        }

        if (values[0] is not double startTime ||
            values[1] is not double endTime ||
            values[2] is not double viewportStart ||
            values[3] is not double viewportDuration ||
            values[4] is not double canvasWidth)
        {
            return 0.0;
        }

        if (viewportDuration <= 0 || canvasWidth <= 0)
        {
            return 0.0;
        }

        double startX = ((startTime - viewportStart) / viewportDuration) * canvasWidth;
        double endX = ((endTime - viewportStart) / viewportDuration) * canvasWidth;

        string? mode = parameter as string;
        if (mode == "Width")
        {
            return Math.Max(0, endX - startX);
        }

        return startX; // "X" or default
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
