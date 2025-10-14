// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Tray;

/// <summary>
/// Represents flags used when showing a standard balloon tip.
/// More information: https://docs.microsoft.com/en-us/windows/win32/api/shellapi/ns-shellapi-notifyicondataa
/// </summary>
[Flags]
internal enum BalloonTipSetting
{
    /// <summary>
    /// Do not display an icon.
    /// </summary>
    None = 0x00,

    /// <summary>
    /// Display an information icon.
    /// </summary>
    Info = 0x01,

    /// <summary>
    /// Display a warning icon.
    /// </summary>
    Warning = 0x02,

    /// <summary>
    /// Display an error icon.
    /// </summary>
    Error = 0x03,

    /// <summary>
    /// Use a custom icon provided by the application.
    /// </summary>
    User = 0x04,

    /// <summary>
    /// Do not play the associated sound. Applies only to balloon ToolTips.
    /// Windows XP (SHELL32.DLL version 6.0) and later.
    /// </summary>
    NoSound = 0x10,

    /// <summary>
    /// Use the large version of the icon.
    /// Windows Vista (SHELL32.DLL version 6.0.6) and later.
    /// </summary>
    LargeIcon = 0x20,

    /// <summary>
    /// Respect Windows quiet time settings.
    /// Windows 7 and later.
    /// </summary>
    RespectQuietTime = 0x80
}
