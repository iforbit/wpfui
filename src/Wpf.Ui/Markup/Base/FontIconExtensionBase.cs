// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Markup;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Markup;

/// <summary>
/// Base class for markup extensions that provide font-based icons with a specific symbol enum type.
/// Eliminates code duplication across different icon font families (Bootstrap, Material, Segoe, etc.).
/// </summary>
/// <typeparam name="TEnum">The enum type representing the icon symbols.</typeparam>
/// <typeparam name="TIcon">The icon type that will be created.</typeparam>
[MarkupExtensionReturnType(typeof(IconElement))]
public abstract class FontIconExtensionBase<TEnum, TIcon> : MarkupExtension
    where TEnum : struct, Enum
    where TIcon : FontIconBase<TEnum>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FontIconExtensionBase{TEnum, TIcon}"/> class.
    /// </summary>
    protected FontIconExtensionBase() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontIconExtensionBase{TEnum, TIcon}"/> class with a symbol.
    /// </summary>
    /// <param name="symbol">The symbol to display.</param>
    protected FontIconExtensionBase(TEnum symbol)
    {
        Symbol = symbol;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontIconExtensionBase{TEnum, TIcon}"/> class with a symbol name.
    /// </summary>
    /// <param name="symbol">The symbol name to parse.</param>
    protected FontIconExtensionBase(string symbol)
    {
        Symbol = (TEnum)Enum.Parse(typeof(TEnum), symbol);
    }

    /// <summary>
    /// Gets or sets the symbol to display.
    /// </summary>
    [ConstructorArgument("symbol")]
    public TEnum Symbol { get; set; }

    /// <summary>
    /// Gets or sets the font size.
    /// </summary>
    public double FontSize { get; set; }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        TIcon icon = new() { Symbol = Symbol };

        if (FontSize > 0)
        {
            icon.FontSize = FontSize;
        }

        return icon;
    }
}
