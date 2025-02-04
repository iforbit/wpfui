// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls.Helpers;

internal static class ItemsControlHelper
{
    public static readonly DependencyProperty IsMovingItemsToDifferentControlProperty = DependencyProperty.RegisterAttached(
        "IsMovingItemsToDifferentControl", typeof(bool), typeof(ItemsControlHelper), new PropertyMetadata(BooleanBoxes.FalseBox));

    public static void SetIsMovingItemsToDifferentControl(DependencyObject element, bool value)
    {
        element.SetValue(IsMovingItemsToDifferentControlProperty, BooleanBoxes.Box(value));
    }

    public static bool GetIsMovingItemsToDifferentControl(DependencyObject element)
    {
        return (bool)element.GetValue(IsMovingItemsToDifferentControlProperty);
    }

    public static ItemsControl? ItemsControlFromItemContainer(DependencyObject? container)
    {
        if (container is null)
        {
            return null;
        }

        var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);

        if (itemsControl is not null
            && itemsControl != DependencyProperty.UnsetValue)
        {
            return itemsControl;
        }

        DependencyObject? visualParent = VisualTreeHelper.GetParent(container);
        if (visualParent is not null)
        {
            itemsControl = ItemsControl.ItemsControlFromItemContainer(visualParent);
        }

        if (itemsControl is not null
            && itemsControl != DependencyProperty.UnsetValue)
        {
            return itemsControl;
        }

        if (container is FrameworkElement { Parent: { } } frameworkElement)
        {
            itemsControl = ItemsControl.ItemsControlFromItemContainer(frameworkElement.Parent);
        }

        return itemsControl;
    }

    public static void MoveItemsToDifferentControl(ItemsControl source, ItemsControl target)
    {
        try
        {
            SetIsMovingItemsToDifferentControl(source, true);
            SetIsMovingItemsToDifferentControl(target, true);

            System.Collections.IEnumerable? itemsSource = source.ItemsSource;
            if (itemsSource is not null)
            {
                source.SetCurrentValue(ItemsControl.ItemsSourceProperty, null);
                target.SetCurrentValue(ItemsControl.ItemsSourceProperty, itemsSource);
            }
            else
            {
                while (source.Items.Count > 0)
                {
                    var item = source.Items[0];
                    source.Items.Remove(item);
                    _ = target.Items.Add(item);
                }
            }
        }
        finally
        {
            SetIsMovingItemsToDifferentControl(source, false);
            SetIsMovingItemsToDifferentControl(target, false);
        }
    }
}