// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Diagnostics;

using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

using Wpf.Ui.DirectX.Core;
using Wpf.Ui.DirectX.Rendering.Transform;
using Wpf.Ui.DirectX.Services;

namespace Wpf.Ui.DirectX.Rendering;

public sealed class D3D11Renderer : IRenderable, IDisposable
{
    private readonly ID3DGraphicsService _graphicsService;
    private readonly IRenderThreadService _renderThread;
    private readonly IReadOnlyList<IGraphSeries> _seriesList;
    private readonly SeriesRendererManager _rendererManager;
    private readonly TransformManager _transformManager;
    private readonly ID3D11Device _device;
    private readonly ID3D11DeviceContext _context;

    private readonly IntPtr _hwnd;
    private readonly int _width;
    private readonly int _height;

    private readonly TimeSpan _minReinitInterval = TimeSpan.FromSeconds(5);

    private ID3D11RenderTargetView? _renderTargetView;
    private IDXGISwapChain? _swapChain;

    private bool _disposed;

    public bool IsReady => _device.NativePointer != IntPtr.Zero && _context.NativePointer != IntPtr.Zero;

    public double Width => _width;

    public double Height => _height;

    public ID3D11Device Device => _device;

    public ID3D11DeviceContext Context => _context;

    // 생성자: 최초 한 번만 호출, 재초기화는 별도 메서드로 처리
    public D3D11Renderer(
        ID3DGraphicsService graphicsService,
        IRenderThreadService renderThread,
        IntPtr hwnd,
        int width,
        int height,
        IReadOnlyList<IGraphSeries> seriesList,
        SeriesRendererManager rendererManager,
        TransformManager transformManager)
    {
        _graphicsService = graphicsService;
        _renderThread = renderThread;
        _hwnd = hwnd;
        _width = width;
        _height = height;
        _seriesList = seriesList;
        _rendererManager = rendererManager;
        _transformManager = transformManager;

        _device = _graphicsService.Device;
        _context = _graphicsService.Context;

        InitializeResources();
    }

    private void InitializeResources()
    {
        DisposeResources(); // ✅ 항상 기존 리소스 정리 후 시작

        _swapChain = ((IDXGIFactory1)_graphicsService.Factory).CreateSwapChain(_device, new SwapChainDescription
        {
            BufferDescription = new ModeDescription((uint)_width, (uint)_height, new Rational(60, 1), Format.B8G8R8A8_UNorm),
            SampleDescription = new SampleDescription(1, 0),
            BufferUsage = Usage.RenderTargetOutput,
            BufferCount = 1,
            OutputWindow = _hwnd,
            Windowed = true,
            SwapEffect = SwapEffect.Discard,
            Flags = SwapChainFlags.None
        });

        using ID3D11Texture2D backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0);
        _renderTargetView = _device.CreateRenderTargetView(backBuffer);

        Debug.WriteLine("✅ Renderer resources initialized.");
    }

    private void DisposeResources()
    {
        static void SafeDispose<T>(ref T? resource)
            where T : class, IDisposable
        {
            resource?.Dispose();
            resource = null;
        }

        SafeDispose(ref _renderTargetView);
        SafeDispose(ref _swapChain);

        Debug.WriteLine("🧹 All GPU resources disposed.");
    }

    public void RenderFrame(float time)
    {
        if (_disposed || !IsReady)
        {
            return;
        }

        _context.OMSetRenderTargets(_renderTargetView);
        _context.ClearRenderTargetView(_renderTargetView, new Color4(1, 1, 1, 1));
        _context.RSSetViewport(new Viewport(0, 0, (int)_width, (int)_height, 0, 1));

        _transformManager.ApplyToContext(_context);
        var batcher = new DrawGroupBatcher();
        IGraphSeries[] snapshot = _seriesList.ToArray();
        foreach (IGraphSeries? series in snapshot)
        {
            if (!series.IsVisible || !series.IsReady)
            {
                continue;
            }

            var renderer = _rendererManager.GetRenderer(series);
            if (renderer is IRenderStyleKeyProvider)
            {
                batcher.Add((dynamic)series, (dynamic)renderer);
            }
        }

        batcher.Execute();

        try
        {
            _ = _swapChain?.Present(0, PresentFlags.None);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Present failed: {ex.Message}");
        }
    }

    public void SetTransform(float xScale, float yScale, float xOffset, float yOffset)
    {
        _transformManager.SetTransform(xScale, yScale, xOffset, yOffset);
    }

    private float GetLatestX()
    {
        float latestX = float.MinValue;

        foreach (IGraphSeries series in _seriesList)
        {
            if (!series.IsReady || !series.IsVisible)
            {
                continue;
            }

            latestX = Math.Max(latestX, series.LastX);
        }

        return latestX;
    }

    public void Resize(int width, int height)
    {
        DisposeResources();
        InitializeResources();
    }

    public bool IsContextValid()
    {
        try
        {
            if (_context == null || _context.NativePointer == IntPtr.Zero || _disposed)
            {
                Debug.WriteLine("❌ Invalid context");
            }

            // Context가 여전히 유효한지 확인
            return _context != null &&
                   _context.NativePointer != IntPtr.Zero &&
                   !_disposed;
        }
        catch
        {
            return false;
        }
    }

    public bool IsDeviceValid()
    {
        try
        {
            return _device != null &&
                   _device.NativePointer != IntPtr.Zero &&
                   !_disposed;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _renderThread.Unregister(this);
        DisposeResources();
    }
}