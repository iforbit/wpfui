// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

/// <summary>
/// Repesents scalable ribbon contol
/// </summary>
public interface IScalableRibbonControl
{
    /// <summary>
    /// Resets the scale.
    /// </summary>
    void ResetScale();

    /// <summary>
    /// Enlarge control size.
    /// </summary>
    void Enlarge();

    /// <summary>
    /// Reduce control size.
    /// </summary>
    void Reduce();

    /// <summary>
    /// Occurs when contol is scaled.
    /// </summary>
    event EventHandler Scaled;
}