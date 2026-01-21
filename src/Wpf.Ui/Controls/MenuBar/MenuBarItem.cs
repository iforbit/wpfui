// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Lighted Solutions and Contributors.

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents a top-level menu item in the MenuBar with enhanced icon and keyboard shortcut support.
/// </summary>
public class MenuBarItem : System.Windows.Controls.MenuItem
{
    /// <summary>
    /// Identifies the <see cref="Icon"/> dependency property.
    /// </summary>
    public static new readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(IconElement),
        typeof(MenuBarItem),
        new PropertyMetadata(null, null, IconElement.Coerce)
    );

    /// <summary>
    /// Identifies the <see cref="IconSize"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
        nameof(IconSize),
        typeof(double),
        typeof(MenuBarItem),
        new PropertyMetadata(16.0)
    );

    /// <summary>
    /// Identifies the <see cref="Description"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description),
        typeof(string),
        typeof(MenuBarItem),
        new PropertyMetadata(string.Empty)
    );

    /// <summary>
    /// Identifies the <see cref="IsHighlightedOnOpen"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsHighlightedOnOpenProperty = DependencyProperty.Register(
        nameof(IsHighlightedOnOpen),
        typeof(bool),
        typeof(MenuBarItem),
        new PropertyMetadata(true)
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
    /// Gets or sets an optional description for the menu item (used in tooltips or extended displays).
    /// </summary>
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the item should be highlighted when submenu is open.
    /// </summary>
    public bool IsHighlightedOnOpen
    {
        get => (bool)GetValue(IsHighlightedOnOpenProperty);
        set => SetValue(IsHighlightedOnOpenProperty, value);
    }

    static MenuBarItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MenuBarItem),
            new FrameworkPropertyMetadata(typeof(MenuBarItem))
        );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuBarItem"/> class.
    /// </summary>
    public MenuBarItem()
    {
    }
}
