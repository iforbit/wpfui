// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls.Primitives;

namespace Wpf.Ui.Controls;

public class RibbonToggleButtonAutomationPeer : System.Windows.Automation.Peers.ToggleButtonAutomationPeer
{
    /// <summary>Initializes a new instance of the <see cref="RibbonToggleButtonAutomationPeer"/> class.</summary>
    /// <param name="owner">The element associated with this automation peer.</param>
    public RibbonToggleButtonAutomationPeer(ToggleButton owner)
        : base(owner)
    {
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return "RibbonToggleButton";
    }

    /// <inheritdoc />
    protected override string? GetNameCore()
    {
        var name = base.GetNameCore();

        if (string.IsNullOrEmpty(name))
        {
            name = (this.Owner as IHeaderedControl)?.Header as string;
        }

        return name;
    }
}
