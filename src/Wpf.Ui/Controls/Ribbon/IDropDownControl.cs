// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls.Primitives;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents control that have drop down popup
/// </summary>
public interface IDropDownControl
{
    /// <summary>
    /// Gets drop down popup
    /// </summary>
    Popup? DropDownPopup { get; }

    /// <summary>
    /// Gets or sets a value indicating whether control context menu is opened
    /// </summary>
    bool IsContextMenuOpened { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether drop down is opened
    /// </summary>
    bool IsDropDownOpen { get; set; }

    /// <summary>
    /// Occurs when drop down is opened.
    /// </summary>
    event EventHandler DropDownOpened;

    /// <summary>
    /// Occurs when drop down menu is closed.
    /// </summary>
    event EventHandler DropDownClosed;
}