// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Automation.Peers;

namespace Wpf.Ui.Controls;

/// <summary>
/// Base automation peer for <see cref="IHeaderedControl"/>.
/// </summary>
public abstract class RibbonHeaderedControlAutomationPeer : FrameworkElementAutomationPeer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonHeaderedControlAutomationPeer"/> class.
    /// Creates a new instance.
    /// </summary>
    protected RibbonHeaderedControlAutomationPeer(FrameworkElement owner)
        : base(owner)
    {
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return this.Owner.GetType().Name;
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
