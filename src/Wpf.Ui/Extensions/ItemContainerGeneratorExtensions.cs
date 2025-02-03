// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using Wpf.Ui.Internal;

namespace Wpf.Ui.Extensions;

/// <summary>
/// Extension-Methods for <see cref="ItemContainerGenerator" />.
/// </summary>
public static class ItemContainerGeneratorExtensions
{
    /// <summary>
    /// Returns the container or the content of the container.
    /// </summary>
    /// <returns>
    /// The container for <paramref name="item" /> if the container is of type <typeparamref name="TContainerOrContent" />.
    /// The container content for <paramref name="item" /> if the container content is of type <typeparamref name="TContainerOrContent" />, but the container itself is not of type <typeparamref name="TContainerOrContent" />.
    /// </returns>
    public static TContainerOrContent? ContainerOrContainerContentFromItem<TContainerOrContent>( this ItemContainerGenerator @this, object? item )
        where TContainerOrContent : class
    {
        var container = @this.ContainerFromItem(item) as TContainerOrContent;

        if (container is not null)
        {
            return container;
        }

        var contentPresenterFromContainer = @this.ContainerFromItem(item) as ContentPresenter;
        if (contentPresenterFromContainer is not null
            && VisualTreeHelper.GetChildrenCount(contentPresenterFromContainer) > 0)
        {
            return VisualTreeHelper.GetChild(contentPresenterFromContainer, 0) as TContainerOrContent;
        }

        return null;
    }

    /// <summary>
    /// Returns the container or the content of the container.
    /// </summary>
    /// <returns>
    /// The container for <paramref name="index" /> if the container is of type <typeparamref name="TContainerOrContent" />.
    /// The container content for <paramref name="index" /> if the container content is of type <typeparamref name="TContainerOrContent" />, but the container itself is not of type <typeparamref name="TContainerOrContent" />.
    /// </returns>
    public static TContainerOrContent? ContainerOrContainerContentFromIndex<TContainerOrContent>( this ItemContainerGenerator @this, int index )
        where TContainerOrContent : class
    {
        var container = @this.ContainerFromIndex(index) as TContainerOrContent;

        if (container is not null)
        {
            return container;
        }

        var contentPresenterFromContainer = @this.ContainerFromIndex(index) as ContentPresenter;
        if (contentPresenterFromContainer is not null
            && VisualTreeHelper.GetChildrenCount(contentPresenterFromContainer) > 0)
        {
            return VisualTreeHelper.GetChild(contentPresenterFromContainer, 0) as TContainerOrContent;
        }

        return null;
    }

    public static object? ItemFromContainerOrContainerContent( this ItemContainerGenerator @this, DependencyObject? container )
    {
        if (container is null)
        {
            return null;
        }

        var item = @this.ItemFromContainer(container);

        if (item is not null
            && item != DependencyProperty.UnsetValue)
        {
            return item;
        }

        var visualParent = UIHelper.GetVisualParent(container);
        if (visualParent is not null)
        {
            item = @this.ItemFromContainer(visualParent);
        }

        if (item is not null
            && item != DependencyProperty.UnsetValue)
        {
            return item;
        }

        if (container is FrameworkElement frameworkElement
            && frameworkElement.Parent is not null)
        {
            item = @this.ItemFromContainer(frameworkElement.Parent);
        }

        return item;
    }
}