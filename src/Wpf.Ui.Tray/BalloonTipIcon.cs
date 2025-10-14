// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Tray;

/// <summary>
/// Defines a set of standardized icons that can be associated with a Balloon Tip.
/// </summary>
public enum BalloonTipIcon
{
    /// <summary>
    /// Not a standard icon.
    /// </summary>
    None = 0,

    /// <summary>
    /// An information icon.
    /// </summary>
    Info = 1,

    /// <summary>
    /// A warning icon.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// An error icon.
    /// </summary>
    Error = 3
}
