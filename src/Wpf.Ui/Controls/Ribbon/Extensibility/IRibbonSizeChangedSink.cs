// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls.Ribbon.Extensibility;

/// <summary>
/// Interface which is used to signal size changes
/// </summary>
public interface IRibbonSizeChangedSink
{
    /// <summary>
    /// Called when the size is changed
    /// </summary>
    /// <param name="previous">Size before change</param>
    /// <param name="current">Size after change</param>
    void OnSizePropertyChanged(RibbonControlSize previous, RibbonControlSize current );
}