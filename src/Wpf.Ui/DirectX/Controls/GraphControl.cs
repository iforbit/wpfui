// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using SharpGen.Runtime;

using System.Diagnostics;
using System.Runtime.InteropServices;

using Wpf.Ui.DirectX.Models;
using Wpf.Ui.DirectX.Rendering;
using Wpf.Ui.DirectX.Services;
using Wpf.Ui.DirectX.Threading;

namespace Wpf.Ui.DirectX.Controls;

public sealed class GraphControl : HwndHost, IRenderStateNotifier
{
    private readonly object _rendererLock = new();
    private readonly List<GraphItemBase> _graphItems = new();
    private readonly TaskCompletionSource _rendererReady = new();

    private IntPtr _hwnd;
    private D3D11Renderer? _renderer;
    private ID3DGraphicsService? _graphicsService;
    private IRenderThreadService? _renderThread;

    private CancellationTokenSource? _resizeToken;
    private bool _disposed;
    private bool _rendererReadyFired = false; // ✅ RendererReady 발생 여부 추적

    public event EventHandler? RendererResetting;

    public event EventHandler? RendererReset;

    public event EventHandler RendererReady;

    public Task WaitForRendererAsync() => _rendererReady.Task;

    public bool IsRendererAvailable =>
     _renderer is not null && !_disposed;

    public bool IsRendererReady =>
        _rendererReadyFired && _renderer != null && _graphItems.Count > 0;

    public float XOffset { get; set; } = 0f;

    public float XScale { get; set; } = 1f;

    public float YScale { get; set; } = 1f;

    public float CurrentXScale { get; private set; } = 1.0f;

    public float CurrentYScale { get; private set; } = 1.0f;

    public GraphControl()
    {
        SizeChanged += OnSizeChanged;
    }

    public void AttachRenderThread(IRenderThreadService renderThread)
    {
        _renderThread = renderThread;
        TryInitializeRenderer();
    }

    public void AddItem(GraphItemBase item)
    {
        if (!_graphItems.Contains(item))
        {
            item.Transform(XOffset, XScale, YScale);
            _graphItems.Add(item);

            if (_renderer is not null)
            {
                _renderer.AddGraphItem(item);
                if (_rendererReadyFired) // ✅ RendererReady 발생 후에만
                {
                    RequestRender();
                }
            }
        }
    }

    public void UpdateTransform(float xOffset, float? xScale = null, float? yScale = null)
    {
        XOffset = xOffset;
        XScale = xScale ?? XScale;
        YScale = yScale ?? YScale;

        _renderer?.SetTransform(XOffset, XScale, YScale);

        foreach (GraphItemBase item in _graphItems)
        {
            item.Transform(XOffset, XScale, YScale);
        }
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        double width = Math.Max(ActualWidth, 1);
        double height = Math.Max(ActualHeight, 1);

        _hwnd = User32Interop.CreateHostWindow(hwndParent.Handle, (int)width, (int)height);
        _graphicsService = new D3DGraphicsService();

        return new HandleRef(this, _hwnd);
    }

    public void RequestRender()
    {
        if (!_rendererReadyFired || _renderer == null || _graphItems.Count == 0 || _disposed)
        {
            Debug.WriteLine("⚠️ RequestRender ignored: renderer not fully ready or no items");
            return;
        }

        _renderThread?.RequestRender();
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        Dispose();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        _resizeToken?.Cancel();
        _resizeToken = new();

        CancellationToken token = _resizeToken.Token;

        _ = Task.Delay(300, token).ContinueWith(
            t =>
            {
                if (!t.IsCanceled)
                {
                    Dispatcher.Invoke(TryInitializeRendererSafely);
                }
            },
            token);
    }

    private void TryInitializeRendererSafely()
    {
        lock (_rendererLock)
        {
            // ✅ 렌더링 중지: 렌더러 재설정 전
            RendererResetting?.Invoke(this, EventArgs.Empty);

            if (_renderThread?.IsRunning == true)
            {
                _renderThread.Stop();
            }

            if (_renderer != null)
            {
                _renderThread?.Unregister(_renderer);
                _renderer.Dispose();
                _renderer = null;
            }

            TryInitializeRenderer();

            if (_renderThread?.IsRunning == false)
            {
                _renderThread.Start();
            }

            RendererReset?.Invoke(this, EventArgs.Empty);
        }
    }

    private void TryInitializeRenderer()
    {
        if (ActualWidth <= 0 || ActualHeight <= 0 || _hwnd == IntPtr.Zero)
        {
            return;
        }

        if (_renderThread == null)
        {
            Debug.WriteLine("⚠️ TryInitializeRenderer: RenderThread is null. Renderer not initialized.");
            return;
        }

        try
        {
            _graphicsService?.Dispose();
            _graphicsService = new D3DGraphicsService();

            var newRenderer = new D3D11Renderer(_graphicsService, _renderThread, _hwnd, ActualWidth, ActualHeight);
            newRenderer.SetTransform(XOffset, XScale, YScale);

            foreach (GraphItemBase item in _graphItems)
            {
                item.Transform(XOffset, XScale, YScale);
                newRenderer.AddGraphItem(item);
            }

            _renderer = newRenderer;
            _renderThread.Register(newRenderer);
            _renderThread.Start();

            // ✅ 명시적 Transform 적용 (초기 값이라도 적용되게)
            _renderer.SetTransform(XOffset, XScale, YScale);
            _ = _rendererReady.TrySetResult();
            _rendererReadyFired = true; // ✅ 준비 완료 상태 반영
            RendererReady?.Invoke(this, EventArgs.Empty);
        }
        catch (SharpGenException ex)
        {
            _ = _rendererReady.TrySetException(ex);
            Debug.WriteLine($"[Renderer Init Failed] {ex.ResultCode} : {ex.Message}");
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            foreach (GraphItemBase item in _graphItems)
            {
                item.Dispose();
            }

            _graphItems.Clear();
            _renderer?.Dispose();
            _renderer = null;
            _graphicsService?.Dispose();
            _graphicsService = null;
            _resizeToken?.Dispose();
        }

        if (_hwnd != IntPtr.Zero)
        {
            User32Interop.DestroyWindow(_hwnd);
            _hwnd = IntPtr.Zero;
        }

        _disposed = true;
        base.Dispose(disposing);
    }
}
