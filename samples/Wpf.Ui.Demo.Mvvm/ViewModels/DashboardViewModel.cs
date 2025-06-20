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

    private readonly Dictionary<string, ChunkedVertexBuffer> _buffers = new();

    private readonly Dictionary<string, GraphLineItem> _lineItems = new();
    private readonly Dictionary<string, float> _channelTime = new();
    private readonly string[] _channels = ["CH1", "CH2", "CH3"];

    private const float XStep = 0.002f; // 20Hz 샘플링 간격처럼 동작

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
                OnRenderFrame((float)_stopwatch.Elapsed.TotalSeconds);
                _graphControl.RequestRender();
            }
        };
    }

    public void SetGraphControl(GraphControl control)
    {
        _graphControl = control;
        _graphControl.AttachRenderThread(_renderThread!);

        // Renderer가 실제 준비된 시점에만 타이머 시작
        _graphControl.RendererReady += (_, _) =>
        {
            Debug.WriteLine("✅ RendererReady: (internal task ready)");
            // 타이머는 여기서 X
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
            _graphControl.UpdateTransform(-5f, 0.2f, 0.5f);  // ✅ Transform도 여기서 적용
            _dataTimer.Start();
            _renderTimer.Start();
            IsRendering = true;
            Debug.WriteLine("✅ RendererReset: Timers restarted");
        };

        foreach (string ch in _channels)
        {
            var buffer = new ChunkedVertexBuffer
            {
                XStep = XStep,
                MaxBufferLength = 500_000,
                AutoTrim = true,
                MaxVisibleRange = 30f
            };

            var item = new GraphLineItem
            {
                GraphColor = ch switch
                {
                    "CH1" => new Color4(1f, 0f, 0f, 1f),
                    "CH2" => new Color4(0f, 1f, 0f, 1f),
                    "CH3" => new Color4(0f, 0f, 1f, 1f),
                    _ => new Color4(1f, 1f, 1f, 1f)
                }
            };

            _buffers[ch] = buffer;
            _lineItems[ch] = item;

            _graphControl.AddItem(item); // 내부에서 자동 Transform + 렌더 요청
        }

        _graphControl.UpdateTransform(-5f, 0.2f, 0.5f);
    }

    private void GenerateVertices()
    {
        if (!_buffers.TryGetValue("CH1", out _))
            return;
        _ = (float)_stopwatch.Elapsed.TotalSeconds; // 또는 타이머 기준 시각

        for (int i = 0; i < 10; i++)
        {
            foreach (string ch in _channels)
            {
                if (!_buffers.TryGetValue(ch, out ChunkedVertexBuffer? buffer) ||
                    !_lineItems.TryGetValue(ch, out GraphLineItem? item))
                    continue;

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

                buffer.AppendPoint(x, y, item.GraphColor);
                _channelTime[ch] = x + XStep; // ✅ 시간 전진
            }
        }
    }

    public void OnRenderFrame(float time)
    {
        float visibleEnd = _buffers["CH1"].LastX;
        float visibleStart = visibleEnd - 2.0f;
        float centerX = (visibleStart + visibleEnd) / 2f;
        float offset = -centerX;

        foreach (string ch in _channels)
        {
            if (!_buffers.TryGetValue(ch, out ChunkedVertexBuffer? buffer) ||
                !_lineItems.TryGetValue(ch, out GraphLineItem? item))
                continue;

            if (buffer.LastX <= 0 || float.IsNaN(buffer.LastX))
                continue; // ✅ 안전 필터링

            VertexPositionColor[] tempArray = ArrayPool<VertexPositionColor>.Shared.Rent(4096);
            try
            {
                Span<VertexPositionColor> temp = tempArray;
                int count = buffer.CopyVerticesInRange(visibleStart, visibleEnd, temp);

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

    // ViewModel에 추가
    [RelayCommand]
    private void ApplyTestTransform()
    {
        _graphControl?.UpdateTransform(-1600f, 0.005f, 1.0f);
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

            foreach (GraphLineItem item in _lineItems.Values)
            {
                item.Dispose();
            }

            _lineItems.Clear();
            _buffers.Clear();
            _dataTimer?.Dispose();
        }
    }
}

