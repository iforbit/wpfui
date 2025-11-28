// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Markup;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Markup;

/// <summary>
/// Custom <see cref="MarkupExtension"/> which can provide <see cref="SegoeFluentIcon"/>.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;ui:Button
///     Appearance="Primary"
///     Content="WPF UI button with Segoe Fluent icon"
///     Icon="{ui:SegoeFluentIcon Symbol=Home}" /&gt;
/// </code>
/// <code lang="xml">
/// &lt;ui:Button Icon="{ui:SegoeFluentIcon Home}" /&gt;
/// </code>
/// <code lang="xml">
/// &lt;ui:HyperlinkButton Icon="{ui:SegoeFluentIcon Home}" /&gt;
/// </code>
/// <code lang="xml">
/// &lt;ui:TitleBar Icon="{ui:SegoeFluentIcon Home}" /&gt;
/// </code>
/// </example>
[ContentProperty(nameof(Symbol))]
[MarkupExtensionReturnType(typeof(SegoeFluentIcon))]
public class SegoeFluentIconExtension : FontIconExtensionBase<SegoeFluentSymbol, SegoeFluentIcon>
{
    public SegoeFluentIconExtension() { }

    public SegoeFluentIconExtension(SegoeFluentSymbol symbol)
        : base(symbol)
    {
    }

    public SegoeFluentIconExtension(string symbol)
        : base(symbol)
    {
    }
}
