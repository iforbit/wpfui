// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Tray;

/// <summary>
/// Represents a contract with a service that provides methods for displaying the icon and menu in the tray area.
/// </summary>
public interface INotifyIconService
{
    /// <summary>
    /// Occurs when the user clicks the left mouse button on the tray icon.
    /// </summary>
    event NotifyIconEventHandler? LeftClick;

    /// <summary>
    /// Occurs when the user double-clicks the left mouse button on the tray icon.
    /// </summary>
    event NotifyIconEventHandler? LeftDoubleClick;

    /// <summary>
    /// Occurs when the user clicks the right mouse button on the tray icon.
    /// </summary>
    event NotifyIconEventHandler? RightClick;

    /// <summary>
    /// Gets the notify icon id.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets a value indicating whether the notify icon is registered in the tray.
    /// </summary>
    public bool IsRegistered { get; }

    /// <summary>
    /// Gets or sets the ToolTip text displayed when the mouse pointer rests on a notification area icon.
    /// </summary>
    public string TooltipText { get; set; }

    /// <summary>
    /// Gets or sets the context menu displayed after clicking the icon.
    /// </summary>
    ContextMenu? ContextMenu { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="System.Windows.Media.Imaging.BitmapFrame"/> of the tray icon.
    /// </summary>
    public ImageSource? Icon { get; set; }

    /// <summary>
    /// Tries to register the Notify Icon in the shell.
    /// </summary>
    public bool Register();

    /// <summary>
    /// Tries to unregister the Notify Icon from the shell.
    /// </summary>
    public bool Unregister();

    /// <summary>
    /// Sets parent window of the tray icon.
    /// </summary>
    public void SetParentWindow(Window window);

    /// <summary>
    /// Shows a balloon tip notification.
    /// </summary>
    /// <param name="title">The title of the balloon tip (max 64 characters).</param>
    /// <param name="message">The message of the balloon tip (max 256 characters).</param>
    /// <param name="icon">The icon to display.</param>
    public void ShowBalloonTip(string title, string message, BalloonTipIcon icon = BalloonTipIcon.Info);

    /// <summary>
    /// Hides the currently displayed balloon tip.
    /// </summary>
    public void HideBalloonTip();
}
