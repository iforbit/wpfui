// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace Wpf.Ui.Controls.Ribbon.Automation.Peers;

/// <summary>
/// Automation peer for <see cref="RibbonTitleBar"/>.
/// </summary>
public class RibbonTitleBarAutomationPeer : FrameworkElementAutomationPeer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonTitleBarAutomationPeer"/> class.
    /// Creates a new instance.
    /// </summary>
    public RibbonTitleBarAutomationPeer( RibbonTitleBar owner )
        : base(owner)
    {
    }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Header;
    }

    /// <inheritdoc />
    protected override bool IsContentElementCore()
    {
        return false;
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return this.Owner.GetType().Name;
    }

    /// <inheritdoc />
    protected override string? GetNameCore()
    {
        var contentPresenter = this.Owner as HeaderedContentControl;

        if (contentPresenter?.Header is not null)
        {
            return contentPresenter.Header.ToString();
        }

        return base.GetNameCore();
    }
}
