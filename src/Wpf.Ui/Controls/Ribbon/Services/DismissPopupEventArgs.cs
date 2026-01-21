// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

/// <summary>
/// Dismiss popup arguments.
/// </summary>
public class DismissPopupEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DismissPopupEventArgs"/> class.
    /// Standard constructor.
    /// </summary>
    public DismissPopupEventArgs()
        : this(DismissPopupMode.Always)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DismissPopupEventArgs"/> class.
    /// Constructor.
    /// </summary>
    /// <param name="dismissMode">Dismiss mode.</param>
    public DismissPopupEventArgs(DismissPopupMode dismissMode)
        : this(dismissMode, DismissPopupReason.Undefined)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DismissPopupEventArgs"/> class.
    /// Constructor.
    /// </summary>
    /// <param name="dismissMode">Dismiss mode.</param>
    /// <param name="reason">Dismiss reason.</param>
    public DismissPopupEventArgs(DismissPopupMode dismissMode, DismissPopupReason reason)
    {
        this.RoutedEvent = PopupService.DismissPopupEvent;
        this.DismissMode = dismissMode;
        this.DismissReason = reason;
    }

    /// <summary>
    /// Gets popup dismiss mode.
    /// </summary>
    public DismissPopupMode DismissMode { get; }

    /// <summary>
    /// Gets or sets popup dismiss reason.
    /// </summary>
    public DismissPopupReason DismissReason { get; set; }

    /// <inheritdoc />
    protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
    {
        var handler = (EventHandler<DismissPopupEventArgs>)genericHandler;
        handler(genericTarget, this);
    }
}
