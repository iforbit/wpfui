// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace Wpf.Ui.Controls.Ribbon.Automation.Peers;

/// <summary>
/// Automation peer for <see cref="RibbonDropDownButton"/>.
/// </summary>
public class RibbonDropDownButtonAutomationPeer : RibbonHeaderedControlAutomationPeer, IExpandCollapseProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonDropDownButtonAutomationPeer"/> class.
    /// Creates a new instance.
    /// </summary>
    public RibbonDropDownButtonAutomationPeer(RibbonDropDownButton owner )
        : base(owner)
    {
        this.OwnerDropDownButton = owner;
    }

    private RibbonDropDownButton OwnerDropDownButton { get; }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return this.Owner.GetType().Name;
    }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Custom;
    }

    /// <inheritdoc />
    protected override string GetLocalizedControlTypeCore()
    {
        return this.Owner.GetType().Name;
    }

    /// <inheritdoc />
    public override object GetPattern(PatternInterface patternInterface )
    {
        switch (patternInterface)
        {
            case PatternInterface.ExpandCollapse:
                return this;
        }

        return base.GetPattern(patternInterface);
    }

    /// <inheritdoc />
    public void Collapse()
    {
        this.OwnerDropDownButton.IsDropDownOpen = false;
    }

    /// <inheritdoc />
    public void Expand()
    {
        this.OwnerDropDownButton.IsDropDownOpen = true;
    }

    /// <inheritdoc />
    ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState => this.OwnerDropDownButton.IsDropDownOpen == false ? ExpandCollapseState.Collapsed : ExpandCollapseState.Expanded;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    internal void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue )
    {
        this.RaisePropertyChangedEvent(
            ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
            oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
            newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
    }
}