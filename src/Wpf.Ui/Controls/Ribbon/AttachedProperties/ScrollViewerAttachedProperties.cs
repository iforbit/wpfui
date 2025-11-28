// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;

namespace Wpf.Ui.Controls;

/// <summary>
/// Attached properties for <see cref="ScrollViewer"/>.
/// </summary>
[StyleTypedProperty(Property = "ScrollBarStyle", StyleTargetType = typeof(ScrollViewer))]
public class ScrollViewerAttachedProperties : DependencyObject
{
    /// <summary>
    /// Defines the <see cref="Style"/> to be used for the <see cref="ScrollBarStyleProperty"/> of an <see cref="ScrollViewer"/>.
    /// </summary>
    public static readonly DependencyProperty ScrollBarStyleProperty = DependencyProperty.RegisterAttached("ScrollBarStyle", typeof(Style), typeof(ScrollViewerAttachedProperties), new PropertyMetadata(default(Style)));

    /// <summary>Helper for setting <see cref="ScrollBarStyleProperty"/> on <paramref name="element"/>.</summary>
    /// <param name="element"><see cref="DependencyObject"/> to set <see cref="ScrollBarStyleProperty"/> on.</param>
    /// <param name="value">ScrollBarStyle property value.</param>
    public static void SetScrollBarStyle(DependencyObject element, Style? value)
    {
        element.SetValue(ScrollBarStyleProperty, value);
    }

    /// <summary>Helper for getting <see cref="ScrollBarStyleProperty"/> from <paramref name="element"/>.</summary>
    /// <param name="element"><see cref="DependencyObject"/> to read <see cref="ScrollBarStyleProperty"/> from.</param>
    /// <returns>ScrollBarStyle property value.</returns>
    [AttachedPropertyBrowsableForType(typeof(ScrollViewer))]
    public static Style? GetScrollBarStyle(DependencyObject element)
    {
        return (Style)element.GetValue(ScrollBarStyleProperty);
    }
}
