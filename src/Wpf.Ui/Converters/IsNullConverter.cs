// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Data;
using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Converters;

/// <summary>
///     Converts <c>null</c> to <c>true</c> and not <c>null</c> to <c>false</c>.
/// </summary>
public sealed class IsNullConverter : IValueConverter
{
    /// <summary>
    ///     A singleton instance for <see cref="IsNullConverter" />.
    /// </summary>
    public static readonly IsNullConverter Instance = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return BooleanBoxes.Box(value is null);
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}