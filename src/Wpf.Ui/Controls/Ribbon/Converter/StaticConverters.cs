// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls.Converter;

/// <summary>
/// Hold static instances of several commonly used converters.
/// </summary>
public static class StaticConverters
{
    /// <summary>
    /// Get a static instance of <see cref="ThicknessConverter"/>
    /// </summary>
    public static readonly ThicknessConverter ThicknessConverter = new();

    /// <summary>
    /// Get a static instance of <see cref="CornerRadiusConverter"/>
    /// </summary>
    public static readonly CornerRadiusConverter CornerRadiusConverter = new();
}
