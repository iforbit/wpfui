// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Lighted Solutions and Contributors.

using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Controls;

/// <summary>
/// A simple button for MenuBar window controls (minimize, maximize, close).
/// </summary>
public class MenuBarButton : Button
{
    /// <summary>
    /// Identifies the <see cref="ButtonType"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ButtonTypeProperty = DependencyProperty.Register(
        nameof(ButtonType),
        typeof(TitleBarButtonType),
        typeof(MenuBarButton),
        new PropertyMetadata(TitleBarButtonType.Unknown)
    );

    /// <summary>
    /// Identifies the <see cref="HoverBackground"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register(
        nameof(HoverBackground),
        typeof(Brush),
        typeof(MenuBarButton),
        new PropertyMetadata(null)
    );

    /// <summary>
    /// Identifies the <see cref="HoverForeground"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HoverForegroundProperty = DependencyProperty.Register(
        nameof(HoverForeground),
        typeof(Brush),
        typeof(MenuBarButton),
        new PropertyMetadata(null)
    );

    /// <summary>
    /// Gets or sets the type of this button.
    /// </summary>
    public TitleBarButtonType ButtonType
    {
        get => (TitleBarButtonType)GetValue(ButtonTypeProperty);
        set => SetValue(ButtonTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the background brush when hovered.
    /// </summary>
    public Brush? HoverBackground
    {
        get => (Brush?)GetValue(HoverBackgroundProperty);
        set => SetValue(HoverBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the foreground brush when hovered.
    /// </summary>
    public Brush? HoverForeground
    {
        get => (Brush?)GetValue(HoverForegroundProperty);
        set => SetValue(HoverForegroundProperty, value);
    }

    static MenuBarButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MenuBarButton),
            new FrameworkPropertyMetadata(typeof(MenuBarButton))
        );
    }
}
