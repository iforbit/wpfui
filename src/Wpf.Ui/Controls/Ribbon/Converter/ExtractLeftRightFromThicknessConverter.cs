// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Data;

namespace Wpf.Ui.Controls.Converter;

/// <summary>
/// Used to convert the left and right values from an input <see cref="Thickness"/> to an output <see cref="Thickness"/>.
/// </summary>
public class ExtractLeftRightFromThicknessConverter : IValueConverter
{
    /// <summary>
    /// The default converter instance.
    /// </summary>
    public static readonly ExtractLeftRightFromThicknessConverter Default = new();

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var input = (Thickness)value;
        return new Thickness(input.Left, 0, input.Right, 0);
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
