// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;

using Wpf.Ui.DirectX.Core;
using Wpf.Ui.DirectX.Models;
using Wpf.Ui.DirectX.Rendering;
using Wpf.Ui.DirectX.Rendering.Interop;
using Wpf.Ui.DirectX.Rendering.Transform;
using Wpf.Ui.DirectX.Services;

namespace Wpf.Ui.DirectX.Controls;

public sealed class GraphControl : HwndHost, IRenderStateNotifier
{
    private readonly List<IGraphSeries> _seriesList = new();
    private SeriesRendererManager? _rendererManager;
    private TransformManager? _transformManager;
    private D3D11Renderer? _renderer;
    private ID3DGraphicsService? _graphicsService;
    private IRenderThreadService? _renderThread;
    private IntPtr _hwnd;

    private DispatcherTimer? _graphicsRetryTimer;
    private CancellationTokenSource? _resizeToken;
    private bool _disposed;
    private bool _isInitialized;

    public IEnumerable<IGraphSeries> Series => _seriesList;

    public event EventHandler? RendererResetting;

    public event EventHandler? RendererReset;

    public event EventHandler RendererReady;

    private readonly bool _rendererReadyFired = false;
    private readonly TaskCompletionSource _rendererReady = new();

    public Task WaitForRendererAsync() => _rendererReady.Task;

    public bool IsRendererReady => _renderer is not null && _renderer.IsReady;

    public TransformManager? TransformManager => _transformManager;

    public GraphControl()
    {
    }

    public void AddSeries<T>(GraphSeries<T> series)
      where T : unmanaged
    {
        _seriesList.Add(series);
        _rendererManager?.AddSeries(series);
    }

    public void AttachRenderThread(IRenderThreadService renderThread)
    {
        _renderThread = renderThread;

        if (!_isInitialized && _hwnd != IntPtr.Zero)
        {
            TryResolveGraphicsService();
            TryInitializeRenderer(); // ✅ 여기서 안전하게 초기화
        }
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

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        double width = Math.Max(ActualWidth, 1);
        double height = Math.Max(ActualHeight, 1);

        _hwnd = User32Interop.CreateHostWindow(hwndParent.Handle, (int)width, (int)height);
        return new HandleRef(this, _hwnd);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        Dispose();
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

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        _resizeToken?.Cancel();
        _resizeToken = new CancellationTokenSource();

        CancellationToken token = _resizeToken.Token;

        _ = Task.Delay(200, token).ContinueWith(
            t =>
        {
            if (!t.IsCanceled)
            {
                Dispatcher.Invoke(() => TryResizeRenderer());
            }
        },
            token);
    }

    private void TryResizeRenderer()
    {
        if (_renderer is not null)
        {
            _renderer.Resize((int)ActualWidth, (int)ActualHeight);
        }
    }

    private void TryInitializeRenderer()
    {
        if (_isInitialized || _hwnd == IntPtr.Zero)
        {
            return;
        }

        if (_graphicsService == null || _renderThread == null)
        {
            Debug.WriteLine("⚠️ TryInitializeRenderer: GraphicsService or RenderThread not ready.");
            return;
        }

        _rendererManager = new SeriesRendererManager(_graphicsService.Device, _graphicsService.Context);

        // ✅ TransformManager 생성
        _transformManager = new TransformManager(_graphicsService.Device);

        _renderer = new D3D11Renderer(
         _graphicsService,
         _renderThread,
         _hwnd,
         800,
         600,
         _seriesList,
         _rendererManager,
         _transformManager);

        _renderThread.Register(_renderer);
        _isInitialized = true;
    }

    /// <summary>
    /// 외부에서 Transform 변경 요청 (스크롤, 줌 등)
    /// </summary>
    public void UpdateTransform(float xOffset, float xScale, float yScale, float yOffset = 0f)
    {
        if (_transformManager is null)
        {
            return;
        }

        _transformManager.IsUserControlled = true;
        _transformManager.SetTransform(xOffset, xScale, yScale, yOffset);
    }

    public void FollowLatestX(float latestX, float visibleRange = 30f)
    {
        if (_transformManager is null || _graphicsService?.Context is null)
        {
            return;
        }

        _transformManager.FollowLatestX(_graphicsService.Context, latestX, visibleRange);
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            foreach (IGraphSeries item in _seriesList)
            {
                item.Dispose();
            }

            _seriesList.Clear();
            _renderer?.Dispose();
            _renderer = null;
            _rendererManager = null;
            _transformManager?.Dispose();
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
