// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using System.Windows.Documents;

namespace Wpf.Ui.Internal;
internal static class UIHelper
{
    /// <summary>
    /// Gets the first visual child of <paramref name="parent"/>.
    /// If there are no visual children <c>null</c> is returned.
    /// </summary>
    /// <returns>The first visual child of <paramref name="parent"/> or <c>null</c> if there are no children.</returns>
    public static DependencyObject? GetFirstVisualChild( DependencyObject parent )
    {
        var childrenCount = VisualTreeHelper.GetChildrenCount(parent);

        return childrenCount == 0
            ? null
            : VisualTreeHelper.GetChild(parent, 0);
    }

    /// <summary>
    /// Tries to find immediate visual child of type <typeparamref name="T"/> which matches <paramref name="predicate"/>
    /// </summary>
    /// <typeparam name="T">The type of visual child to search for. This must be a subclass of <see cref="DependencyObject"/>.</typeparam>
    /// <param name="parent">The parent dependency object in which to search for the visual child.</param>
    /// <param name="predicate">A predicate that defines the condition the child must meet.</param>
    /// <returns>
    /// The visual child of type <typeparamref name="T"/> that matches <paramref name="predicate"/>.
    /// Returns <c>null</c> if no child matches.
    /// </returns>
    public static T? FindImmediateVisualChild<T>( DependencyObject parent, Predicate<T> predicate )
        where T : DependencyObject
    {
        foreach (DependencyObject child in GetVisualChildren(parent))
        {
            if (child is T obj
                && predicate(obj))
            {
                return obj;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the first visual child of type TChildItem by walking down the visual tree.
    /// </summary>
    /// <typeparam name="TChildItem">The type of visual child to find.</typeparam>
    /// <param name="parent">The parent element whose visual tree shall be walked down.</param>
    /// <returns>The first element of type TChildItem found in the visual tree is returned. If none is found, null is returned.</returns>
    public static TChildItem? FindVisualChild<TChildItem>( DependencyObject parent )
        where TChildItem : DependencyObject
    {
        foreach (DependencyObject child in GetVisualChildren(parent))
        {
            if (child is TChildItem item)
            {
                return item;
            }

            TChildItem? childOfChild = FindVisualChild<TChildItem>(child);
            if (childOfChild is not null)
            {
                return childOfChild;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets all visual children of <paramref name="parent"/>.
    /// </summary>
    public static IEnumerable<DependencyObject> GetVisualChildren( DependencyObject parent )
    {
        var visualChildrenCount = VisualTreeHelper.GetChildrenCount(parent);

        for (var i = 0; i < visualChildrenCount; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);

            yield return child;
        }
    }

    /// <summary>
    /// Finds the parent control of type <typeparamref name="T"/>.
    /// First looks at the visual tree and then at the logical tree to find the parent.
    /// </summary>
    /// <typeparam name="T">The type of parent control to search for. This must be a subclass of <see cref="DependencyObject"/>.</typeparam>

    /// <returns>The found visual/logical parent or null.</returns>
    /// <remarks>This method searches further up the parent chain instead of just using the immediate parent.</remarks>
    public static T? GetParent<T>( DependencyObject? element, Predicate<T>? filter = null )
        where T : DependencyObject
    {
        if (element is null)
        {
            return null;
        }

        {
            DependencyObject? item = GetVisualParent(element);

            while (item is not null)
            {
                if (item is T variable
                    && (filter?.Invoke(variable) ?? true))
                {
                    return variable;
                }

                item = GetVisualParent(item) ?? LogicalTreeHelper.GetParent(item);
            }
        }

        {
            DependencyObject? item = LogicalTreeHelper.GetParent(element);

            while (item is not null)
            {
                if (item is T variable
                    && (filter?.Invoke(variable) ?? true))
                {
                    return variable;
                }

                item = LogicalTreeHelper.GetParent(item);
            }
        }

        return null;
    }

    /// <summary>
    /// Returns either the visual or logical parent of <paramref name="element"/>.
    /// This also works for <see cref="ContentElement"/> and <see cref="FrameworkContentElement"/>.
    /// </summary>
    public static DependencyObject GetVisualOrLogicalParent( DependencyObject element )
    {
        return GetVisualParent(element) ?? LogicalTreeHelper.GetParent(element);
    }

    /// <summary>
    /// Returns the visual parent of <paramref name="element"/>.
    /// This also works for <see cref="ContentElement"/> and <see cref="FrameworkContentElement"/>.
    /// </summary>
    public static DependencyObject? GetVisualParent( DependencyObject? element )
    {
        if (element is null)
        {
            return null;
        }

        if (element is ContentElement contentElement)
        {
            DependencyObject? parent = ContentOperations.GetParent(contentElement);

            if (parent is not null)
            {
                return parent;
            }

            var frameworkContentElement = contentElement as FrameworkContentElement;
            return frameworkContentElement?.Parent;
        }

        return VisualTreeHelper.GetParent(element);
    }

    /// <summary>
    /// First checks if <paramref name="visual"/> is either a <see cref="AdornerDecorator"/> or <see cref="ScrollContentPresenter"/> and if it is returns it's <see cref="AdornerLayer"/>.
    /// If those checks yield no result <see cref="AdornerLayer.GetAdornerLayer"/> is called.
    /// </summary>
    /// <param name="visual">The visual element for which to find an adorner layer.</param>
    /// <returns>An adorner layer for the specified visual, or null if no adorner layer can be found.</returns>
    /// <exception cref="T:System.ArgumentNullException">Raised when visual is null.</exception>
    public static AdornerLayer? GetAdornerLayer( Visual visual )
    {
        if (visual is null)
        {
            throw new ArgumentNullException(nameof(visual));
        }

        if (visual is AdornerDecorator decorator)
        {
            return decorator.AdornerLayer;
        }

        if (visual is ScrollContentPresenter scrollContentPresenter)
        {
            return scrollContentPresenter.AdornerLayer;
        }

        return AdornerLayer.GetAdornerLayer(visual);
    }

    /// <summary>
    /// Gets all containers from the <see cref="ItemContainerGenerator"/> of <paramref name="itemsControl"/>.
    /// </summary>
    /// <typeparam name="T">The desired container type.</typeparam>
    public static IEnumerable<T> GetAllItemContainers<T>( ItemsControl itemsControl )
        where T : class
    {
        return GetAllItemContainers<T>(itemsControl.ItemContainerGenerator);
    }

    /// <summary>
    /// Gets all containers from <paramref name="itemContainerGenerator"/>.
    /// </summary>
    /// <typeparam name="T">The desired container type.</typeparam>
    public static IEnumerable<T> GetAllItemContainers<T>( ItemContainerGenerator itemContainerGenerator )
        where T : class
    {
        for (var i = 0; i < itemContainerGenerator.Items.Count; i++)
        {
            if (itemContainerGenerator.ContainerFromIndex(i) is T container)
            {
                yield return container;
            }
        }
    }
}