// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.
//
// Based on Fluent.Ribbon: https://github.com/fluentribbon/Fluent.Ribbon

using System.Windows.Automation;
using System.Windows.Automation.Peers;

namespace Wpf.Ui.Controls;

/// <summary>
/// Automation peer for <see cref="BackstageTabItem"/>.
/// </summary>
public class RibbonBackstageTabItemAutomationPeer : FrameworkElementAutomationPeer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonBackstageTabItemAutomationPeer"/> class.
    /// Creates a new instance.
    /// </summary>
    public RibbonBackstageTabItemAutomationPeer(BackstageTabItem owner)
        : base(owner)
    {
        this.OwningBackstageTabItem = owner;
    }

    private BackstageTabItem OwningBackstageTabItem { get; }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.TabItem;
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return this.Owner.GetType().Name;
    }

    /// <inheritdoc />
    protected override string? GetNameCore()
    {
        var name = AutomationProperties.GetName(this.Owner);

        if (string.IsNullOrEmpty(name))
        {
            name = (this.Owner as IHeaderedControl)?.Header as string;
        }

        return name;
    }

    /// <inheritdoc />
    protected override List<AutomationPeer> GetChildrenCore()
    {
        List<AutomationPeer> children = GetHeaderChildren() ?? new List<AutomationPeer>();

        if (this.OwningBackstageTabItem.IsSelected == false)
        {
            return children;
        }

        if (this.OwningBackstageTabItem.TabControlParent?.SelectedContentHost is not null)
        {
            var contentHostPeer = new FrameworkElementAutomationPeer(this.OwningBackstageTabItem.TabControlParent.SelectedContentHost);
            List<AutomationPeer>? contentChildren = contentHostPeer.GetChildren();

            if (contentChildren is not null)
            {
                children.AddRange(contentChildren);
            }
        }

        return children;

        List<AutomationPeer>? GetHeaderChildren()
        {
            if (this.OwningBackstageTabItem.Header is string)
            {
                return null;
            }

            if (this.OwningBackstageTabItem.HeaderContentHost is not null)
            {
                return new FrameworkElementAutomationPeer(this.OwningBackstageTabItem.HeaderContentHost).GetChildren();
            }

            return null;
        }
    }
}
