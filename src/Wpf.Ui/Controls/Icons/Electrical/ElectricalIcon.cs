// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// Represents a text element containing an electrical symbol glyph.
/// </summary>
public class ElectricalIcon : FontIconBase<ElectricalSymbolRegular>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElectricalIcon"/> class.
    /// </summary>
    public ElectricalIcon() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElectricalIcon"/> class with a symbol and font size.
    /// </summary>
    /// <param name="symbol">The electrical symbol to display.</param>
    /// <param name="fontSize">The font size.</param>
    public ElectricalIcon(ElectricalSymbolRegular symbol, double fontSize = 24)
        : base(symbol, fontSize)
    {
    }

    /// <inheritdoc />
    protected override void SetFontReference()
    {
        SetResourceReference(FontFamilyProperty, "ElectricalIcons");
    }
}
