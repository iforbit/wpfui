// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Runtime.InteropServices;

namespace Wpf.Ui.DirectX.Rendering.Interop;

// Window 생성 유틸리티 (User32Interop.cs 등 별도 파일에 존재해야 함)
internal static class User32Interop
{
    private const string CLASS_NAME = "GraphControlHost";
    private const string WINDOW_CLASS = "STATIC";

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern nint CreateWindowExW(
     uint dwExStyle,
     string lpClassName,
     string lpWindowName,
     uint dwStyle,
     int x,
     int y,
     int nWidth,
     int nHeight,
     nint hWndParent,
     nint hMenu,
     nint hInstance,
     nint lpParam);

    public static nint CreateHostWindow(nint hwndParent, int width, int height)
    {
        return CreateWindowExW(
            0,
            WINDOW_CLASS,
            string.Empty,
            0x40000000 | 0x10000000, // WS_CHILD | WS_VISIBLE
            0,
            0,
            width,
            height,
            hwndParent,
            nint.Zero,
            nint.Zero,
            nint.Zero);
    }

    public static void DestroyWindow(nint hwnd)
    {
        _ = NativeMethods.DestroyWindow(hwnd);
    }

    private const int WS_CHILD = 0x40000000;
    private const int WS_VISIBLE = 0x10000000;
}

