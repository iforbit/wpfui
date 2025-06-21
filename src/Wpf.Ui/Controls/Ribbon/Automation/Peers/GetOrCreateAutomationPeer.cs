// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Automation.Peers;

namespace Wpf.Ui.Controls.Automation.Peers;

internal static class UIElementExtensions
{
    public static AutomationPeer? GetOrCreateAutomationPeer(this UIElement element)
    {
        return UIElementAutomationPeer.FromElement(element)
               ?? UIElementAutomationPeer.CreatePeerForElement(element);
    }
}