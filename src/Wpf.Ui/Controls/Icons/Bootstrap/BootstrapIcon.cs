// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.
//
// Bootstrap Icons
// Copyright (c) 2019-2024 The Bootstrap Authors
// Licensed under the MIT License
// https://github.com/twbs/icons

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// Represents a text element containing a Bootstrap icon glyph.
/// </summary>
public class BootstrapIcon : FontIconBase<BootstrapSymbolRegular>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BootstrapIcon"/> class.
    /// </summary>
    public BootstrapIcon() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BootstrapIcon"/> class with a symbol and font size.
    /// </summary>
    /// <param name="symbol">The Bootstrap symbol to display.</param>
    /// <param name="fontSize">The font size.</param>
    public BootstrapIcon(BootstrapSymbolRegular symbol, double fontSize = 14)
        : base(symbol, fontSize)
    {
    }

    /// <inheritdoc />
    protected override void SetFontReference()
    {
        SetResourceReference(FontFamilyProperty, "BootstrapIcons");
    }
}
