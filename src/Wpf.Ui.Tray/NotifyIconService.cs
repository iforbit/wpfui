// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, you can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Tray;

/// <summary>
/// Base implementation of the notify icon service.
/// Fixed: Now properly implements IDisposable
/// </summary>
public class NotifyIconService : INotifyIconService, IDisposable
{
    private readonly Internal.InternalNotifyIconManager internalNotifyIconManager;
    private bool _disposed = false;

    public event NotifyIconEventHandler? LeftClick;

    public event NotifyIconEventHandler? LeftDoubleClick;

    public event NotifyIconEventHandler? RightClick;

    public Window ParentWindow { get; internal set; } = null!;

    public int Id => internalNotifyIconManager.Id;

    public bool IsRegistered => internalNotifyIconManager.IsRegistered;

    public string TooltipText
    {
        get => internalNotifyIconManager.TooltipText;
        set => internalNotifyIconManager.TooltipText = value;
    }

    public ContextMenu? ContextMenu
    {
        get => internalNotifyIconManager.ContextMenu;
        set => internalNotifyIconManager.ContextMenu = value;
    }

    public ImageSource? Icon
    {
        get => internalNotifyIconManager.Icon;
        set => internalNotifyIconManager.Icon = value;
    }

    public NotifyIconService()
    {
        internalNotifyIconManager = new Internal.InternalNotifyIconManager();

        RegisterHandlers();
    }

    ~NotifyIconService()
    {
        Dispose(false);
    }

    public bool Register()
    {
        if (ParentWindow is not null)
        {
            return internalNotifyIconManager.Register(ParentWindow);
        }

        return internalNotifyIconManager.Register();
    }

    public bool Unregister()
    {
        return internalNotifyIconManager.Unregister();
    }

    /// <inheritdoc />
    public void SetParentWindow(Window parentWindow)
    {
        if (ParentWindow is not null)
        {
            ParentWindow.Closing -= OnParentWindowClosing;
        }

        ParentWindow = parentWindow;
        ParentWindow.Closing += OnParentWindowClosing;
    }

    /// <inheritdoc />
    public void ShowBalloonTip(string title, string message, BalloonTipIcon icon = BalloonTipIcon.Info)
    {
        internalNotifyIconManager.ShowBalloonTip(title, message, icon);
    }

    /// <inheritdoc />
    public void HideBalloonTip()
    {
        internalNotifyIconManager.HideBalloonTip();
    }

    /// <summary>
    /// This virtual method is called when the user clicks the left mouse button on the tray icon.
    /// </summary>
    protected virtual void OnLeftClick()
    {
        LeftClick?.Invoke();
    }

    /// <summary>
    /// This virtual method is called when the user double-clicks the left mouse button on the tray icon.
    /// </summary>
    protected virtual void OnLeftDoubleClick()
    {
        LeftDoubleClick?.Invoke();
    }

    /// <summary>
    /// This virtual method is called when the user clicks the right mouse button on the tray icon.
    /// </summary>
    protected virtual void OnRightClick()
    {
        RightClick?.Invoke();
    }

    /// <summary>
    /// This virtual method is called when the user double-clicks the right mouse button on the tray icon.
    /// </summary>
    protected virtual void OnRightDoubleClick() { }

    /// <summary>
    /// This virtual method is called when the user clicks the middle mouse button on the tray icon.
    /// </summary>
    protected virtual void OnMiddleClick() { }

    /// <summary>
    /// This virtual method is called when the user double-clicks the middle mouse button on the tray icon.
    /// </summary>
    protected virtual void OnMiddleDoubleClick() { }

    private void OnParentWindowClosing(object? sender, CancelEventArgs e)
    {
        // Only dispose if the window is actually closing (not just hiding)
        if (!e.Cancel)
        {
            Dispose();
        }
    }

    private void RegisterHandlers()
    {
        internalNotifyIconManager.LeftClick += OnLeftClick;
        internalNotifyIconManager.LeftDoubleClick += OnLeftDoubleClick;
        internalNotifyIconManager.RightClick += OnRightClick;
        internalNotifyIconManager.RightDoubleClick += OnRightDoubleClick;
        internalNotifyIconManager.MiddleClick += OnMiddleClick;
        internalNotifyIconManager.MiddleDoubleClick += OnMiddleDoubleClick;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Clean up managed resources
            if (ParentWindow is not null)
            {
                ParentWindow.Closing -= OnParentWindowClosing;
            }

            internalNotifyIconManager.Dispose();
        }

        _disposed = true;
    }
}
