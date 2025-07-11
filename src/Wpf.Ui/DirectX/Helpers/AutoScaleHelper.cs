// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.DirectX.Helpers;

public static class AutoScaleHelper
{
    public static float EstimateXScale(IReadOnlyList<float> xs)
    {
        if (xs.Count < 2)
        {
            return 1.0f;
        }

        float avgDx = (xs[^1] - xs[0]) / (xs.Count - 1);
        float scale = 1.0f / avgDx;

        return Math.Clamp(scale, 0.01f, 100f); // 확대/축소 안정 범위
    }
}