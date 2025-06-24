// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Runtime.InteropServices;

namespace Wpf.Ui.DirectX.Rendering.Interop;

internal static class NativeMethods
{
    private static readonly WndProcDelegate _wndProcDelegate = DefWindowProc;

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern ushort RegisterClassExW([In] ref WndClassEx lpwcx);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern nint CreateWindow(
        string lpClassName,
        nint lpWindowName,
        int dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        nint hWndParent,
        nint hMenu,
        nint hInstance,
        nint lpParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyWindow(nint hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint DefWindowProc(nint hWnd, uint msg, nint wParam, nint lParam);

    public delegate nint WndProcDelegate(nint hWnd, uint msg, nint wParam, nint lParam);

    public static void RegisterWindowClass(string className)
    {
        var wndClass = new WndClassEx
        {
            CbSize = Marshal.SizeOf<WndClassEx>(),
            Style = 0,
            LpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            CbClsExtra = 0,
            CbWndExtra = 0,
            HInstance = Marshal.GetHINSTANCE(typeof(NativeMethods).Module),
            HIcon = nint.Zero,
            HCursor = nint.Zero,
            HbrBackground = nint.Zero,
            LpszMenuName = null,
            LpszClassName = className,
            HIconSm = nint.Zero
        };

        _ = RegisterClassExW(ref wndClass);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct WndClassEx
{
    public int CbSize;
    public int Style;
    public nint LpfnWndProc;
    public int CbClsExtra;
    public int CbWndExtra;
    public nint HInstance;
    public nint HIcon;
    public nint HCursor;
    public nint HbrBackground;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string? LpszMenuName;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string? LpszClassName;
    public nint HIconSm;
}