// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui;

public interface ILogicalChildSupport
{
    /// <summary>Adds the provided object to the logical tree of this element. </summary>
    /// <param name="child">Child element to be added.</param>
    void AddLogicalChild( object child );

    /// <summary>
    ///     Removes the provided object from this element's logical tree. <see cref="T:System.Windows.FrameworkElement" />
    ///     updates the affected logical tree parent pointers to keep in sync with this deletion.
    /// </summary>
    /// <param name="child">The element to remove.</param>
    void RemoveLogicalChild( object child );
}