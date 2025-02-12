// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Reflection;
using System.Windows.Controls;

namespace Wpf.Ui.Internal;
internal static class AccessTextHelper
{
    private static readonly MethodInfo? RemoveAccessKeyMarkerMethodInfo = typeof(AccessText).GetMethod("RemoveAccessKeyMarker", BindingFlags.Static | BindingFlags.NonPublic);

    public static string? RemoveAccessKeyMarker(string? input)
    {
        if (input is null)
        {
            return null;
        }

        return (string?)RemoveAccessKeyMarkerMethodInfo?.Invoke(null, new object[] { input });
    }
}