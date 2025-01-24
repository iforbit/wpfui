// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf.Ui.Internal.KnowBoxes;

/// <summary>
/// Class containing boxed values for <see cref="double"/>.
/// </summary>
internal static class DoubleBoxes
{
    /// <summary>
    /// Gets a boxed value for <c>0D</c>.
    /// </summary>
    internal static readonly object Zero = 0D;

    /// <summary>
    /// Gets a boxed value for <see cref="double.NaN"/>.
    /// </summary>
    internal static readonly object NaN = double.NaN;

    /// <summary>
    /// Gets a boxed value for <see cref="double.MaxValue"/>.
    /// </summary>
    internal static readonly object MaxValue = double.MaxValue;

    /// <summary>
    /// Gets a boxed value for <c>1D</c>.
    /// </summary>
    internal static readonly object One = 1D;
}