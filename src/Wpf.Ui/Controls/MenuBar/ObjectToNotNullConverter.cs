// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Lighted Solutions and Contributors.

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Wpf.Ui.Controls;

/// <summary>
/// Converts an object to a boolean indicating whether it is not null.
/// </summary>
[ValueConversion(typeof(object), typeof(bool))]
public class ObjectToNotNullConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static readonly ObjectToNotNullConverter Instance = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return Instance;
    }
}
