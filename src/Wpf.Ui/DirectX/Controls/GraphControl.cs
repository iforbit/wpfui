// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using SharpGen.Runtime;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;

using Wpf.Ui.DirectX.Models;
using Wpf.Ui.DirectX.Models.VertexTypes;
using Wpf.Ui.DirectX.Rendering;
using Wpf.Ui.DirectX.Services;
using Wpf.Ui.DirectX.Threading;

namespace Wpf.Ui.DirectX.Controls;

public sealed class GraphControl : HwndHost, IRenderStateNotifier
{
    private readonly object _rendererLock = new();
    private readonly List<IGraphItem> _graphItems = new();
    private readonly TaskCompletionSource _rendererReady = new();

    private IntPtr _hwnd;
    private D3D11Renderer? _renderer;
    private ID3DGraphicsService? _graphicsService;
    private IRenderThreadService? _renderThread;
    private DispatcherTimer? _graphicsRetryTimer;

    private CancellationTokenSource? _resizeToken;
    private bool _disposed;
    private bool _rendererReadyFired = false;

    public event EventHandler? RendererResetting;

    public event EventHandler? RendererReset;

    public event EventHandler RendererReady;

    public Task WaitForRendererAsync() => _rendererReady.Task;

    public bool IsRendererAvailable => _renderer is not null && !_disposed;

    public bool IsRendererReady => _rendererReadyFired && _renderer != null && _graphItems.Count > 0;

    public float XOffset { get; set; } = 0f;

    public float XScale { get; set; } = 1f;

    public float YOffset { get; set; } = 0f;

    public float YScale { get; set; } = 1f;

    public float CurrentXScale { get; private set; } = 1.0f;

    public float CurrentYScale { get; private set; } = 1.0f;

    public GraphControl()
    {
        // TryResolveGraphicsService();
        SizeChanged += OnSizeChanged;
    }

    private void TryResolveGraphicsService()
    {
        if (_graphicsService != null)
        {
            return;
        }

        if (Application.Current.Resources.Contains("ServiceProvider") &&
            Application.Current.Resources["ServiceProvider"] is IServiceProvider provider)
        {
            _graphicsService = provider.GetService(typeof(ID3DGraphicsService)) as ID3DGraphicsService;
        }

        if (_graphicsService == null)
        {
            StartRetryGraphicsService();
            Debug.WriteLine($"🧪 call StartRetryGraphicsService");
        }

        Debug.WriteLine($"🧪 call TryResolveGraphicsService");
    }

