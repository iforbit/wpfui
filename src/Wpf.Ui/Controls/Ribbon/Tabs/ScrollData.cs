// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;

namespace Wpf.Ui.Controls;

/// <summary>
/// Helper class to hold scrolling data.
/// This class exists to reduce working set when SCP is delegating to another implementation of ISI.
/// Standard "extra pointer always for less data sometimes" cache savings model:
/// </summary>
internal class ScrollData
{
    /// <summary>
    /// Gets or sets scroll viewer
    /// </summary>
    internal ScrollViewer? ScrollOwner { get; set; }

    /// <summary>
    /// Gets or sets scroll offset
    /// </summary>
    internal double OffsetX { get; set; }

    /// <summary>
    /// Gets or sets viewportSize is computed from our FinalSize, but may be in different units.
    /// </summary>
    internal double ViewportWidth { get; set; }

    /// <summary>
    /// Gets or sets extent is the total size of our content.
    /// </summary>
    internal double ExtentWidth { get; set; }
}
