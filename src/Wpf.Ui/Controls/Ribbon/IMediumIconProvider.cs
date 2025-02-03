// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.Controls.Ribbon.Helpers;

namespace Wpf.Ui.Controls.Ribbon;


/// <summary>
/// Inferface for controls which provide a medium icon.
/// </summary>
public interface IMediumIconProvider
{
    /// <summary>
    /// Gets or sets the medium icon.
    /// </summary>
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Localizable(false)]
    object? MediumIcon { get; set; }
}

/// <summary>
/// Provides some <see cref="DependencyProperty"/> for <see cref="IMediumIconProvider"/>.
/// </summary>
public class MediumIconProviderProperties : DependencyObject
{
    private MediumIconProviderProperties()
    {
    }

    /// <summary>
    /// <see cref="DependencyProperty"/> for <see cref="IMediumIconProvider.MediumIcon"/>.
    /// </summary>
    public static readonly DependencyProperty MediumIconProperty = DependencyProperty.Register(nameof(IMediumIconProvider.MediumIcon), typeof(object), typeof(MediumIconProviderProperties), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));
}