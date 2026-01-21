// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Data;

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
