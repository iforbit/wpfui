// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// Base class for icon sources that use a font-based icon with a specific symbol enum type.
/// Eliminates code duplication across different icon font families (Bootstrap, Material, Segoe, etc.).
/// </summary>
/// <typeparam name="TEnum">The enum type representing the icon symbols.</typeparam>
/// <typeparam name="TIcon">The icon type that will be created.</typeparam>
public abstract class FontIconSourceBase<TEnum, TIcon> : IconSource
    where TEnum : struct, Enum
    where TIcon : FontIconBase<TEnum>, new()
{
    /// <summary>Identifies the <see cref="FontSize"/> dependency property.</summary>
    public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
        nameof(FontSize),
        typeof(double),
        typeof(FontIconSourceBase<TEnum, TIcon>),
        new PropertyMetadata(SystemFonts.MessageFontSize)
    );

    /// <summary>Identifies the <see cref="FontStyle"/> dependency property.</summary>
    public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register(
        nameof(FontStyle),
        typeof(FontStyle),
        typeof(FontIconSourceBase<TEnum, TIcon>),
        new PropertyMetadata(FontStyles.Normal)
    );

    /// <summary>Identifies the <see cref="FontWeight"/> dependency property.</summary>
    public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register(
        nameof(FontWeight),
        typeof(FontWeight),
        typeof(FontIconSourceBase<TEnum, TIcon>),
        new PropertyMetadata(FontWeights.Normal)
    );

    /// <summary>Identifies the <see cref="Symbol"/> dependency property.</summary>
    public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(
        nameof(Symbol),
        typeof(TEnum),
        typeof(FontIconSourceBase<TEnum, TIcon>),
        new PropertyMetadata(default(TEnum))
    );

    /// <inheritdoc cref="Control.FontSize"/>
    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <inheritdoc cref="Control.FontWeight"/>
    public FontWeight FontWeight
    {
        get => (FontWeight)GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    /// <inheritdoc cref="Control.FontStyle"/>
    public FontStyle FontStyle
    {
        get => (FontStyle)GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the symbol to display.
    /// </summary>
    public TEnum Symbol
    {
        get => (TEnum)GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    /// <inheritdoc />
    public override IconElement CreateIconElement()
    {
        TIcon icon = new() { Symbol = Symbol, FontSize = FontSize };

        if (!FontSize.Equals(SystemFonts.MessageFontSize))
        {
            icon.FontSize = FontSize;
        }

        if (FontWeight != FontWeights.Normal)
        {
            icon.FontWeight = FontWeight;
        }

        if (FontStyle != FontStyles.Normal)
        {
            icon.FontStyle = FontStyle;
        }

        if (Foreground != SystemColors.ControlTextBrush)
        {
            icon.Foreground = Foreground;
        }

        return icon;
    }
}
