// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Data;

namespace Wpf.Ui.Controls;

/// <summary>
/// Keyframe Time을 Canvas X 위치로 변환 (각 타임라인별 독립 계산)
/// MultiBinding: [0]=Time, [1]=ViewportStart, [2]=ViewportDuration, [3]=CanvasWidth
/// </summary>
public class TimeToCanvasXConverter : IMultiValueConverter
{
    public static readonly TimeToCanvasXConverter Instance = new();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 4)
        {
            return 0.0;
        }

        if (values[0] is not double time ||
            values[1] is not double viewportStart ||
            values[2] is not double viewportDuration ||
            values[3] is not double canvasWidth)
        {
            return 0.0;
        }

        if (viewportDuration <= 0 || canvasWidth <= 0)
        {
            return 0.0;
        }

        return ((time - viewportStart) / viewportDuration) * canvasWidth;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
