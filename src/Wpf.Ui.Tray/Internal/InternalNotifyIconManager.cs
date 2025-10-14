// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace Wpf.Ui.Tray.Internal;

/// <summary>
/// Internal service for Notify Icon management.
/// </summary>
internal class InternalNotifyIconManager : IDisposable, INotifyIcon
{
    /// <summary>
    /// Whether the control is disposed.
    /// </summary>
    private bool _disposed;

    /// <inheritdoc />
    public int Id { get; set; } = -1;

    /// <inheritdoc />
    public bool IsRegistered { get; set; }

    /// <inheritdoc />
    public string TooltipText { get; set; } = string.Empty;

    /// <inheritdoc />
    public ImageSource? Icon { get; set; } = default!;

    /// <inheritdoc />
    public HwndSource HookWindow { get; set; } = default!;

    /// <inheritdoc />
    public ContextMenu? ContextMenu { get; set; } = default!;

    /// <inheritdoc />
    public bool FocusOnLeftClick { get; set; } = true;

    /// <inheritdoc />
    public bool MenuOnRightClick { get; set; } = true;

    public event NotifyIconEventHandler? LeftClick;

    public event NotifyIconEventHandler? LeftDoubleClick;

    public event NotifyIconEventHandler? RightClick;

    public event NotifyIconEventHandler? RightDoubleClick;

    public event NotifyIconEventHandler? MiddleClick;

    public event NotifyIconEventHandler? MiddleDoubleClick;

    /// <summary>
    /// Gets or sets a set of information for Shell32 to manipulate the icon.
    /// </summary>
    public Interop.Shell32.NOTIFYICONDATA ShellIconData { get; set; } = default!;

    public InternalNotifyIconManager()
    {
        ApplicationThemeManager.Changed += OnThemeChanged;
    }

