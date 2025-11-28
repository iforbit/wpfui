// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// Represents a text element containing a Material Design icon glyph.
/// </summary>
public class MaterialIcon : FontIconBase<MaterialSymbolRegular>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialIcon"/> class.
    /// </summary>
    public MaterialIcon() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialIcon"/> class with a symbol and font size.
    /// </summary>
    /// <param name="symbol">The Material Design symbol to display.</param>
    /// <param name="fontSize">The font size.</param>
    public MaterialIcon(MaterialSymbolRegular symbol, double fontSize = 14)
        : base(symbol, fontSize)
    {
    }

    /// <inheritdoc />
    protected override void SetFontReference()
    {
        SetResourceReference(FontFamilyProperty, "MaterialIcons-Regular");
    }
}
