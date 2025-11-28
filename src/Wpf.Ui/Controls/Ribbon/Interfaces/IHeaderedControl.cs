// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents a control that has a header.
/// </summary>
public interface IHeaderedControl
{
    /// <summary>
    /// Gets or sets the header.
    /// </summary>
    object? Header { get; set; }

    /// <summary>
    ///     Gets or sets headerTemplate is the template used to display the header.
    /// </summary>
    DataTemplate? HeaderTemplate { get; set; }

    /// <summary>
    ///     Gets or sets headerTemplateSelector allows the application writer to provide custom logic
    ///     for choosing the template used to display the header of each item.
    /// </summary>
    /// <remarks>
    ///     This property is ignored if <seealso cref="HeaderTemplate"/> is set.
    /// </remarks>
    DataTemplateSelector? HeaderTemplateSelector { get; set; }
}