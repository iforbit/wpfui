// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Diagnostics;
using System.Windows.Controls;

using Wpf.Ui.DirectX.Controls;
using Wpf.Ui.DirectX.Models;
using Wpf.Ui.DirectX.Models.VertexTypes;
using Wpf.Ui.DirectX.Services;

namespace Wpf.Ui.Demo.Mvvm.ViewModels;

public partial class DashboardViewModel : ViewModel, IDisposable
{
    private bool _isInitialized = false;

    private GraphControl? _graphControl;
    private readonly IRenderThreadService? _renderThread;

    private readonly Dictionary<string, RealTimeSeries<VertexPosition>> _seriesMap = new();
    private readonly Dictionary<string, float> _channelTime = new();
    private readonly string[] _channels = ["CH1", "CH2", "CH3"];
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
            _channelTime[ch] = 0f;

        _dataTimer.Elapsed += (_, _) =>
        {
            lock (_generateVerticesLock)
            {
                GenerateVertices();
            }
        };
    }

    public void SetGraphControl(GraphControl control)
    {
        _graphControl = control;

        _graphControl.RendererResetting += (_, _) =>
        {
            IsRendering = false;
        };

        _graphControl.RendererReset += (_, _) =>
        {
            IsRendering = true;
        };
        _graphControl.AttachRenderThread(_renderThread!);

        // 시리즈 초기화
        foreach (string ch in _channels)
        {
            var series = new RealTimeSeries<VertexPosition>
            {
                Name = ch,
                GraphColor = ch switch
                {
                    "CH1" => new(1f, 0f, 0f, 1f),
                    "CH2" => new(0f, 1f, 0f, 1f),
                    "CH3" => new(0f, 0f, 1f, 1f),
                    _ => new(1f, 1f, 1f, 1f)
                }
            };

            series.Initialize();
            _seriesMap[ch] = series;
            _graphControl.AddSeries(series);
        }

        _dataTimer.Start();
    }

    private void GenerateVertices()
    {
        Span<VertexPosition> buffer = stackalloc VertexPosition[1];

        for (int i = 0; i < 10; i++)
        {
            foreach (string ch in _channels)
            {
                if (!_seriesMap.TryGetValue(ch, out RealTimeSeries<VertexPosition>? series)) continue;

                double x = _channelTime[ch];
                x += 0.02;

                float xf = (float)x;
                float y = ch switch
                {
                    "CH1" => MathF.Sin(xf),
                    "CH2" => MathF.Cos(xf),
                    "CH3" => MathF.Sin(xf) * MathF.Cos(xf),
                    _ => 0f
                };

                buffer[0] = new VertexPosition(xf, y);
                series.Append(buffer);
                _channelTime[ch] = xf;
            }
        }

        // ⏩ Transform 자동 스크롤
        if (_graphControl?.TransformManager != null)
        {
            float latestX = _seriesMap.Values.Max(s => s.LastX);
            _graphControl.FollowLatestX(latestX);
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
            _dataTimer?.Dispose();
            _graphControl?.Dispose();

            foreach (RealTimeSeries<VertexPosition> series in _seriesMap.Values)
            {
                series.Dispose();
            }

            _seriesMap.Clear();
        }
    }
}

