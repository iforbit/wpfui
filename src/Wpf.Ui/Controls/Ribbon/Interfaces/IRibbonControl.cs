// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

/// <summary>
/// Base interface for Fluent controls
/// </summary>
public interface IRibbonControl : IHeaderedControl, ILogicalChildSupport
{
    /// <summary>
    /// Gets or sets Size for the element
    /// </summary>
    RibbonControlSize Size { get; set; }

    /// <summary>
    /// Gets or sets SizeDefinition for element
    /// </summary>
    RibbonControlSizeDefinition SizeDefinition { get; set; }

    /// <summary>
    /// Gets or sets SimplifiedSizeDefinition for element on Simplified mode
    /// </summary>
    RibbonControlSizeDefinition SimplifiedSizeDefinition { get; set; }

    /// <summary>
    /// Gets or sets Icon for the element
    /// </summary>
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Localizable(false)]
    object? Icon { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this control should be shown in Simplified mode
    /// </summary>
    bool ShowInSimplified { get; set; }

    /// <summary>
    /// Gets a value indicating whether the ribbon is currently in Simplified mode
    /// </summary>
    bool IsSimplified { get; }
}