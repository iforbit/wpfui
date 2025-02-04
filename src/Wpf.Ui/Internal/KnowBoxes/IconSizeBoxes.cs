// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Internal.KnowBoxes;

/// <summary>
/// Class containing boxed values for <see cref="IconSize"/>.
/// </summary>
public static class IconSizeBoxes
{
    /// <summary>
    /// Gets a boxed value for <see cref="IconSize.Small"/>.
    /// </summary>
    public static readonly object Small = IconSize.Small;

    /// <summary>
    /// Gets a boxed value for <see cref="IconSize.Medium"/>.
    /// </summary>
    public static readonly object Medium = IconSize.Medium;

    /// <summary>
    /// Gets a boxed value for <see cref="IconSize.Large"/>.
    /// </summary>
    public static readonly object Large = IconSize.Large;

    /// <summary>
    /// Gets a boxed value for <see cref="IconSize.Custom"/>.
    /// </summary>
    public static readonly object Custom = IconSize.Custom;

    /// <summary>
    /// Gets a boxed value for <see cref="IconSize"/>.
    /// </summary>
    public static object Box(IconSize iconSize )
    {
        return iconSize switch
        {
            IconSize.Small => Small,
            IconSize.Medium => Medium,
            IconSize.Large => Large,
            IconSize.Custom => Custom,
            _ => throw new ArgumentOutOfRangeException(nameof(iconSize), iconSize, null)
        };
    }
}