// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Reflection;
using System.Windows.Automation.Peers;

namespace Wpf.Ui.Extensions;

/// <summary>
/// Extension methods for <see cref="AutomationPeer"/>.
/// </summary>
internal static class AutomationPeerExtensions
{
    private static readonly MethodInfo? GetWrapperPeerMethodInfo = typeof(ItemAutomationPeer).GetMethod("GetWrapperPeer", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly MethodInfo? GetWrapperMethodInfo = typeof(ItemAutomationPeer).GetMethod("GetWrapper", BindingFlags.Instance | BindingFlags.NonPublic);

    internal static AutomationPeer? GetWrapperPeer(this ItemAutomationPeer automationPeer )
    {
        return (AutomationPeer?)GetWrapperPeerMethodInfo?.Invoke(automationPeer, null);
    }

    internal static UIElement? GetWrapper(this ItemAutomationPeer automationPeer )
    {
        return (UIElement?)GetWrapperMethodInfo?.Invoke(automationPeer, null);
    }
}