// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Interop;

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// A button control that allows picking a color from anywhere on the screen.
/// Works like the eyedropper tool in MS Paint.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;ui:EyedropperButton Content="Pick Color" ColorPicked="OnColorPicked" /&gt;
/// </code>
/// </example>
public class EyedropperButton : Button
{
    private static readonly Cursor EyedropperCursor = LoadEyedropperCursor();

    /// <summary>
    /// Event raised when a color is picked from the screen.
    /// </summary>
    public event EventHandler<Color>? ColorPicked;

    /// <summary>
    /// Event raised when the eyedropper operation is cancelled (e.g., by pressing Escape).
    /// </summary>
    public event EventHandler? PickingCancelled;

    private Window? _overlayWindow;

    /// <summary>
    /// Initializes a new instance of the <see cref="EyedropperButton"/> class.
    /// </summary>
    public EyedropperButton()
    {
        Click += OnButtonClick;
    }

    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        StartCapture();
    }

    /// <summary>
    /// Starts the eyedropper capture mode using a fullscreen transparent overlay.
    /// </summary>
    public void StartCapture()
    {
        if (_overlayWindow != null)
        {
            return;
        }

        // Create nearly-transparent fullscreen overlay window
        // Note: Fully transparent (Alpha=0) won't receive mouse events in WPF
        _overlayWindow = new Window
        {
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)), // Almost transparent but receives mouse events
            Topmost = true,
            ShowInTaskbar = false,
            // Cover all monitors (virtual screen)
            Left = SystemParameters.VirtualScreenLeft,
            Top = SystemParameters.VirtualScreenTop,
            Width = SystemParameters.VirtualScreenWidth,
            Height = SystemParameters.VirtualScreenHeight,
            Cursor = EyedropperCursor,
        };

        _overlayWindow.MouseLeftButtonDown += OnOverlayMouseLeftButtonDown;
        _overlayWindow.MouseRightButtonDown += OnOverlayMouseRightButtonDown;
        _overlayWindow.KeyDown += OnOverlayKeyDown;
        _overlayWindow.Deactivated += OnOverlayDeactivated;

        _overlayWindow.Show();
        _ = _overlayWindow.Focus();
    }

    private void OnOverlayMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_overlayWindow == null)
        {
            return;
        }

        // Get screen coordinates
        Point screenPoint = _overlayWindow.PointToScreen(e.GetPosition(_overlayWindow));
        Color color = GetColorAtScreenPosition((int)screenPoint.X, (int)screenPoint.Y);

        CloseOverlay();
        ColorPicked?.Invoke(this, color);

        e.Handled = true;
    }

    private void OnOverlayMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Right-click cancels
        CloseOverlay();
        PickingCancelled?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }

    private void OnOverlayKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CloseOverlay();
            PickingCancelled?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }
    }

    private void OnOverlayDeactivated(object? sender, EventArgs e)
    {
        // If overlay loses focus, cancel
        if (_overlayWindow != null)
        {
            CloseOverlay();
            PickingCancelled?.Invoke(this, EventArgs.Empty);
        }
    }

    private void CloseOverlay()
    {
        if (_overlayWindow == null)
        {
            return;
        }

        // Store reference and set to null first to prevent re-entry from Deactivated event
        Window window = _overlayWindow;
        _overlayWindow = null;

        window.MouseLeftButtonDown -= OnOverlayMouseLeftButtonDown;
        window.MouseRightButtonDown -= OnOverlayMouseRightButtonDown;
        window.KeyDown -= OnOverlayKeyDown;
        window.Deactivated -= OnOverlayDeactivated;

        window.Close();
    }

    /// <summary>
    /// Loads the custom eyedropper cursor from embedded resources.
    /// </summary>
    private static Cursor LoadEyedropperCursor()
    {
        try
        {
            System.Windows.Resources.StreamResourceInfo? resourceInfo = Application.GetResourceStream(
                new Uri("pack://application:,,,/Wpf.Ui;component/Resources/Cursor/eyedropper.cur"));

            if (resourceInfo?.Stream != null)
            {
                return new Cursor(resourceInfo.Stream);
            }
        }
        catch
        {
            // Fallback to cross cursor if resource loading fails
        }

        return Cursors.Cross;
    }

    /// <summary>
    /// Gets the color at the specified screen position using Win32 API.
    /// </summary>
    /// <param name="x">The x-coordinate in screen pixels.</param>
    /// <param name="y">The y-coordinate in screen pixels.</param>
    /// <returns>The color at the specified position.</returns>
    private static Color GetColorAtScreenPosition(int x, int y)
    {
        // Get device context for the entire screen
        IntPtr hdc = User32.GetDC(IntPtr.Zero);

        try
        {
            // Get the pixel color
            uint pixel = Gdi32.GetPixel(hdc, x, y);

            // Handle CLR_INVALID (pixel outside clipping region)
            if (pixel == 0xFFFFFFFF)
            {
                return Colors.Black;
            }

            // Extract RGB components (COLORREF format: 0x00BBGGRR)
            byte r = (byte)(pixel & 0xFF);
            byte g = (byte)((pixel >> 8) & 0xFF);
            byte b = (byte)((pixel >> 16) & 0xFF);

            return Color.FromRgb(r, g, b);
        }
        finally
        {
            // Always release the device context
            _ = User32.ReleaseDC(IntPtr.Zero, hdc);
        }
    }
}
