// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Buffers;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;

using Vortice.Mathematics;

using Wpf.Ui.DirectX.Controls;
using Wpf.Ui.DirectX.Models;
using Wpf.Ui.DirectX.Models.VertexTypes;
using Wpf.Ui.DirectX.Threading;

namespace Wpf.Ui.Demo.Mvvm.ViewModels;

public partial class DashboardViewModel : ViewModel, IDisposable
{
    private bool _isInitialized = false;

    private GraphControl? _graphControl;
    private readonly IRenderThreadService? _renderThread;

    private readonly Dictionary<string, ChunkedVertexBuffer<VertexPositionColor>> _buffers = new();

    private readonly Dictionary<string, FastGraphItem<VertexPosition>> _fastItems = new();
    private readonly Dictionary<string, GraphLineItem<VertexPositionColor>> _lineItems = new();

    private readonly Dictionary<string, float> _channelTime = new();
    private readonly string[] _channels = ["CH1", "CH2", "CH3"];

    private const double XStep = 0.02f; // 20Hz 샘플링 간격처럼 동작

    private readonly DispatcherTimer _renderTimer = new();

    private readonly System.Timers.Timer _dataTimer = new(5);
    private readonly object _generateVerticesLock = new();

    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    [ObservableProperty]
    private bool _isRendering;

    private object? _currentTab;
    public object? CurrentTab
    {
        get => _currentTab;
        set => SetProperty(ref _currentTab, value);
    }
    public DashboardViewModel(IRenderThreadService renderThread)
    {
        _renderThread = renderThread;

        foreach (string ch in _channels)
        {
            _channelTime[ch] = 0f;
        }

        _dataTimer.Elapsed += (_, _) =>
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_generateVerticesLock, ref lockTaken);
                if (!lockTaken)
                    return;

