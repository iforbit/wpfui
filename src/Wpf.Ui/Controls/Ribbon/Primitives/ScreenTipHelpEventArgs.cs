// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

/// <summary>
/// Event args for HelpPressed event handler
/// </summary>
public class ScreenTipHelpEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenTipHelpEventArgs"/> class.
    /// Constructor
    /// </summary>
    /// <param name="helpTopic">Help topic</param>
    public ScreenTipHelpEventArgs(object? helpTopic)
    {
        this.HelpTopic = helpTopic;
    }

    /// <summary>
    /// Gets help topic associated with screen tip
    /// </summary>
    public object? HelpTopic { get; }
}
