// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Vortice.Direct3D11;

using Wpf.Ui.DirectX.Core;
using Wpf.Ui.DirectX.Models;

namespace Wpf.Ui.DirectX.Rendering;

public abstract class SeriesRendererBase<T> : IRenderStyleKeyProvider
    where T : unmanaged
{
    private readonly GraphSeries<T> _series;
    private readonly ID3D11Device _device;
    private readonly ID3D11DeviceContext _context;

    protected GraphSeries<T> Series => _series;

    protected ID3D11Device Device => _device;

    protected ID3D11DeviceContext Context => _context;

    private bool _isInitialized = false;

    protected SeriesRendererBase(GraphSeries<T> series, ID3D11Device device, ID3D11DeviceContext context)
    {
        _series = series ?? throw new ArgumentNullException(nameof(series));
        _device = device ?? throw new ArgumentNullException(nameof(device));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// 반드시 명시적으로 호출할 것. (ex: 생성 직후 renderer.Initialize();)
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        InitializeShaders();
        InitializeBuffers();
        _isInitialized = true;
    }

    protected abstract void InitializeShaders();

    protected abstract void InitializeBuffers();

    public abstract void Upload();

    public abstract void Draw();

    public abstract DrawKey GetDrawKey();
}
