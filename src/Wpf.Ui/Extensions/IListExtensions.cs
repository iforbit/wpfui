// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections;

namespace Wpf.Ui.Extensions;
internal static class IListExtensions
{
    private static readonly IList EmptyReadOnlyList = ArrayList.ReadOnly(new ArrayList());

    public static IList NullSafe( this IList? list )
    {
        return list ?? EmptyReadOnlyList;
    }
}