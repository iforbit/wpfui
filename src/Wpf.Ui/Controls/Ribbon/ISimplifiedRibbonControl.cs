// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls.Ribbon;

/// <summary>
/// Base interface for controls supports simplified state
/// </summary>
public interface ISimplifiedRibbonControl : ISimplifiedStateControl
{
    /// <summary>
    /// Gets or sets SimplifiedSizeDefinition for element on Simplified mode
    /// </summary>
    RibbonControlSizeDefinition SimplifiedSizeDefinition { get; set; }

    /// <summary>
    /// Gets a value indicating whether gets or sets whether or not the ribbon is in Simplified mode
    /// </summary>
    bool IsSimplified { get; }
}