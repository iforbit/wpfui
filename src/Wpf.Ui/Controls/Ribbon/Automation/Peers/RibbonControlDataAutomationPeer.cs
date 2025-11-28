// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.
//
// Based on Fluent.Ribbon: https://github.com/fluentribbon/Fluent.Ribbon

using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace Wpf.Ui.Controls;

/// <summary>
/// Automation peer for ribbon control items.
/// </summary>
public class RibbonControlDataAutomationPeer : ItemAutomationPeer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonControlDataAutomationPeer"/> class.
    /// Creates a new instance.
    /// </summary>
    public RibbonControlDataAutomationPeer(object item, ItemsControlAutomationPeer itemsControlPeer)
        : base(item, itemsControlPeer)
    {
    }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.ListItem;
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        AutomationPeer? wrapperPeer = this.GetWrapperPeer();
        return wrapperPeer?.GetClassName() ?? string.Empty;
    }

    /// <inheritdoc />
    public override object GetPattern(PatternInterface patternInterface)
    {
        // Doesnt implement any patterns of its own, so just forward to the wrapper peer.
        AutomationPeer? wrapperPeer = this.GetWrapperPeer();

        return wrapperPeer?.GetPattern(patternInterface) ?? base.GetPattern(patternInterface);
    }

    private UIElement? GetWrapper()
    {
        ItemsControlAutomationPeer itemsControlAutomationPeer = this.ItemsControlAutomationPeer;

        var owner = (ItemsControl?)itemsControlAutomationPeer?.Owner;
        return owner?.ItemContainerGenerator.ContainerFromItem(this.Item) as UIElement;
    }

    private AutomationPeer? GetWrapperPeer()
    {
        UIElement? wrapper = this.GetWrapper();
        if (wrapper is null)
        {
            return null;
        }

        AutomationPeer? wrapperPeer = UIElementAutomationPeer.CreatePeerForElement(wrapper);
        if (wrapperPeer is not null)
        {
            return wrapperPeer;
        }

        if (wrapper is FrameworkElement element)
        {
            return new FrameworkElementAutomationPeer(element);
        }

        return new UIElementAutomationPeer(wrapper);
    }
}
