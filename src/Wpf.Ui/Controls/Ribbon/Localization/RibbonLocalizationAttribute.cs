// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls.Localization;

/// <summary>
/// Attribute class providing informations about a localization
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class RibbonLocalizationAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonLocalizationAttribute"/> class.
    /// Creates a new instance.
    /// </summary>
    /// <param name="displayName">Specifies the display name.</param>
    /// <param name="cultureName">Specifies the culture name.</param>
    public RibbonLocalizationAttribute(string displayName, string cultureName)
    {
        this.DisplayName = displayName;
        this.CultureName = cultureName;
    }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the culture name.
    /// </summary>
    public string CultureName { get; }
}