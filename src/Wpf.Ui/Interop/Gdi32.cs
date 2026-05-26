// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Runtime.InteropServices;

namespace Wpf.Ui.Interop;

/// <summary>
/// GDI32 procedure declarations for graphics device interface functions.
/// </summary>
internal static class Gdi32
{
    /// <summary>
    /// Retrieves the red, green, blue (RGB) color value of the pixel at the specified coordinates.
    /// </summary>
    /// <param name="hdc">A handle to the device context.</param>
    /// <param name="x">The x-coordinate, in logical units, of the pixel to be examined.</param>
    /// <param name="y">The y-coordinate, in logical units, of the pixel to be examined.</param>
    /// <returns>
    /// The return value is the COLORREF value that specifies the RGB of the pixel.
    /// If the pixel is outside of the current clipping region, the return value is CLR_INVALID (0xFFFFFFFF).
    /// </returns>
    [DllImport(Libraries.Gdi32)]
    internal static extern uint GetPixel(IntPtr hdc, int x, int y);
}
