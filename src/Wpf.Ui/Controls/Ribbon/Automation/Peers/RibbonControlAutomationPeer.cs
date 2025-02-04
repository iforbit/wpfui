// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Automation.Peers;
using WPFRibbonControl = Wpf.Ui.Controls.RibbonControl;

namespace Wpf.Ui.Controls.Automation.Peers;

/// <summary>
/// Automation peer for <see cref="RibbonControl" />.
/// </summary>
public class RibbonControlAutomationPeer : FrameworkElementAutomationPeer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonControlAutomationPeer"/> class.
    /// Creates a new instance.
    /// </summary>
    public RibbonControlAutomationPeer(WPFRibbonControl owner)
        : base(owner)
    {
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return this.Owner.GetType().Name;
    }
}