    ~InternalNotifyIconManager()
    {
        Dispose(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public virtual bool Register()
    {
        IsRegistered = TrayManager.Register(this);

        return IsRegistered;
    }

    /// <inheritdoc />
    public virtual bool Register(Window parentWindow)
    {
        IsRegistered = TrayManager.Register(this, parentWindow);

        return IsRegistered;
    }

    /// <inheritdoc />
    public virtual bool ModifyIcon()
    {
        return TrayManager.ModifyIcon(this);
    }

    /// <inheritdoc />
    public virtual bool ModifyToolTip()
    {
        return TrayManager.ModifyToolTip(this);
    }

    /// <inheritdoc />
    public virtual bool Unregister()
    {
        return TrayManager.Unregister(this);
    }

    /// <summary>
    /// Occurs when the application theme is changing.
    /// </summary>
    protected virtual void OnThemeChanged(ApplicationTheme currentApplicationTheme, Color systemAccent)
    {
        ContextMenu?.UpdateDefaultStyle();
        ContextMenu?.UpdateLayout();
    }

    /// <summary>
    /// Focus the application main window.
    /// </summary>
    protected virtual void FocusApp()
    {
        // Debug.WriteLine(
        //     $"INFO | {typeof(TrayHandler)} invoked {nameof(FocusApp)} method.",
        //     "Wpf.Ui.NotifyIcon"
        // );

        Window? mainWindow = Application.Current.MainWindow;

        if (mainWindow == null)
        {
            return;
        }

        // Restore window if minimized or hidden
        if (mainWindow.WindowState == WindowState.Minimized)
        {
            mainWindow.WindowState = WindowState.Normal;
        }

        // Show window if hidden
        if (!mainWindow.IsVisible)
        {
            mainWindow.Show();
        }

        // Activate window to bring it to front
        _ = mainWindow.Activate();

        if (mainWindow.Topmost)
        {
            mainWindow.Topmost = false;
            mainWindow.Topmost = true;
        }
        else
        {
            mainWindow.Topmost = true;
            mainWindow.Topmost = false;
        }

        _ = mainWindow.Focus();
    }

    /// <summary>
    /// Shows the menu if it has been added.
    /// </summary>
    protected virtual void OpenMenu()
    {
        // Debug.WriteLine(
        //     $"INFO | {typeof(TrayHandler)} invoked {nameof(OpenMenu)} method.",
        //     "Wpf.Ui.NotifyIcon"
        // );

        if (ContextMenu is null)
        {
            return;
        }

        // Without setting the handler window at the front, menu may appear behind the taskbar
        _ = Interop.User32.SetForegroundWindow(HookWindow.Handle);
        ContextMenuService.SetPlacement(ContextMenu, PlacementMode.MousePoint);

        // ContextMenu.ApplyMica();
        ContextMenu.SetCurrentValue(ContextMenu.IsOpenProperty, true);
    }

    /// <summary>
    /// This virtual method is called when tray icon is left-clicked and it raises the left click <see langword="event"/>.
    /// </summary>
    protected virtual void OnLeftClick()
    {
        LeftClick?.Invoke();
    }

    /// <summary>
    /// This virtual method is called when tray icon is left-clicked and it raises the left double click <see langword="event"/>.
    /// </summary>
    protected virtual void OnLeftDoubleClick()
    {
        LeftDoubleClick?.Invoke();
    }

    /// <summary>
    /// This virtual method is called when tray icon is left-clicked and it raises the right click <see langword="event"/>.
    /// </summary>
    protected virtual void OnRightClick()
    {
        RightClick?.Invoke();
    }

    /// <summary>
    /// This virtual method is called when tray icon is left-clicked and it raises the right double click <see langword="event"/>.
    /// </summary>
    protected virtual void OnRightDoubleClick()
    {
        RightDoubleClick?.Invoke();
    }

    /// <summary>
    /// This virtual method is called when tray icon is left-clicked and it raises the middle click <see langword="event"/>.
    /// </summary>
    protected virtual void OnMiddleClick()
    {
        MiddleClick?.Invoke();
    }

    /// <summary>
    /// This virtual method is called when tray icon is left-clicked and it raises the middle double click <see langword="event"/>.
    /// </summary>
    protected virtual void OnMiddleDoubleClick()
    {
        MiddleDoubleClick?.Invoke();
    }

    /// <summary>
    /// If disposing equals <see langword="true"/>, the method has been called directly or indirectly
    /// by a user's code. Managed and unmanaged resources can be disposed. If disposing equals <see langword="false"/>,
    /// the method has been called by the runtime from inside the finalizer and you should not
    /// reference other objects.
    /// <para>Only unmanaged resources can be disposed.</para>
    /// </summary>
    /// <param name="disposing">If disposing equals <see langword="true"/>, dispose all managed and unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (!disposing)
        {
            return;
        }

        System.Diagnostics.Debug.WriteLine(
            $"INFO | {typeof(NotifyIconService)} disposed.",
            "Wpf.Ui.NotifyIcon"
        );

        // ContextMenu가 열려있으면 닫기
        if (ContextMenu?.IsOpen == true)
        {
            ContextMenu.SetCurrentValue(ContextMenu.IsOpenProperty, false);
        }

        // BalloonTip 숨기기
        HideBalloonTip();

        // ApplicationThemeManager 이벤트 구독 해제
        ApplicationThemeManager.Changed -= OnThemeChanged;

        // Tray Icon 해제
        _ = Unregister();
    }

    /// <inheritdoc />
    public void ShowBalloonTip(string title, string message, BalloonTipIcon icon = BalloonTipIcon.Info)
    {
        if (!IsRegistered)
        {
            // Debug.WriteLine(
            //     $"WARN | Cannot show balloon tip - NotifyIcon is not registered.",
            //     "Wpf.Ui.NotifyIcon"
            // );
            return;
        }

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
        {
            // Debug.WriteLine(
            //     $"WARN | Balloon tip title and message cannot be null or empty.",
            //     "Wpf.Ui.NotifyIcon"
            // );
            return;
        }

        // Truncate to Windows limits
        if (title.Length > 64)
        {
            title = title.Substring(0, 64);
        }

        if (message.Length > 256)
        {
            message = message.Substring(0, 256);
        }

        BalloonTipSetting flags = (BalloonTipSetting)icon;

        ShellIconData.uFlags = Interop.Shell32.NIF.INFO;
        ShellIconData.szInfo = message;
        ShellIconData.szInfoTitle = title;
        ShellIconData.dwInfoFlags = (uint)flags;

        _ = Interop.Shell32.Shell_NotifyIcon(Interop.Shell32.NIM.MODIFY, ShellIconData);

        // Debug.WriteLine(
        //     $"INFO | Balloon tip shown: {title} - {message}",
        //     "Wpf.Ui.NotifyIcon"
        // );
    }

    /// <inheritdoc />
    public void HideBalloonTip()
    {
        if (!IsRegistered)
        {
            return;
        }

        ShellIconData.szInfo = string.Empty;
        ShellIconData.szInfoTitle = string.Empty;
        ShellIconData.uFlags = Interop.Shell32.NIF.INFO;

        _ = Interop.Shell32.Shell_NotifyIcon(Interop.Shell32.NIM.MODIFY, ShellIconData);

        // Debug.WriteLine(
        //     $"INFO | Balloon tip hidden.",
        //     "Wpf.Ui.NotifyIcon"
        // );
    }

    /// <inheritdoc />
    public IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        var uMsg = (Interop.User32.WM)msg;

        // System.Diagnostics.Debug.WriteLine(
        //     $"WndProc: msg={uMsg}, wParam={wParam}, lParam={lParam}",
        //     "Wpf.Ui.NotifyIcon"
        // );

        switch (uMsg)
        {
            case Interop.User32.WM.DESTROY:
                // System.Diagnostics.Debug.WriteLine(
                //     $"INFO | {typeof(TrayHandler)} received {uMsg} message.",
                //     "Wpf.Ui.NotifyIcon"
                // );
                Dispose();

                handled = true;

                return IntPtr.Zero;

            case Interop.User32.WM.NCDESTROY:
                // System.Diagnostics.Debug.WriteLine(
                //     $"INFO | {typeof(TrayHandler)} received {uMsg} message.",
                //     "Wpf.Ui.NotifyIcon"
                // );
                handled = false;

                return IntPtr.Zero;

            case Interop.User32.WM.CLOSE:
                // System.Diagnostics.Debug.WriteLine(
                //     $"INFO | {typeof(TrayHandler)} received {uMsg} message.",
                //     "Wpf.Ui.NotifyIcon"
                // );
                handled = true;

                return IntPtr.Zero;
        }

        if (uMsg != Interop.User32.WM.TRAYMOUSEMESSAGE)
        {
            handled = false;

            return IntPtr.Zero;
        }

        // NOTIFYICON_VERSION_4 mouse event handling:
        // - lParam contains the mouse message in LOWORD (lower 16 bits)
        // - HIWORD contains the icon ID
        // - We need to extract LOWORD to get WM_LBUTTONDOWN (0x0201), WM_RBUTTONDOWN (0x0204), etc.
        var lMsg = (Interop.User32.WM)(lParam.ToInt32() & 0xFFFF);

        // System.Diagnostics.Debug.WriteLine(
        //     $"TRAY MESSAGE: lParam={lParam}, lMsg={lMsg} (0x{((int)lMsg):X})",
        //     "Wpf.Ui.NotifyIcon"
        // );

        switch (lMsg)
        {
            case Interop.User32.WM.LBUTTONDOWN: // 0x0201 - Left button pressed
                OnLeftClick();

                if (FocusOnLeftClick)
                {
                    FocusApp();
                }

                break;

            case Interop.User32.WM.LBUTTONDBLCLK: // 0x0203 - Left button double-click
                OnLeftDoubleClick();
                break;

            case Interop.User32.WM.RBUTTONDOWN: // 0x0204 - Right button pressed
                OnRightClick();

                if (MenuOnRightClick)
                {
                    OpenMenu();
                }

                break;

            case Interop.User32.WM.RBUTTONDBLCLK: // 0x0206 - Right button double-click
                OnRightDoubleClick();
                break;

            case Interop.User32.WM.MBUTTONDOWN: // 0x0207 - Middle button pressed
                OnMiddleClick();
                break;

            case Interop.User32.WM.MBUTTONDBLCLK: // 0x0209 - Middle button double-click
                OnMiddleDoubleClick();
                break;
        }

        handled = true;

        return IntPtr.Zero;
    }
}
