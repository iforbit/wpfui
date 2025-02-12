// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Automation.Peers;

namespace Wpf.Ui.Controls.Automation.Peers;

/// <inheritdoc />
public class RibbonButtonAutomationPeer : ButtonAutomationPeer
{
    /// <summary>Initializes a new instance of the <see cref="RibbonButtonAutomationPeer"/> class.</summary>
    /// <param name="owner">The element associated with this automation peer.</param>
    public RibbonButtonAutomationPeer(System.Windows.Controls.Button owner)
        : base(owner)
    {
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return "RibbonButton";
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