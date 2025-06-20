// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Runtime.InteropServices;

namespace Wpf.Ui.DirectX.Rendering;

internal static class NativeMethods
{
    private static readonly WndProcDelegate _wndProcDelegate = DefWindowProc;

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern ushort RegisterClassExW([In] ref WndClassEx lpwcx);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr CreateWindow(
        string lpClassName,
        IntPtr lpWindowName,
        int dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

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
            HIcon = IntPtr.Zero,
            HCursor = IntPtr.Zero,
            HbrBackground = IntPtr.Zero,
            LpszMenuName = null,
            LpszClassName = className,
            HIconSm = IntPtr.Zero
        };

        _ = RegisterClassExW(ref wndClass);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct WndClassEx
{
    public int CbSize;
    public int Style;
    public IntPtr LpfnWndProc;
    public int CbClsExtra;
    public int CbWndExtra;
    public IntPtr HInstance;
    public IntPtr HIcon;
    public IntPtr HCursor;
    public IntPtr HbrBackground;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string? LpszMenuName;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string? LpszClassName;
    public IntPtr HIconSm;
}