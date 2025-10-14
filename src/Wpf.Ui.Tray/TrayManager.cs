// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Linq;
using System.Windows;

namespace Wpf.Ui.Tray;

/// <summary>
/// Responsible for managing the icons in the Tray bar.
/// Fixed: Ghost icon issue when app is forcefully closed
/// Fixed: Thread safety for Register/Unregister operations
/// </summary>
internal static class TrayManager
{
    private static readonly object _lock = new object();
    private static bool _shutdownHandlersRegistered = false;

    private static void EnsureShutdownHandlers()
    {
        lock (_lock)
        {
            if (_shutdownHandlersRegistered)
            {
                return;
            }

            _shutdownHandlersRegistered = true;

            // Handle application shutdown to clean up all tray icons
            if (Application.Current != null)
            {
                Application.Current.Exit += OnApplicationExit;
                Application.Current.SessionEnding += OnSessionEnding;
            }
        }
    }

    private static void OnApplicationExit(object? sender, ExitEventArgs e)
    {
        CleanupAllIcons();
    }

    private static void OnSessionEnding(object? sender, SessionEndingCancelEventArgs e)
    {
        CleanupAllIcons();
    }

    private static void CleanupAllIcons()
    {
        lock (_lock)
        {
            foreach (INotifyIcon? notifyIcon in TrayData.NotifyIcons.ToList())
            {
                try
                {
                    _ = UnregisterInternal(notifyIcon);
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }

            TrayData.NotifyIcons.Clear();
        }
    }

    public static bool Register(INotifyIcon notifyIcon)
    {
        if (notifyIcon is null)
        {
            return false;
        }

        EnsureShutdownHandlers();

        return Register(notifyIcon, GetParentSource());
    }

    public static bool Register(INotifyIcon notifyIcon, Window parentWindow)
    {
        if (parentWindow == null)
        {
            return false;
        }

        EnsureShutdownHandlers();

        return Register(notifyIcon, (HwndSource)PresentationSource.FromVisual(parentWindow));
    }

    public static bool Register(INotifyIcon notifyIcon, HwndSource? parentSource)
    {
        lock (_lock)
        {
            if (parentSource is null)
            {
                if (!notifyIcon.IsRegistered)
                {
                    return false;
                }

                _ = UnregisterInternal(notifyIcon);

                return false;
            }

            if (parentSource.Handle == IntPtr.Zero)
            {
                return false;
            }

            if (notifyIcon.IsRegistered)
            {
                _ = UnregisterInternal(notifyIcon);
            }

            notifyIcon.Id = TrayData.NotifyIcons.Count + 1;

            notifyIcon.HookWindow = new TrayHandler(
                $"wpfui_th_{parentSource.Handle}_{notifyIcon.Id}",
                parentSource.Handle
            )
            {
                ElementId = notifyIcon.Id,
            };

            notifyIcon.ShellIconData = new Interop.Shell32.NOTIFYICONDATA
            {
                uID = notifyIcon.Id,
                uFlags = Interop.Shell32.NIF.MESSAGE | Interop.Shell32.NIF.SHOWTIP,
                uCallbackMessage = (int)Interop.User32.WM.TRAYMOUSEMESSAGE,
                hWnd = notifyIcon.HookWindow.Handle,
                dwState = 0x2,
            };

            if (!string.IsNullOrEmpty(notifyIcon.TooltipText))
            {
                notifyIcon.ShellIconData.szTip = notifyIcon.TooltipText;
                notifyIcon.ShellIconData.uFlags |= Interop.Shell32.NIF.TIP;
            }

            ReloadHicon(notifyIcon);

            notifyIcon.HookWindow.AddHook(notifyIcon.WndProc);

            _ = Interop.Shell32.Shell_NotifyIcon(Interop.Shell32.NIM.ADD, notifyIcon.ShellIconData);

            // Set NOTIFYICON_VERSION_4 to ensure the icon is always visible and not hidden by default
            notifyIcon.ShellIconData.uVersion = 4; // NOTIFYICON_VERSION_4
            _ = Interop.Shell32.Shell_NotifyIcon(Interop.Shell32.NIM.SETVERSION, notifyIcon.ShellIconData);

            TrayData.NotifyIcons.Add(notifyIcon);

            notifyIcon.IsRegistered = true;

            return true;
        }
    }

    public static bool ModifyIcon(INotifyIcon notifyIcon)
    {
        lock (_lock)
        {
            if (!notifyIcon.IsRegistered)
            {
                return true;
            }

            ReloadHicon(notifyIcon);

            return Interop.Shell32.Shell_NotifyIcon(Interop.Shell32.NIM.MODIFY, notifyIcon.ShellIconData);
        }
    }

    public static bool ModifyToolTip(INotifyIcon notifyIcon)
    {
        lock (_lock)
        {
            if (!notifyIcon.IsRegistered)
            {
                return true;
            }

            notifyIcon.ShellIconData.szTip = notifyIcon.TooltipText;
            notifyIcon.ShellIconData.uFlags |= Interop.Shell32.NIF.TIP;

            return Interop.Shell32.Shell_NotifyIcon(Interop.Shell32.NIM.MODIFY, notifyIcon.ShellIconData);
        }
    }

    /// <summary>
    /// Tries to remove the <see cref="INotifyIcon"/> from the shell.
    /// </summary>
    public static bool Unregister(INotifyIcon notifyIcon)
    {
        lock (_lock)
        {
            return UnregisterInternal(notifyIcon);
        }
    }

    /// <summary>
    /// Internal unregister without lock (called from within locked context).
    /// </summary>
    private static bool UnregisterInternal(INotifyIcon notifyIcon)
    {
        if (notifyIcon.ShellIconData == null || !notifyIcon.IsRegistered)
        {
            return false;
        }

        _ = Interop.Shell32.Shell_NotifyIcon(Interop.Shell32.NIM.DELETE, notifyIcon.ShellIconData);

        notifyIcon.IsRegistered = false;

        _ = TrayData.NotifyIcons.Remove(notifyIcon);

        return true;
    }

    /// <summary>
    /// Gets application source.
    /// </summary>
    private static HwndSource? GetParentSource()
    {
        Window mainWindow = Application.Current.MainWindow;

        if (mainWindow == null)
        {
            return null;
        }

        return (HwndSource)PresentationSource.FromVisual(mainWindow);
    }

    private static void ReloadHicon(INotifyIcon notifyIcon)
    {
        IntPtr hIcon = IntPtr.Zero;

        if (notifyIcon.Icon is not null)
        {
            hIcon = Hicon.FromSource(notifyIcon.Icon);
        }

        if (hIcon == IntPtr.Zero)
        {
            hIcon = Hicon.FromApp();
        }

        if (hIcon != IntPtr.Zero)
        {
            notifyIcon.ShellIconData.hIcon = hIcon;
            notifyIcon.ShellIconData.uFlags |= Interop.Shell32.NIF.ICON;
        }
    }
}
