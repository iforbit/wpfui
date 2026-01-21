// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Lighted Solutions and Contributors.

using System.Windows.Shell;

namespace Wpf.Ui.Controls;

/// <summary>
/// A window that integrates MenuBar in the title bar area using WindowChrome.
/// </summary>
public class MenuBarWindow : FluentWindow
{
    /// <summary>
    /// Identifies the <see cref="MenuBar"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MenuBarProperty = DependencyProperty.Register(
        nameof(MenuBar),
        typeof(MenuBar),
        typeof(MenuBarWindow),
        new PropertyMetadata(null, OnMenuBarChanged)
    );

    /// <summary>
    /// Identifies the <see cref="MenuBarHeight"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MenuBarHeightProperty = DependencyProperty.Register(
        nameof(MenuBarHeight),
        typeof(double),
        typeof(MenuBarWindow),
        new PropertyMetadata(32.0, OnMenuBarHeightChanged)
    );

    /// <summary>
    /// Gets or sets the MenuBar for this window.
    /// </summary>
    public MenuBar? MenuBar
    {
        get => (MenuBar?)GetValue(MenuBarProperty);
        set => SetValue(MenuBarProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the menu bar area.
    /// </summary>
    public double MenuBarHeight
    {
        get => (double)GetValue(MenuBarHeightProperty);
        set => SetValue(MenuBarHeightProperty, value);
    }

    static MenuBarWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MenuBarWindow),
            new FrameworkPropertyMetadata(typeof(MenuBarWindow))
        );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuBarWindow"/> class.
    /// </summary>
    public MenuBarWindow()
    {
        SetupWindowChrome();
    }

    private void SetupWindowChrome()
    {
        var chrome = new WindowChrome
        {
            CaptionHeight = MenuBarHeight,
            CornerRadius = new CornerRadius(0),
            GlassFrameThickness = new Thickness(-1),
            ResizeBorderThickness = new Thickness(4),
            UseAeroCaptionButtons = false
        };

        WindowChrome.SetWindowChrome(this, chrome);
    }

    private static void OnMenuBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuBarWindow window && e.NewValue is MenuBar menuBar)
        {
            menuBar.Height = window.MenuBarHeight;
        }
    }

    private static void OnMenuBarHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuBarWindow window && e.NewValue is double height)
        {
            var chrome = WindowChrome.GetWindowChrome(window);
            if (chrome != null)
            {
                chrome.CaptionHeight = height;
            }

            if (window.MenuBar != null)
            {
                window.MenuBar.SetCurrentValue(MenuBar.HeightProperty, height);
            }
        }
    }

    /// <inheritdoc />
    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);

        if (MenuBar != null)
        {
            MenuBar.SetCurrentValue(MenuBar.IsMaximizedProperty, WindowState == WindowState.Maximized);
        }
    }
}
