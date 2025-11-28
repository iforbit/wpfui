// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.Appearance;

namespace Wpf.Ui.Gallery.Helpers;

internal sealed class ThemeToIndexConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ApplicationTheme.Dark)
        {
            return 1;
        }

        if (value is ApplicationTheme.HighContrast)
        {
            return 2;
        }

        if (value is ApplicationTheme.System)
        {
            return 3;
        }

        return 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is 1)
        {
            return ApplicationTheme.Dark;
        }

        if (value is 2)
        {
            return ApplicationTheme.HighContrast;
        }

        if (value is 3)
        {
            return ApplicationTheme.System;
        }

        return ApplicationTheme.Light;
    }
}
