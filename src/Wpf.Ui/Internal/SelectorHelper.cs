// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Reflection;
using System.Windows.Controls.Primitives;

namespace Wpf.Ui.Internal;
internal static class SelectorHelper
{
    private static readonly PropertyInfo? canSelectMultiplePropertyInfo = typeof(Selector).GetProperty("CanSelectMultiple", BindingFlags.Instance | BindingFlags.NonPublic);

    public static void SetCanSelectMultiple( Selector selector, bool value )
    {
        if (canSelectMultiplePropertyInfo is null)
        {
            throw new MissingMemberException(nameof(Selector), "CanSelectMultiple");
        }

        canSelectMultiplePropertyInfo.SetValue(selector, value);
    }
}