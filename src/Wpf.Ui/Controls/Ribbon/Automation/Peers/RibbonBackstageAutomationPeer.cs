// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.
//
// Based on Fluent.Ribbon: https://github.com/fluentribbon/Fluent.Ribbon

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace Wpf.Ui.Controls;

/// <summary>
/// Automation peer for <see cref="Backstage"/>.
/// </summary>
public sealed class RibbonBackstageAutomationPeer : RibbonControlAutomationPeer, IExpandCollapseProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonBackstageAutomationPeer"/> class.
    /// Creates a new instance.
    /// </summary>
    public RibbonBackstageAutomationPeer(Backstage owner)
        : base(owner)
    {
        this.OwningBackstage = owner;
    }

    private Backstage OwningBackstage { get; }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Menu;
    }

    /// <inheritdoc />
    public override object GetPattern(PatternInterface patternInterface)
    {
        switch (patternInterface)
        {
            case PatternInterface.ExpandCollapse:
                return this;
        }

        return base.GetPattern(patternInterface);
    }

    /// <inheritdoc />
    protected override List<AutomationPeer> GetChildrenCore()
    {
        var children = new List<AutomationPeer>();

        if (this.OwningBackstage.Content is not null)
        {
            AutomationPeer? automationPeer = CreatePeerForElement(this.OwningBackstage.Content);

            if (automationPeer is not null)
            {
                children.Add(automationPeer);
            }
        }

        return children;
    }

    /// <inheritdoc />
    public void Collapse()
    {
        this.OwningBackstage.SetIsOpen(false);
    }

    /// <inheritdoc />
    public void Expand()
    {
        this.OwningBackstage.SetIsOpen(true);
    }

    /// <inheritdoc />
    ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState => this.OwningBackstage.IsOpen == false ? ExpandCollapseState.Collapsed : ExpandCollapseState.Expanded;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    internal void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
    {
        this.RaisePropertyChangedEvent(
            ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
            oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
            newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
    }
}
