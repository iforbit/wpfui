// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;

using Wpf.Ui.DirectX.Core;
using Wpf.Ui.DirectX.Models;
using Wpf.Ui.DirectX.Models.VertexTypes;
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

    public bool AutoScaleEnabled { get; set; } = true;

    public float VisibleMin { get; private set; } = 0f;

    public float VisibleMax { get; private set; } = 30f;

    public float VisibleRange => MathF.Max(VisibleMax - VisibleMin, 100f); // 최소 100ms

    public float ViewX
    {
        get => VisibleMin;
        set => SetVisibleRange(value, value + VisibleRange);
    }

    public float ViewCenter
    {
        get => VisibleMin + (VisibleRange * 0.5f);
        set => SetVisibleRange(value - (VisibleRange * 0.5f), value + (VisibleRange * 0.5f));
    }

    public float MinX => _seriesList.Count > 0
       ? MathF.Min(_seriesList.OfType<IGraphSeries>().Min(s => s.MinX), 0f)
       : 0f;

    public float MaxX => _seriesList.Count > 0
        ? MathF.Max(_seriesList.OfType<IGraphSeries>().Max(s => s.MaxX), 100f)
        : 100f;

    public float FullRangeX => MaxX - MinX;

    public float LastX => _seriesList.OfType<IGraphSeries>().Max(s => s.LastX);

    public bool LockYScale { get; set; } = false;

    public bool EnableAutoScroll { get; set; } = false;

    public float AutoScrollThreshold { get; set; } = 0.5f; // View 끝에서 50% 이내일 때 자동 이동 시작

    private bool _autoScrollEngaged = false;
    private readonly float _defaultViewRange = 5f;

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
    /// xOffset, xScale, yScale, yOffset 수동 지정
    /// </summary>
    public void UpdateTransform(float xOffset, float xScale, float yScale, float yOffset = 0f)
    {
        _transformManager?.SetTransform(xOffset, xScale, yScale, yOffset);
    }

    /// <summary>
    /// 내부에서 상태값 기반으로 Transform 재계산
    /// VisibleMin, VisibleRange 상태를 이용해 계산
    /// </summary>
    private void UpdateViewTransform()
    {
        float xScale = 2f / VisibleRange;
        float xOffset = VisibleMin;

        float yScale = TransformManager?.YScale ?? 1f;
        float yOffset = TransformManager?.YOffset ?? 0f;

        TransformManager?.SetTransform(xOffset, xScale, yScale, yOffset);
    }

    public void ApplyTransformFromView()
    {
        float xScale = 2f / VisibleRange;
        float xOffset = VisibleMin;

        float yScale = TransformManager?.YScale ?? 1f;
        float yOffset = TransformManager?.YOffset ?? 0f;

        TransformManager?.SetTransform(xOffset, xScale, yScale, yOffset);
    }

    // 스크롤 시점 조정
    public void SetVisibleRange(float min, float max)
    {
        if (min >= max)
        {
            return;
        }

        float dataMin = MinX;
        float dataMax = MaxX;
        float range = max - min;

        if (min < dataMin)
        {
            min = dataMin;
            max = min + range;
        }

        if (max > dataMax)
        {
            max = dataMax;
            min = max - range;
        }

        VisibleMin = min;
        VisibleMax = max;
        UpdateViewTransform();
    }

    public void UpdateAutoScrollLogic()
    {
        float latestX = LastX;

        if (!_autoScrollEngaged)
        {
            if (latestX >= _defaultViewRange)
            {
                _autoScrollEngaged = true;
            }
            else
            {
                SetVisibleRange(0f, _defaultViewRange);
                return;
            }
        }

        if (EnableAutoScroll)
        {
            float viewStart = latestX - VisibleRange;
            SetVisibleRange(viewStart, latestX);
        }
    }

    public void UpdateViewX(float viewX) => SetVisibleRange(viewX, viewX + VisibleRange);

    public void FollowLatestX()
    {
        float latestX = LastX;
        float range = MathF.Max(VisibleRange, 30f); // ✅ 최소 30 이상 유지
        float viewStartX = Math.Max(latestX - range, 0f);
        SetVisibleRange(viewStartX, viewStartX + range);
    }

    public void AutoAdjustTransform()
    {
        const int RecentCount = 500;
        const float MinVisibleRange = 100f;
        const float MaxVisibleRange = 20000f;
        const float MinYRange = 2.0f;

        RealTimeSeries<VertexPosition>? series = _seriesList
            .OfType<RealTimeSeries<VertexPosition>>()
            .FirstOrDefault(s => s.IsReady && s.IsVisible);

        if (series is null || series.TotalVertexCount < 2)
        {
            Debug.WriteLine("🛑 [AutoAdjust] Not enough data");
            return;
        }

        ReadOnlySpan<VertexPosition> span = series.GetSpanUnsafe();
        int count = Math.Min(RecentCount, span.Length);

        float x0 = span[^count].Position.X;
        float x1 = span[^1].Position.X;
        float avgDx = (x1 - x0) / Math.Max(count - 1, 1);

        float visibleRange = Math.Clamp(avgDx * count, MinVisibleRange, MaxVisibleRange);
        float latestX = series.LastX;

        float xOffset = TransformManager.XOffset;
        float xScale = TransformManager.XScale;

        // ✅ AutoScroll이 켜진 경우에만 X축 조정
        if (EnableAutoScroll)
        {
            xScale = 2f / visibleRange;
            SetVisibleRange(xOffset, xOffset + visibleRange); // ✅ View 상태 갱신
        }

        xOffset = MathF.Max(latestX - visibleRange, 0f);

        // 🧭 Y 범위 계산
        series.GetRecentYRange(RecentCount, out float minY, out float maxY);
        float dy = MathF.Max(maxY - minY, MinYRange);
        float yCenter = (minY + maxY) * 0.5f;
        float yScale = 1.0f / dy;

        if (LockYScale)
        {
            yScale = TransformManager?.YScale ?? 1f;
            yCenter = TransformManager?.YOffset ?? 0f;
        }

        const float EPSILON_XOFFSET = 1f;
        const float EPSILON_SCALE = 0.0001f;

        if (Math.Abs(xOffset - TransformManager.XOffset) < EPSILON_XOFFSET &&
            Math.Abs(xScale - TransformManager.XScale) < EPSILON_SCALE &&
            Math.Abs(yScale - TransformManager.YScale) < EPSILON_SCALE &&
            Math.Abs(yCenter - TransformManager.YOffset) < EPSILON_SCALE)
        {
            return;
        }

        TransformManager?.SetTransform(xOffset, xScale, yScale, yCenter);
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
