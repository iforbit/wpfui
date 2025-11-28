// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Input;

using Wpf.Ui.Interop;

namespace Wpf.Ui.Controls;

public static class WindowSteeringHelper
{
    /// <summary>
    /// 마우스 왼쪽 버튼을 눌렀을 때 창 이동 또는 최대화 처리
    /// </summary>
    public static void HandleMouseLeftButtonDown(MouseButtonEventArgs e, bool handleDragMove, bool handleStateChange)
    {
        if (e.Source is not DependencyObject dpo)
        {
            return;
        }

        HandleMouseLeftButtonDown(dpo, e, handleDragMove, handleStateChange);
    }

    public static void HandleMouseLeftButtonDown(DependencyObject dependencyObject, MouseButtonEventArgs e, bool handleDragMove, bool handleStateChange)
    {
        var window = Window.GetWindow(dependencyObject);
        if (window is null)
        {
            return;
        }

        if (handleDragMove && e.ClickCount == 1)
        {
            e.Handled = true;

            // 마우스 캡처 해제 후 창 이동
            _ = User32.ReleaseCapture();
            try
            {
                window.DragMove();
            }
            catch
            {
                // DragMove 중 오류 발생 가능 (예: 마우스가 빠르게 움직일 경우)
            }
        }
        else if (handleStateChange && e.ClickCount == 2 &&
                 (window.ResizeMode == ResizeMode.CanResize || window.ResizeMode == ResizeMode.CanResizeWithGrip))
        {
            e.Handled = true;
            ToggleWindowState(window);
        }
    }

    private static void ToggleWindowState(Window window)
    {
        IntPtr hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        if (window.WindowState == WindowState.Normal)
        {
            _ = User32.ShowWindow(hwnd, User32.SW.MAXIMIZE);
        }
        else
        {
            _ = User32.ShowWindow(hwnd, User32.SW.RESTORE);
        }
    }

    /// <summary>
    /// 시스템 메뉴를 표시 (Win32 API 사용)
    /// </summary>
    public static void ShowSystemMenu(DependencyObject dependencyObject, MouseButtonEventArgs e)
    {
        var window = Window.GetWindow(dependencyObject);
        if (window is null)
        {
            return;
        }

        ShowSystemMenu(window, e);
    }

    public static void ShowSystemMenu(Window window, MouseButtonEventArgs e)
    {
        e.Handled = true;
        IntPtr hwnd = new WindowInteropHelper(window).Handle;

        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        IntPtr hMenu = User32.GetSystemMenu(hwnd, false);
        if (hMenu != IntPtr.Zero)
        {
            Point screenLocation = window.PointToScreen(Mouse.GetPosition(window));
            _ = User32.TrackPopupMenuEx(hMenu, 0, (int)screenLocation.X, (int)screenLocation.Y, hwnd, IntPtr.Zero);
        }
    }

    /// <summary>
    /// 특정 위치에서 시스템 메뉴를 표시
    /// </summary>
    public static void ShowSystemMenu(Window window, Point screenLocation)
    {
        IntPtr hwnd = new WindowInteropHelper(window).Handle;
        IntPtr hMenu = User32.GetSystemMenu(hwnd, false);

        if (hMenu != IntPtr.Zero)
        {
            _ = User32.TrackPopupMenuEx(hMenu, 0, (int)screenLocation.X, (int)screenLocation.Y, hwnd, IntPtr.Zero);
        }
    }

    /// <summary>
    /// 창의 위치 및 크기 설정
    /// </summary>
    public static void SetWindowPosition(Window window, int x, int y, int width, int height)
    {
        IntPtr hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        _ = User32.SetWindowPos(hwnd, IntPtr.Zero, x, y, width, height, User32.SWP.NOZORDER);
    }

    /// <summary>
    /// 창을 최상위로 가져오기
    /// </summary>
    public static void BringWindowToFront(Window window)
    {
        IntPtr hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        _ = User32.SetForegroundWindow(hwnd);
    }

    /// <summary>
    /// 특정 DPI 값을 가져오기
    /// </summary>
    public static uint GetDpiForWindow(Window window)
    {
        IntPtr hwnd = new WindowInteropHelper(window).Handle;
        return hwnd != IntPtr.Zero ? User32.GetDpiForWindow(hwnd) : 96; // 기본 DPI = 96
    }
}