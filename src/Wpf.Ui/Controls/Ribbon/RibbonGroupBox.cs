// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;

namespace Wpf.Ui.Controls.Ribbon;

public class RibbonGroupBox : HeaderedContentControl
{
    static RibbonGroupBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonGroupBox), new FrameworkPropertyMetadata(typeof(RibbonGroupBox)));
    }
}
