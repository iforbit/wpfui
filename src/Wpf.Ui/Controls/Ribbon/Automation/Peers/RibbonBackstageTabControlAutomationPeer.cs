// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.
//
// Based on Fluent.Ribbon: https://github.com/fluentribbon/Fluent.Ribbon

using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace Wpf.Ui.Controls;

/// <summary>
///     Automation peer for <see cref="BackstageTabControl" />.
/// </summary>
public sealed class RibbonBackstageTabControlAutomationPeer : SelectorAutomationPeer, ISelectionProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonBackstageTabControlAutomationPeer"/> class.
    ///     Creates a new instance.
    /// </summary>
    public RibbonBackstageTabControlAutomationPeer(BackstageTabControl owner)
        : base(owner)
    {
        this.OwningBackstageTabControl = owner;
    }

    private BackstageTabControl OwningBackstageTabControl { get; }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Tab;
    }

    /// <inheritdoc />
    protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
    {
        return new RibbonControlDataAutomationPeer(item, this);
    }

    bool ISelectionProvider.IsSelectionRequired => true;

    bool ISelectionProvider.CanSelectMultiple => false;

    /// <inheritdoc />
    protected override List<AutomationPeer> GetChildrenCore()
    {
        List<AutomationPeer> baseResult = base.GetChildrenCore() ?? new List<AutomationPeer>();

        if (this.OwningBackstageTabControl.BackButton is { } backButton)
        {
            AutomationPeer? backButtonAutomationPeer = backButton.GetOrCreateAutomationPeer();

            if (backButtonAutomationPeer is not null)
            {
                baseResult.Insert(0, backButtonAutomationPeer);
            }
        }

        return baseResult;
    }
}
