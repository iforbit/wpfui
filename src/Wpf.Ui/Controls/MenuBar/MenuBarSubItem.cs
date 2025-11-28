// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Lighted Solutions and Contributors.

using System.Windows;
using System.Windows.Input;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents a submenu item in the MenuBar with enhanced icon, keyboard shortcut, and toggle support.
/// </summary>
public class MenuBarSubItem : System.Windows.Controls.MenuItem
{
    /// <summary>
    /// Identifies the <see cref="Icon"/> dependency property.
    /// </summary>
    public static new readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(IconElement),
        typeof(MenuBarSubItem),
        new PropertyMetadata(null, null, IconElement.Coerce)
    );

    /// <summary>
    /// Identifies the <see cref="IconSize"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
        nameof(IconSize),
        typeof(double),
        typeof(MenuBarSubItem),
        new PropertyMetadata(16.0)
    );

    /// <summary>
    /// Identifies the <see cref="Description"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description),
        typeof(string),
        typeof(MenuBarSubItem),
        new PropertyMetadata(string.Empty)
    );

    /// <summary>
    /// Identifies the <see cref="ShortcutKey"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ShortcutKeyProperty = DependencyProperty.Register(
        nameof(ShortcutKey),
        typeof(string),
        typeof(MenuBarSubItem),
        new PropertyMetadata(string.Empty, OnShortcutKeyChanged)
    );

    /// <summary>
    /// Identifies the <see cref="IsToggle"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsToggleProperty = DependencyProperty.Register(
        nameof(IsToggle),
        typeof(bool),
        typeof(MenuBarSubItem),
        new PropertyMetadata(false)
    );

    /// <summary>
    /// Identifies the <see cref="ToggleState"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ToggleStateProperty = DependencyProperty.Register(
        nameof(ToggleState),
        typeof(bool),
        typeof(MenuBarSubItem),
        new PropertyMetadata(false)
    );

    /// <summary>
    /// Gets or sets the icon for this menu item.
    /// </summary>
    public new IconElement? Icon
    {
        get => (IconElement?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the size of the icon.
    /// </summary>
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets an optional description for the menu item.
    /// </summary>
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Gets or sets the keyboard shortcut text (e.g., "Ctrl+S"). This is an alias for InputGestureText.
    /// </summary>
    public string ShortcutKey
    {
        get => (string)GetValue(ShortcutKeyProperty);
        set => SetValue(ShortcutKeyProperty, value);
    }

    /// <summary>
    /// Gets or sets whether this item acts as a toggle button.
    /// </summary>
    public bool IsToggle
    {
        get => (bool)GetValue(IsToggleProperty);
        set => SetValue(IsToggleProperty, value);
    }

    /// <summary>
    /// Gets or sets the toggle state when IsToggle is true.
    /// </summary>
    public bool ToggleState
    {
        get => (bool)GetValue(ToggleStateProperty);
        set => SetValue(ToggleStateProperty, value);
    }

    static MenuBarSubItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MenuBarSubItem),
            new FrameworkPropertyMetadata(typeof(MenuBarSubItem))
        );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuBarSubItem"/> class.
    /// </summary>
    public MenuBarSubItem()
    {
    }

    /// <inheritdoc />
    protected override void OnClick()
    {
        if (IsToggle)
        {
            ToggleState = !ToggleState;
            IsChecked = ToggleState;
        }

        base.OnClick();
    }

    private static void OnShortcutKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuBarSubItem item && e.NewValue is string shortcut)
        {
            item.InputGestureText = shortcut;
        }
    }
}
