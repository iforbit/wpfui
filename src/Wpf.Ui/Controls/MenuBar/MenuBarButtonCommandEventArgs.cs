// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

/// <summary>
/// Event arguments for MenuBar button command events.
/// </summary>
public class MenuBarButtonCommandEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Gets the type of button that was clicked.
    /// </summary>
    public TitleBarButtonType ButtonType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuBarButtonCommandEventArgs"/> class.
    /// </summary>
    public MenuBarButtonCommandEventArgs(RoutedEvent routedEvent, object source, TitleBarButtonType buttonType)
        : base(routedEvent, source)
    {
        ButtonType = buttonType;
    }
}
