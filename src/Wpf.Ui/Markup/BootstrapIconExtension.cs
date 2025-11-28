// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.
//
// Bootstrap Icons
// Copyright (c) 2019-2024 The Bootstrap Authors
// Licensed under the MIT License
// https://github.com/twbs/icons

using System.Windows.Markup;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Markup;

/// <summary>
/// Custom <see cref="MarkupExtension"/> which can provide <see cref="BootstrapIcon"/>.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;ui:Button
///     Appearance="Primary"
///     Content="WPF UI button with Bootstrap icon"
///     Icon="{ui:BootstrapIcon Symbol=House}" /&gt;
/// </code>
/// <code lang="xml">
/// &lt;ui:Button Icon="{ui:BootstrapIcon House}" /&gt;
/// </code>
/// <code lang="xml">
/// &lt;ui:HyperlinkButton Icon="{ui:BootstrapIcon House}" /&gt;
/// </code>
/// <code lang="xml">
/// &lt;ui:TitleBar Icon="{ui:BootstrapIcon House}" /&gt;
/// </code>
/// </example>
[ContentProperty(nameof(Symbol))]
[MarkupExtensionReturnType(typeof(BootstrapIcon))]
public class BootstrapIconExtension : FontIconExtensionBase<BootstrapSymbolRegular, BootstrapIcon>
{
    public BootstrapIconExtension() { }

    public BootstrapIconExtension(BootstrapSymbolRegular symbol)
        : base(symbol)
    {
    }

    public BootstrapIconExtension(string symbol)
        : base(symbol)
    {
    }
}
