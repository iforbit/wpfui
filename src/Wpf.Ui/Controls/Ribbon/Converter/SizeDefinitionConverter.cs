// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls.Ribbon.Converter;


/// <summary>
/// Class which enables conversion from <see cref="string"/> to <see cref="RibbonControlSizeDefinition"/>
/// </summary>
public class SizeDefinitionConverter : TypeConverter
{
    /// <inheritdoc />
    public override bool CanConvertFrom( ITypeDescriptorContext? context, Type sourceType )
    {
        return sourceType.IsAssignableFrom(typeof(string));
    }

    /// <inheritdoc />
    public override object ConvertFrom( ITypeDescriptorContext? context, CultureInfo? culture, object value )
    {
        return new RibbonControlSizeDefinition(value as string);
    }
}