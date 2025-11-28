// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace Wpf.Ui.Controls;

/// <summary>
/// Automation peer for <see cref="RibbonGroupBox"/>.
/// </summary>
public class RibbonGroupBoxAutomationPeer : FrameworkElementAutomationPeer, IExpandCollapseProvider, IScrollItemProvider
{
    private RibbonGroupHeaderAutomationPeer? headerPeer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonGroupBoxAutomationPeer"/> class.
    /// Creates a new instance.
    /// </summary>
    public RibbonGroupBoxAutomationPeer(RibbonGroupBox owner)
        : base(owner)
    {
        this.OwningGroup = owner;
    }

    private RibbonGroupBox OwningGroup { get; }

    private RibbonGroupHeaderAutomationPeer? HeaderPeer
    {
        get
        {
            if (this.headerPeer is null
                || !this.headerPeer.Owner.IsDescendantOf(this.OwningGroup))
            {
                if (this.OwningGroup.State == RibbonGroupBoxState.Collapsed)
                {
                    if (this.OwningGroup.CollapsedHeaderContentControl is not null)
                    {
                        this.headerPeer = new RibbonGroupHeaderAutomationPeer(this.OwningGroup.CollapsedHeaderContentControl);
                    }
                }
                else if (this.OwningGroup.Header is not null
                         && this.OwningGroup.HeaderContentControl is not null)
                {
                    this.headerPeer = new RibbonGroupHeaderAutomationPeer(this.OwningGroup.HeaderContentControl);
                }
            }

            return this.headerPeer;
        }
    }

    /// <inheritdoc />
    protected override List<AutomationPeer> GetChildrenCore()
    {
        List<AutomationPeer>? list = base.GetChildrenCore();

        if (this.HeaderPeer is not null)
        {
            if (list is null)
            {
                list = new List<AutomationPeer>(1);
            }

            list.Add(this.HeaderPeer);
        }

        return list;
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return this.Owner.GetType().Name;
    }

    /// <inheritdoc />
    protected override string GetNameCore()
    {
        var name = base.GetNameCore();

        if (string.IsNullOrEmpty(name))
        {
            name = (this.Owner as IHeaderedControl)?.Header as string;
        }

        return name ?? string.Empty;
    }

    /// <inheritdoc />
    public override object GetPattern(PatternInterface patternInterface)
    {
        switch (patternInterface)
        {
            case PatternInterface.ExpandCollapse:
                return this.IsCollapseOrExpandValid ? this : base.GetPattern(patternInterface);

            case PatternInterface.Scroll:
                return base.GetPattern(patternInterface);

            default:
                return base.GetPattern(patternInterface);
        }
    }

    /// <inheritdoc />
    protected override void SetFocusCore()
    {
    }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return this.IsCollapseOrExpandValid
            ? AutomationControlType.Button
            : AutomationControlType.Group;
    }

    /// <inheritdoc />
    public void Expand()
    {
        if (this.IsCollapseOrExpandValid == false)
        {
            return;
        }

        this.OwningGroup.SetCurrentValue(RibbonGroupBox.IsDropDownOpenProperty, true);
    }

    /// <inheritdoc />
    public void Collapse()
    {
        if (this.IsCollapseOrExpandValid == false)
        {
            return;
        }

        this.OwningGroup.SetCurrentValue(RibbonGroupBox.IsDropDownOpenProperty, false);
    }

    /// <inheritdoc />
    public ExpandCollapseState ExpandCollapseState
    {
        get
        {
            if (this.IsCollapseOrExpandValid)
            {
                return ExpandCollapseState.Collapsed;
            }
            else
            {
                return ExpandCollapseState.Expanded;
            }
        }
    }

    private bool IsCollapseOrExpandValid => this.OwningGroup.State is RibbonGroupBoxState.Collapsed or RibbonGroupBoxState.QuickAccess;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    internal void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
    {
        this.RaisePropertyChangedEvent(
            ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
            oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
            newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
    }

    /// <inheritdoc />
    public void ScrollIntoView()
    {
        this.OwningGroup.BringIntoView();
    }
}
