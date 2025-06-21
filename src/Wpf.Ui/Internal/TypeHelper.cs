// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Internal;

internal static class TypeHelper
{
    public static bool InheritsFrom(this Type type, string typeName)
    {
        Type? currentType = type;

        do
        {
            if (currentType.Name == typeName)
            {
                return true;
            }

            currentType = currentType.BaseType;
        }
        while (currentType is not null
               && currentType != typeof(object));

        return false;
    }
}