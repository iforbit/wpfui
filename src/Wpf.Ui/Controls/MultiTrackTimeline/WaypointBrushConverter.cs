// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Data;

namespace Wpf.Ui.Controls;

/// <summary>
/// Waypoint Brush 변환 (컨트롤 WaypointBrush 우선, null이면 기본값)
/// </summary>
public class WaypointBrushConverter : IValueConverter
{
    public static readonly WaypointBrushConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Brush brush)
        {
            return brush;
        }

        return Application.Current.FindResource("PaletteDeepOrangeBrush") as Brush
               ?? new SolidColorBrush(Color.FromRgb(255, 87, 34));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
