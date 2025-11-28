// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Diagnostics;

using Wpf.Ui.Demo.Mvvm.SharedMemory;
using Wpf.Ui.DirectX.Controls;
using Wpf.Ui.DirectX.Models;
using Wpf.Ui.DirectX.Models.VertexTypes;
using Wpf.Ui.DirectX.Services;

namespace Wpf.Ui.Demo.Mvvm.ViewModels;

public partial class DashboardViewModel : ViewModel, IDisposable
{
    // Readonly fields first
    private readonly IRenderThreadService? _renderThread;
    private readonly SharedMemoryReader _reader = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Dictionary<string, RealTimeSeries<VertexPosition>> _seriesMap = new();
    private readonly Dictionary<string, (double X, float Y)> _lastValueMap = new();
    private readonly Dictionary<string, int> _channelIndexMap = new();
    private readonly DateTime _lastLoopUpdate = DateTime.Now;
    private readonly DateTime _lastLoopRateUpdate = DateTime.Now;

    // Non-readonly fields
    private DXGraph? _graphControl;
    private int _receiveCount = 0;
    private int _loopCount = 0;
    private DateTime _lastRateUpdate = DateTime.Now;
    private int _lastChannelCount = 0;

    [ObservableProperty]
    private string _statusMessage = "그래프 대기 중";
    [ObservableProperty]
    private string _receiveRateText = "Signal Hz: -";

    // Command for RibbonSplitButton demo
    [RelayCommand]
    private void NewDocument()
    {
        StatusMessage = "📄 New Document created!";
        Debug.WriteLine("RibbonSplitButton clicked - New Document command executed");
    }

    public DashboardViewModel(IRenderThreadService renderThread)
    {
        _renderThread = renderThread;
        StartPolling();
    }

    public void SetGraphControl(DXGraph control)
    {
        _graphControl = control;
        _seriesMap.Clear();
        _graphControl.AttachRenderThread(_renderThread!);

        _graphControl.SetAutoScrollEnabled(true);

        // Lock Y scale to prevent jittering from auto-scaling
        // Assuming typical amplitude is around -5 to +5
        _graphControl.SetYScaleLock(true);

        // Set initial visible range to show more data points
        // Writer sends 5000 points per update, so show ~10000 range
        _graphControl.SetVisibleRange(0, 10000);
    }

    private void UpdateChannels(int channelCount)
    {
        if (_graphControl == null || channelCount == _lastChannelCount)
            return;

        // Clear data for removed channels (DXGraph doesn't have RemoveSeries)
        if (channelCount < _lastChannelCount)
        {
            for (int i = channelCount; i < _lastChannelCount; i++)
            {
                string name = $"CH{i + 1}";
                if (_seriesMap.TryGetValue(name, out RealTimeSeries<VertexPosition>? series))
                {
                    series.Clear();
                }

                _ = _seriesMap.Remove(name);
                _ = _channelIndexMap.Remove(name);
                _ = _lastValueMap.Remove(name);
            }
        }
        // Add new channels
        else if (channelCount > _lastChannelCount)
        {
            for (int i = _lastChannelCount; i < channelCount; i++)
            {
                string name = $"CH{i + 1}";
                var series = new RealTimeSeries<VertexPosition>(capacity: 100_000)
                {
                    Name = name,
                    GraphColor = i switch
                    {
                        0 => new(1f, 0f, 0f, 1f),  // Red
                        1 => new(0f, 1f, 0f, 1f),  // Green
                        2 => new(0f, 0f, 1f, 1f),  // Blue
                        3 => new(1f, 1f, 0f, 1f),  // Yellow
                        4 => new(1f, 0f, 1f, 1f),  // Magenta
                        5 => new(0f, 1f, 1f, 1f),  // Cyan
                        _ => new(1f, 1f, 1f, 1f)   // White
                    }
                };
                series.Initialize();
                _seriesMap[name] = series;
                _channelIndexMap[name] = i;
                _graphControl.AddSeries(series);
            }
        }

        _lastChannelCount = channelCount;
    }

    private async void StartPolling()
    {
        StatusMessage = "⏳ SharedMemory 연결 중...";
        const int intervalMs = 16; // ~60 FPS to match Writer's UpdateRate

        var stopwatch = Stopwatch.StartNew();
        int _rawReceiveCount = 0;

        // async 메서드에서는 stackalloc 사용 불가 - 배열 사용
        var pointBuffer = new (double X, float Y)[50000];
        var vertexBuffer = new VertexPosition[50000];

        try
        {
            while (!_cts.IsCancellationRequested)
            {
                long loopStart = stopwatch.ElapsedMilliseconds;
                _loopCount++;

                (int channelCount, int pointsPerChannel) = _reader.ReadHeader();
                if (channelCount <= 0 || pointsPerChannel <= 0 || channelCount > 10 || pointsPerChannel > 50000)
                {
                    await Task.Delay(10);
                    continue;
                }

                // Update channels dynamically based on channelCount from SharedMemory
                UpdateChannels(channelCount);

                for (int ch = 0; ch < channelCount; ch++)
                {
                    string name = $"CH{ch + 1}";
                    if (!_seriesMap.TryGetValue(name, out RealTimeSeries<VertexPosition>? series))
                        continue;

                    // 필요한 크기만큼 Slice해서 사용
                    Span<(double X, float Y)> currentPointBuffer = pointBuffer.AsSpan(0, pointsPerChannel);
                    _reader.ReadChannel(ch, pointsPerChannel, currentPointBuffer);
                    _rawReceiveCount += pointsPerChannel;

                    Span<VertexPosition> vertexSpan = vertexBuffer.AsSpan(0, pointsPerChannel);
                    int count = 0;

                    for (int i = 0; i < pointsPerChannel; i++)
                    {
                        (double X, float Y) p = currentPointBuffer[i];
                        bool hasData = series.TotalVertexCount > 0;
                        float lastX = series.LastX;

                        if (hasData && p.X <= lastX)
                            continue;

                        vertexSpan[count++] = new VertexPosition((float)p.X, p.Y);
                        _lastValueMap[name] = (p.X, p.Y);
                        _receiveCount++;
                    }

                    if (count > 0)
                    {
                        series.Append(vertexSpan.Slice(0, count));
                    }
                }

                DateTime now = DateTime.Now;
                if ((now - _lastRateUpdate).TotalSeconds >= 1)
                {
                    double elapsedSec = (now - _lastRateUpdate).TotalSeconds;
                    double loopHz = _loopCount / elapsedSec;
                    double expected = loopHz * channelCount * pointsPerChannel;
                    double signalHz = _receiveCount / elapsedSec;
                    double appendRatio = expected > 0 ? _receiveCount / expected : 0;

                    ReceiveRateText = $"Signal Hz: {signalHz:F1} / {loopHz:F1} ({appendRatio:P0})";

                    _receiveCount = 0;
                    _rawReceiveCount = 0;
                    _loopCount = 0;
                    _lastRateUpdate = now;
                }

                int elapsed = (int)(stopwatch.ElapsedMilliseconds - loopStart);
                int delay = intervalMs - elapsed;
                if (delay > 0)
                    await Task.Delay(delay);
                else
                    await Task.Yield();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ 오류 발생: {ex.Message}";
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cts.Cancel();
            _cts.Dispose();
            _reader.Dispose();
            _graphControl = null;
        }
    }
}