                //GenerateVerticesLegacy();
                GenerateVertices();
              
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"💥 InvalidOperationException: {ex.Message}\n{ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 DataTimer error: {ex}");
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(_generateVerticesLock);
            }
        };

        _renderTimer.Interval = TimeSpan.FromMilliseconds(16);
        _renderTimer.Tick += (_, _) =>
        {
            if (_graphControl?.IsRendererReady == true && IsRendering)
            {
                //OnRenderFrameLegacy((float)_stopwatch.Elapsed.TotalSeconds);
                //OnRenderFrame((float)_stopwatch.Elapsed.TotalSeconds);
                _graphControl.RequestRender();
            }
        };
    }

    public void SetGraphControl(GraphControl control)
    {
        _graphControl = control;
     

        // Renderer가 실제 준비된 시점에만 타이머 시작
        _graphControl.RendererReady += (_, _) =>
        {
            Debug.WriteLine("✅ RendererReady: (internal task ready)");
            // 타이머는 여기서 X
            //AppendTestShape();              // 다시 호출 보장
            _graphControl.RequestRender();  // 수동 렌더링 요청
        };

        _graphControl.RendererResetting += (_, _) =>
        {
            _renderTimer.Stop();
            _dataTimer.Stop();
            IsRendering = false;
            Debug.WriteLine("✅ RendererResetting: Timers stopped");
        };

        _graphControl.RendererReset += (_, _) =>
        {
            //_graphControl.UpdateTransform(-10f, 20f, 20f, 0f); // 확대 + 왼쪽 이동
            _dataTimer.Start();
            _renderTimer.Start();
            IsRendering = true;
            Debug.WriteLine("✅ RendererReset: Timers restarted");
            //AppendTestShape(); // 안전을 위해 추가
            _graphControl.RequestRender();
        };
        
        foreach (string ch in _channels)
        {
            var buffer = new ChunkedVertexBuffer<VertexPositionColor>(500_000, v => v.Position.X)
            {
                AutoTrim = true,
                MaxVisibleRange = 30f
            };

            var item = new FastGraphItem<VertexPosition>(100_000)
            {
                Name = ch,
                GraphColor = ch switch
                {
                    "CH1" => new Color4(1f, 0f, 0f, 1f),
                    "CH2" => new Color4(0f, 1f, 0f, 1f),
                    "CH3" => new Color4(0f, 0f, 1f, 1f),
                    _ => new Color4(1f, 1f, 1f, 1f)
                },
                UseHistoryCache = false
            };
            var Litem = new GraphLineItem<VertexPositionColor>(v => v.Position.X)
            {
                GraphColor = item.GraphColor
            };

            _buffers[ch] = buffer;
            _fastItems[ch] = item;
            _lineItems[ch] = Litem;
            //_graphControl.AddItem(Litem); // 내부에서 자동 Transform + 렌더 요청
            _graphControl.AddItem(item); // 내부에서 자동 Transform + 렌더 요청
        }

        _graphControl.AttachRenderThread(_renderThread!);
    }

    private void GenerateVertices()
    {
        Span<VertexPosition> buffer = stackalloc VertexPosition[1];

        for (int i = 0; i < 10; i++)
        {
            foreach (string ch in _channels)
            {
                if (!_fastItems.TryGetValue(ch, out FastGraphItem<VertexPosition>? item)) continue;

                double x = _channelTime.TryGetValue(ch, out var last) ? last : 0f;

                x = (double)last;
                x += XStep;  // XStep도 double이면 더 좋음

                float xf = (float)x;
                float y = ch switch
                {
                    "CH1" => MathF.Sin(xf),
                    "CH2" => MathF.Cos(xf),
                    "CH3" => MathF.Sin(xf) * MathF.Cos(xf),
                    _ => 0f
                };


                buffer[0] = new VertexPosition(xf, y);
                item.AppendBatch(buffer);
                _channelTime[ch] = xf;
            }
        }
    }

    public void AppendTestShape()
    {
        if (_graphControl == null )
            return;

        var testItem = new FastGraphItem<VertexPosition>(10)
        {
            Name = "TestTriangle",
            GraphColor = new Color4(1f, 1f, 0f, 1f),
            UseHistoryCache = false
        };

        _graphControl.AddItem(testItem);

        // ✅ 중심 좌표를 화면에 보이는 45 정도로 이동
        var shape = new VertexPosition[]
 {
    new VertexPosition(0.0f, 0.0f),
    new VertexPosition(1.0f, 0.0f),
    new VertexPosition(0.5f, 1.0f),
    new VertexPosition(0.0f, 0.0f)
 };


        testItem.AppendBatch(shape);

        //// ⛳️ transform 재계산 강제 적용
        //_graphControl.UpdateTransformFromFirstItem(); // 여기서 LastX 등이 반영된 상태로 다시 계산
        testItem.Transform(0f, 1f, 1f, force: true);
        _graphControl.UpdateTransform(0f, 1f, 1f, 0f); // GPU 상 ViewProjectionBuffer 적용
    }

    private void GenerateVerticesLegacy()
    {
        if (!_buffers.TryGetValue("CH1", out _))
            return;
        _ = (float)_stopwatch.Elapsed.TotalSeconds; // 또는 타이머 기준 시각

        for (int i = 0; i < 10; i++)
        {
            foreach (string ch in _channels)
            {
                if (!_buffers.TryGetValue(ch, out ChunkedVertexBuffer<VertexPositionColor>? buffer) || !_lineItems.TryGetValue(ch, out GraphLineItem<VertexPositionColor>? item)) continue;

                // ✅ 채널별 시간 흐름 추적
                float x = _channelTime.TryGetValue(ch, out var last) ? last : 0f;

                float y = ch switch
                {
                    "CH1" => MathF.Sin(x),
                    "CH2" => MathF.Cos(x),
                    "CH3" => MathF.Sin(x) * MathF.Cos(x),
                    _ => 0f
                };

                if (x < buffer.LastX)
                {
                    Debug.WriteLine($"🚫 Skipping point (backwards): {x} < {buffer.LastX}");
                    continue;
                }

                buffer.Append(new VertexPositionColor(x, y, 0f, item.GraphColor));
                _channelTime[ch] = x + (float)XStep; // ✅ 시간 전진
            }
        }
    }
    public void OnRenderFrameLegacy(float time)
    {
        float visibleEnd = _buffers["CH1"].LastX;
        float visibleStart = visibleEnd - 2.0f;
        float centerX = (visibleStart + visibleEnd) / 2f;
        float offset = -centerX;

        foreach (string ch in _channels)
        {
            if (!_buffers.TryGetValue(ch, out ChunkedVertexBuffer<VertexPositionColor>? buffer) ||
                !_lineItems.TryGetValue(ch, out GraphLineItem<VertexPositionColor>? item))
                continue;

            if (buffer.LastX <= 0 || float.IsNaN(buffer.LastX))
                continue; // ✅ 안전 필터링

            VertexPositionColor[] tempArray = ArrayPool<VertexPositionColor>.Shared.Rent(4096);
            try
            {
                Span<VertexPositionColor> temp = tempArray;
                int count = buffer.CopyInRange(visibleStart, visibleEnd, temp);

                if (count > 0)
                {
                    item.EnqueueVertices(temp.Slice(0, count));
                }
            }
            finally
            {
                ArrayPool<VertexPositionColor>.Shared.Return(tempArray);
            }
        }

        _graphControl?.UpdateTransform(offset, 1f, 1f);
    }

    [RelayCommand]
    private void ToggleRendering()
    {
        IsRendering = !IsRendering;

        if (IsRendering)
        {
            _renderThread?.RequestRender();
        }
    }

    public override void OnNavigatedTo()
    {
        if (!_isInitialized)
        {
            InitializeViewModel();
        }
    }
    private void InitializeViewModel()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;

    }

    private Dock _selectedTabPlacement = Dock.Right;
    public Dock SelectedTabPlacement
    {
        get => _selectedTabPlacement;
        set => SetProperty(ref _selectedTabPlacement, value);
    }

    public void Dispose()
    {
        this.Dispose(true);

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing">Defines whether managed resources should also be freed.</param>
    protected virtual void Dispose(bool disposing)
    {

        if (disposing)
        {
            _graphControl?.Dispose();

            foreach (FastGraphItem<VertexPosition> item in _fastItems.Values)
            {
                item.Dispose();
            }

            foreach (GraphLineItem<VertexPositionColor> item in _lineItems.Values)
            {
                item.Dispose();
            }

            _lineItems.Clear();
            _buffers.Clear();
            _dataTimer?.Dispose();
        }
    }
}

