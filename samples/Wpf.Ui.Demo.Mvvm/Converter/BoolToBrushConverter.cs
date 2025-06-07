// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Data;
using System.Windows.Media;

using Wpf.Ui.Controls;

namespace Wpf.Ui.Demo.Mvvm.Converter;
public class BoolToBrushConverter : IValueConverter
{
    public Brush TrueBrush { get; set; } = Brushes.LimeGreen;
    public Brush FalseBrush { get; set; } = Brushes.Gray;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return boolValue ? TrueBrush : FalseBrush;

        return FalseBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class BoolToSymbolConverter : IValueConverter
{
    public SymbolRegular TrueSymbol { get; set; } = SymbolRegular.PlugConnectedCheckmark20;
    public SymbolRegular FalseSymbol { get; set; } = SymbolRegular.PlugConnected20;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? TrueSymbol : FalseSymbol;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}