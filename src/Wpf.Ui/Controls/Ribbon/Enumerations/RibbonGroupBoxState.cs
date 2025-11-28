// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

public enum RibbonGroupBoxState
{
    /// <summary>
    /// Large. All controls in the group will try to be large size
    /// </summary>
    Large = 0,

    /// <summary>
    /// Middle. All controls in the group will try to be middle size
    /// </summary>
    Middle,

    /// <summary>
    /// Small. All controls in the group will try to be small size
    /// </summary>
    Small,

    /// <summary>
    /// Collapsed. Group will collapse its content in a single button
    /// </summary>
    Collapsed,

    /// <summary>
    /// QuickAccess. Group will collapse its content in a single button in quick access toolbar
    /// </summary>
    QuickAccess
}