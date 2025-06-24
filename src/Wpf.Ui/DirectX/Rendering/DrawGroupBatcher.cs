// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.DirectX.Core;
using Wpf.Ui.DirectX.Models;

namespace Wpf.Ui.DirectX.Rendering;

// DrawGroupBatcher: 동일 스타일 시리즈를 그룹화하여 DrawCall 병합 수행
public sealed class DrawGroupBatcher
{
    private readonly Dictionary<DrawKey, List<object>> _groups = new();

    public void Clear() => _groups.Clear();

    public void Add<T>(GraphSeries<T> series, SeriesRendererBase<T> renderer)
        where T : unmanaged
    {
        DrawKey key = renderer.GetDrawKey();
        if (!_groups.TryGetValue(key, out List<object>? list))
        {
            list = new List<object>();
            _groups[key] = list;
        }

        list.Add(renderer);
    }

    public void Execute()
    {
        foreach (KeyValuePair<DrawKey, List<object>> group in _groups)
        {
            foreach (var rendererObj in group.Value)
            {
                dynamic renderer = rendererObj;
                renderer.Upload();
                renderer.Draw();
            }
        }
    }
}
