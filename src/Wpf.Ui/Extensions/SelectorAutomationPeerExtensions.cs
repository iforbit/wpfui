// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Reflection;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace Wpf.Ui.Extensions;

/// <summary>
/// Extensions for <see cref="SelectorAutomationPeer"/>.
/// </summary>
public static class SelectorAutomationPeerExtensions
{
    private static readonly MethodInfo? RaiseSelectionEventsMethodInfo = typeof(SelectorAutomationPeer).GetMethod("RaiseSelectionEvents", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    /// <summary>
    /// Calls the internal method "RaiseSelectionEvents" on <paramref name="peer"/> and passes <paramref name="e"/> to it.
    /// </summary>
    public static void RaiseSelectionEvents(this SelectorAutomationPeer peer, SelectionChangedEventArgs e)
    {
        _ = RaiseSelectionEventsMethodInfo?.Invoke(peer, new object[] { e });
    }
}