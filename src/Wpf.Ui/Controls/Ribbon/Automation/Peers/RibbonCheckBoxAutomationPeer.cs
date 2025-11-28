// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

public class RibbonCheckBoxAutomationPeer : System.Windows.Automation.Peers.CheckBoxAutomationPeer
{
    /// <summary>Initializes a new instance of the <see cref="RibbonCheckBoxAutomationPeer"/> class.</summary>
    /// <param name="owner">The element associated with this automation peer.</param>
    public RibbonCheckBoxAutomationPeer(RibbonCheckBox owner)
        : base(owner)
    {
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