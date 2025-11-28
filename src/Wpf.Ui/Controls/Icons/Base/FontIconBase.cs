// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// Base class for font-based icons with a specific symbol enum type.
/// Eliminates code duplication across different icon font families (Bootstrap, Material, Segoe, etc.).
/// </summary>
/// <typeparam name="TEnum">The enum type representing the icon symbols.</typeparam>
public abstract class FontIconBase<TEnum> : FontIcon
    where TEnum : struct, Enum
{
    /// <summary>Identifies the <see cref="Symbol"/> dependency property.</summary>
    public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(
        nameof(Symbol),
        typeof(TEnum),
        typeof(FontIconBase<TEnum>),
        new PropertyMetadata(default(TEnum), OnSymbolChanged)
    );

    /// <summary>
    /// Gets or sets the symbol to display.
    /// </summary>
    public TEnum Symbol
    {
        get => (TEnum)GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontIconBase{TEnum}"/> class.
    /// </summary>
    protected FontIconBase() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontIconBase{TEnum}"/> class with a symbol and font size.
    /// </summary>
    /// <param name="symbol">The symbol to display.</param>
    /// <param name="fontSize">The font size.</param>
    protected FontIconBase(TEnum symbol, double fontSize = 14)
    {
        Symbol = symbol;
        FontSize = fontSize;
    }

    /// <inheritdoc />
    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        SetFontReference();
    }

    /// <summary>
    /// Called when the Symbol property changes. Updates the Glyph property.
    /// </summary>
    private static void OnSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FontIconBase<TEnum> self)
        {
            // Convert enum to Unicode string
            int symbolValue = Convert.ToInt32(self.Symbol);
            string glyphString = System.Text.Encoding.Unicode.GetString(BitConverter.GetBytes(symbolValue)).TrimEnd('\0');
            self.SetCurrentValue(GlyphProperty, glyphString);
        }
    }

    /// <summary>
    /// Sets the FontFamily resource reference for this icon.
    /// Derived classes must implement this to specify their font resource key.
    /// </summary>
    protected abstract void SetFontReference();
}
