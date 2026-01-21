// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Data;

namespace Wpf.Ui.Converters;

/// <summary>
/// Converts boolean to opacity value (true = 1.0, false = 0.3)
/// </summary>
internal class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // parameter format: "trueOpacity,falseOpacity" or just use defaults
        double trueOpacity = 1.0;
        double falseOpacity = 0.3;

        if (parameter is string paramStr && !string.IsNullOrEmpty(paramStr))
        {
            var parts = paramStr.Split(',');
            if (parts.Length >= 2)
            {
                _ = double.TryParse(parts[0], out trueOpacity);
                _ = double.TryParse(parts[1], out falseOpacity);
            }
        }

        return value is true ? trueOpacity : falseOpacity;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
