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
    private readonly IRenderThreadService? _renderThread;
    private DXGraph? _graphControl;
    private readonly SharedMemoryReader _reader = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Dictionary<string, RealTimeSeries<VertexPosition>> _seriesMap = new();
    private readonly Dictionary<string, (double X, float Y)> _lastValueMap = new();
    private readonly Dictionary<string, int> _channelIndexMap = new();

    private readonly string[] _channels = ["CH1", "CH2"];

    private int _receiveCount = 0;
    private int _loopCount = 0;
    private DateTime _lastRateUpdate = DateTime.Now;
    private readonly DateTime _lastLoopUpdate = DateTime.Now;

    [ObservableProperty]
    private string _statusMessage = "그래프 대기 중";
    [ObservableProperty]
    private string _receiveRateText = "Signal Hz: -";
    public DashboardViewModel(IRenderThreadService renderThread)
    {
        _renderThread = renderThread;
        for (int i = 0; i < _channels.Length; i++)
            _channelIndexMap[_channels[i]] = i;
        StartPolling();
    }

    public void SetGraphControl(DXGraph control)
    {
        _graphControl = control;
        _seriesMap.Clear();
        _graphControl.AttachRenderThread(_renderThread!);

        foreach (string ch in _channels)
        {
            var series = new RealTimeSeries<VertexPosition>
            {
                Name = ch,
                GraphColor = ch switch
                {
                    "CH1" => new(1f, 0f, 0f, 1f),
                    "CH2" => new(0f, 1f, 0f, 1f),
                    _ => new(1f, 1f, 1f, 1f)
                }
            };
            series.Initialize();
            _seriesMap[ch] = series;
            _graphControl.AddSeries(series);
        }

        _graphControl.SetAutoScrollEnabled(true);
        _graphControl.SetYScaleLock(false);
    }

    private readonly DateTime _lastLoopRateUpdate = DateTime.Now;
    private async void StartPolling()
    {
        StatusMessage = "⏳ SharedMemory 연결 중...";
        const int intervalMs = 10;

        var stopwatch = Stopwatch.StartNew();
        int _rawReceiveCount = 0;

        try
        {
            while (!_cts.IsCancellationRequested)
            {
                long loopStart = stopwatch.ElapsedMilliseconds;
                _loopCount++;

                if (_seriesMap.Count == 0)
                {
                    await Task.Delay(100);
                    continue;
                }

                (int channelCount, int pointsPerChannel) = _reader.ReadHeader();
                if (channelCount <= 0 || pointsPerChannel <= 0 || channelCount > 16 || pointsPerChannel > 1024)
                {
                    await Task.Delay(10);
                    continue;
                }

                Span<(double X, float Y)> pointBuffer = stackalloc (double X, float Y)[pointsPerChannel];

                for (int ch = 0; ch < channelCount; ch++)
                {
                    if (ch >= _channels.Length)
                        continue;

                    string name = _channels[ch];
                    if (!_seriesMap.TryGetValue(name, out RealTimeSeries<VertexPosition>? series))
                        continue;

                    _reader.ReadChannel(ch, pointsPerChannel, pointBuffer);
                    _rawReceiveCount += pointsPerChannel;

                    Span<VertexPosition> vertexSpan = stackalloc VertexPosition[pointsPerChannel];
                    int count = 0;

                    for (int i = 0; i < pointsPerChannel; i++)
                    {
                        (double X, float Y) p = pointBuffer[i];
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
        _graphControl = null;
        _cts.Cancel();
        _reader.Dispose();
    }
}

