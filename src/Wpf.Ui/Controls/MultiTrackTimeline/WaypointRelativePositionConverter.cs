// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Data;

namespace Wpf.Ui.Controls;

/// <summary>
/// Waypoint RelativePosition을 Segment Grid 내 Canvas.Left로 변환
/// MultiBinding: [0]=RelativePosition, [1]=GridWidth
/// </summary>
public class WaypointRelativePositionConverter : IMultiValueConverter
{
    public static readonly WaypointRelativePositionConverter Instance = new();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 ||
            values[0] is not double relativePosition ||
            values[1] is not double gridWidth)
        {
            return 0.0;
        }

        return relativePosition * gridWidth;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