    private void StartRetryGraphicsService()
    {
        _graphicsRetryTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300), IsEnabled = true };
        _graphicsRetryTimer.Tick += (s, e) =>
        {
            if (_graphicsService == null &&
                Application.Current.Resources.Contains("ServiceProvider") &&
                Application.Current.Resources["ServiceProvider"] is IServiceProvider provider)
            {
                _graphicsService = provider.GetService(typeof(ID3DGraphicsService)) as ID3DGraphicsService;
            }

            if (_graphicsService != null)
            {
                _graphicsRetryTimer?.Stop();
                TryInitializeRenderer();
            }
        };
        _graphicsRetryTimer.Start();
    }

    public void AttachRenderThread(IRenderThreadService renderThread)
    {
        _renderThread = renderThread;
        TryInitializeRenderer();
    }

    public void AddItem(IGraphItem item)
    {
        if (_graphItems.Contains(item))
        {
            return;
        }

        _graphItems.Add(item);
        Debug.WriteLine($"📌 AddItem: {item.Name}");
        if (_renderer is not null)
        {
            _renderer.AddGraphItem(item);
            Debug.WriteLine($"📌 AddGraphItem: {item.Name}");
            if (_rendererReadyFired)
            {
                Debug.WriteLine($"📌 _rendererReadyFired: {item.Name}");
                UpdateTransformFromFirstItem();   // ✅ 자동 Transform 적용
                RequestRender();
            }
        }
    }

    public void UpdateTransform(float xOffset, float? xScale = null, float? yScale = null, float? yOffset = null)
    {
        XOffset = xOffset;
        XScale = xScale ?? XScale;
        YScale = yScale ?? YScale;
        YOffset = yOffset ?? YOffset;

        _renderer?.SetTransform(XOffset, XScale, YScale, YOffset);

        foreach (IGraphItem item in _graphItems)
        {
            item.Transform(XOffset, XScale, YScale);
        }
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        double width = Math.Max(ActualWidth, 1);
        double height = Math.Max(ActualHeight, 1);

        _hwnd = User32Interop.CreateHostWindow(hwndParent.Handle, (int)width, (int)height);
        TryResolveGraphicsService();

        return new HandleRef(this, _hwnd);
    }

    public void RequestRender()
    {
        if (!_rendererReadyFired || _renderer == null || !_renderer.IsReady || _graphItems.Count == 0 || _disposed)
        {
            Debug.WriteLine("⚠️ RequestRender ignored: renderer not fully ready or no items");
            return;
        }

        if (_renderThread == null || !_renderThread.IsRunning)
        {
            Debug.WriteLine("⚠️ RequestRender ignored: no render thread assigned");
            return;
        }

        UpdateTransformFromFirstItem(); // ✅ 렌더 요청 직전에 transform 조정
        _renderThread?.RequestRender();
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        Dispose();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        _resizeToken?.Cancel();
        _resizeToken = new CancellationTokenSource();

        CancellationToken token = _resizeToken.Token;

        _ = Task.Delay(300, token).ContinueWith(
            t =>
        {
            if (!t.IsCanceled)
            {
                Dispatcher.Invoke(TryInitializeRendererSafely);
            }
        }, token);
    }

    private void TryInitializeRendererSafely()
    {
        bool needsResize = _renderer is D3D11Renderer r &&
                           (Math.Abs(r.Width - ActualWidth) > 0.1 || Math.Abs(r.Height - ActualHeight) > 0.1);

        if (_renderer != null && _renderer.IsReady && !needsResize)
        {
            if (!_rendererReadyFired)
            {
                _rendererReadyFired = true;
                _ = _rendererReady.TrySetResult();
                RendererReady?.Invoke(this, EventArgs.Empty);
            }

            RendererReset?.Invoke(this, EventArgs.Empty);
            Debug.WriteLine("⚠️ TryInitializeRendererSafely: renderer is already ready and size unchanged, skipping reinit");
            return;
        }

        lock (_rendererLock)
        {
            RendererResetting?.Invoke(this, EventArgs.Empty);

            if (_renderThread?.IsRunning == true)
            {
                _renderThread.Stop();
                Thread.Sleep(50);
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

            // ✅ 여기 보장적 호출
            if (_renderer?.IsReady == true && !_rendererReadyFired)
            {
                _rendererReadyFired = true;
                _ = _rendererReady.TrySetResult();
                RendererReady?.Invoke(this, EventArgs.Empty);
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

        TryResolveGraphicsService();

        if (_graphicsService == null || _renderThread == null)
        {
            Debug.WriteLine("⚠️ TryInitializeRenderer: GraphicsService or RenderThread not ready.");
            return;
        }

        try
        {
            var newRenderer = new D3D11Renderer(_graphicsService, _renderThread, _hwnd, ActualWidth, ActualHeight);
            newRenderer.SetTransform(XOffset, XScale, YScale);
            _renderer = newRenderer;
            foreach (IGraphItem item in _graphItems)
            {
                GraphItemInitializer.Reinitialize(item, newRenderer.Device, newRenderer.Context, XOffset, XScale, YScale);
                newRenderer.AddGraphItem(item);
            }

            _renderThread.Register(newRenderer);
            _renderThread.Start();

            _renderer.OnRendererReady();
            _renderer.SetTransform(XOffset, XScale, YScale);

            _ = _rendererReady.TrySetResult();
            _rendererReadyFired = true;
            RendererReady?.Invoke(this, EventArgs.Empty);
        }
        catch (SharpGenException ex)
        {
            _ = _rendererReady.TrySetException(ex);
            Debug.WriteLine($"[Renderer Init Failed] {ex.ResultCode} : {ex.Message}");
        }
    }

    private bool _initialTransformApplied = false;

    public void UpdateTransformFromFirstItem(float visibleX = 30f, float visibleY = 1.0f)
    {
        if (_graphItems.Count == 0)
        {
            return;
        }

        if (_graphItems[0] is FastGraphItem<VertexPosition> fp)
        {
            if (fp.LastX <= 1e-3f)
            {
                if (!_initialTransformApplied)
                {
                    Debug.WriteLine("⚠️ No LastX yet, applying default transform");
                    UpdateTransform(0f, 1.0f, 1.0f, 0.0f);
                    _initialTransformApplied = true;
                }

                return;
            }

            _initialTransformApplied = false;

            float xEnd = fp.LastX;

            float xStart = Math.Max(0, xEnd - visibleX);

            // ✅ xOffset: 화면 오른쪽이 xEnd가 되도록 조정
            float xOffset = -xStart;
            float xScale = 1.0f;
            fp.GetYRangeInRange(xStart, xEnd, out float minY, out float maxY);
            float rangeY = Math.Max(1e-3f, maxY - minY);
            float yScale = visibleY / rangeY;
            float yOffset = -minY * yScale;

            UpdateTransform(xOffset, xScale, yScale, yOffset);
            Debug.WriteLine($"🧭 Transform (fixed): xOffset={xOffset:F2}, xScale={xScale:F2}, yOffset={yOffset:F2}, yScale={yScale:F2}");
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
            foreach (IGraphItem item in _graphItems)
            {
                item.Dispose();
            }

            _graphItems.Clear();
            _renderer?.Dispose();
            _renderer = null;
            _graphicsService = null;
            _resizeToken?.Dispose();
            _graphicsRetryTimer?.Stop();
            _graphicsRetryTimer = null;
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
