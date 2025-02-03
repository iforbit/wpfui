// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Wpf.Ui.Extensions;

namespace Wpf.Ui.Controls.Ribbon.Automation.Peers;

/// <summary>
/// Automation peer for <see cref="RibbonTabItem"/>.
/// </summary>
public class RibbonTabItemDataAutomationPeer : SelectorItemAutomationPeer, IScrollItemProvider, IExpandCollapseProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonTabItemDataAutomationPeer"/> class.
    /// Creates a new instance.
    /// </summary>
    public RibbonTabItemDataAutomationPeer( object item, RibbonTabControlAutomationPeer tabControlAutomationPeer )
        : base(item, tabControlAutomationPeer)
    {
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return "RibbonTabItem";
    }

    /// <inheritdoc />
    protected override string GetNameCore()
    {
        var nameCore = base.GetNameCore();

        if (string.IsNullOrEmpty(nameCore) == false)
        {
            var wrapper = this.GetWrapper() as RibbonTabItem;
            if (wrapper?.Header is string headerString)
            {
                return headerString;
            }
        }

        return nameCore;
    }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.TabItem;
    }

    /// <summary>
    /// If Ribbon.IsMinimized then set Ribbon.IsDropDownOpen to false
    /// </summary>
    public void Collapse()
    {
        var wrapperTab = this.GetWrapper() as RibbonTabItem;
        if (wrapperTab is not null)
        {
            RibbonTabControl? tabControl = wrapperTab.TabControlParent;
            if (tabControl is not null &&
                tabControl.IsMinimized)
            {
                tabControl.IsDropDownOpen = false;
            }
        }
    }

    /// <summary>
    /// If Ribbon.IsMinimized then set Ribbon.IsDropDownOpen to true
    /// </summary>
    public void Expand()
    {
        var wrapperTab = this.GetWrapper() as RibbonTabItem;

        // Select the tab and display popup
        if (wrapperTab is not null)
        {
            RibbonTabControl? tabControl = wrapperTab.TabControlParent;
            if (tabControl is not null &&
                tabControl.IsMinimized)
            {
                wrapperTab.IsSelected = true;
                tabControl.IsDropDownOpen = true;
            }
        }
    }

    /// <summary>
    /// Gets return Ribbon.IsDropDownOpen
    /// </summary>
    public ExpandCollapseState ExpandCollapseState
    {
        get
        {
            var wrapperTab = this.GetWrapper() as RibbonTabItem;
            if (wrapperTab is not null)
            {
                RibbonTabControl? tabControl = wrapperTab.TabControlParent;
                if (tabControl is not null &&
                    tabControl.IsMinimized)
                {
                    if (wrapperTab.IsSelected && tabControl.IsDropDownOpen)
                    {
                        return ExpandCollapseState.Expanded;
                    }
                    else
                    {
                        return ExpandCollapseState.Collapsed;
                    }
                }
            }

            // When not minimized
            return ExpandCollapseState.Expanded;
        }
    }

    public void ScrollIntoView()
    {
        var wrapperTab = this.GetWrapper() as RibbonTabItem;
        if (wrapperTab is not null)
        {
            wrapperTab.BringIntoView();
        }
    }
}