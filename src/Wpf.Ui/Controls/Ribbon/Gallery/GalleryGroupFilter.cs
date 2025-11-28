// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents gallery group filter definition.
/// </summary>
public class GalleryGroupFilter : DependencyObject
{
    /// <summary>
    /// Gets or sets title of filter.
    /// </summary>
    public string Title
    {
        get => (string)this.GetValue(TitleProperty);
        set => this.SetValue(TitleProperty, value);
    }

    /// <summary>Identifies the <see cref="Title"/> dependency property.</summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(GalleryGroupFilter), new PropertyMetadata("GalleryGroupFilter"));

    /// <summary>
    /// Gets or sets list of groups separated by comma.
    /// </summary>
    public string Groups
    {
        get => (string)this.GetValue(GroupsProperty);
        set => this.SetValue(GroupsProperty, value);
    }

#pragma warning disable WPF0010 // Default value type must match registered type.
    /// <summary>Identifies the <see cref="Groups"/> dependency property.</summary>
    public static readonly DependencyProperty GroupsProperty = DependencyProperty.Register(nameof(Groups), typeof(string), typeof(GalleryGroupFilter), new PropertyMetadata(StringBoxes.Empty));
#pragma warning restore WPF0010 // Default value type must match registered type.
}