// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Vortice.Direct3D11;

using Wpf.Ui.DirectX.Core;
using Wpf.Ui.DirectX.Models;

namespace Wpf.Ui.DirectX.Rendering;

// SeriesRendererManager: 시리즈-렌더러 매핑 및 일괄 렌더링 처리기
public sealed class SeriesRendererManager
{
    private readonly Dictionary<IGraphSeries, object> _renderers = new();
    private readonly ID3D11Device _device;
    private readonly ID3D11DeviceContext _context;

    public SeriesRendererManager(ID3D11Device device, ID3D11DeviceContext context)
    {
        _device = device;
        _context = context;
    }

    public void AddSeries<T>(GraphSeries<T> series)
        where T : unmanaged
    {
        if (!series.IsReady)
        {
            throw new InvalidOperationException("GraphSeries must be initialized before being added.");
        }

        if (_renderers.ContainsKey(series))
        {
            return;
        }

        _renderers[series] = CreateRenderer(series);
    }

    private SeriesRendererBase<T> CreateRenderer<T>(GraphSeries<T> series)
        where T : unmanaged
    {
        if (series is RealTimeSeries<T> rts)
        {
            var renderer = new RealTimeSeriesRenderer<T>(rts, _device, _context);
            renderer.Initialize(); // ✅ 생성 직후 초기화
            return renderer;
        }

        throw new NotSupportedException($"Unsupported series type: {typeof(T)}");
    }

    public object? GetRenderer(IGraphSeries series)
    {
        return _renderers.TryGetValue(series, out var r) ? r : null;
    }
}