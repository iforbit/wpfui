// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace Wpf.Ui.Controls;

/// <summary>
/// <see cref="AutomationPeer"/> for <see cref="TwoLineLabel"/>.
/// </summary>
public class TwoLineLabelAutomationPeer : FrameworkElementAutomationPeer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TwoLineLabelAutomationPeer"/> class.
    /// Constructor.
    /// </summary>
    /// <param name="owner">Owner of the AutomationPeer.</param>
    public TwoLineLabelAutomationPeer(TwoLineLabel owner)
        : base(owner)
    {
    }

    /// <summary>
    /// <see cref="AutomationPeer.GetChildrenCore"/>
    /// </summary>
    protected override List<AutomationPeer>? GetChildrenCore()
    {
        return null;
    }

    /// <summary>
    /// <see cref="AutomationPeer.GetAutomationControlTypeCore"/>
    /// </summary>
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Text;
    }

    /// <summary>
    /// <see cref="AutomationPeer.GetClassNameCore"/>
    /// </summary>
    protected override string GetClassNameCore()
    {
        return "TwoLineLabel";
    }

    /// <inheritdoc />
    protected override string? GetNameCore()
    {
        return ((TwoLineLabel)this.Owner).Text;
    }

    /// <summary>
    /// <see cref="AutomationPeer.IsControlElementCore"/>
    /// </summary>
    protected override bool IsControlElementCore()
    {
        // Return false if TwoLineLabel is part of a ControlTemplate, otherwise return the base method
        var tb = (TwoLineLabel)this.Owner;
        DependencyObject? templatedParent = tb.TemplatedParent;

        // If the templatedParent is a ContentPresenter, this TextBlock is generated from a DataTemplate
        if (templatedParent is null
            || templatedParent is ContentPresenter)
        {
            return base.IsControlElementCore();
        }

        return false;
    }
}
