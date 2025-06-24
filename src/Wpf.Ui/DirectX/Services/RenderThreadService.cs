// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Diagnostics;
using System.Runtime.InteropServices;

using Wpf.Ui.DirectX.Core;

namespace Wpf.Ui.DirectX.Services;

public sealed class RenderThreadService : IRenderThreadService, IDisposable
{
    private readonly List<IRenderable> _renderables = new();
    private readonly object _lock = new();
    private Thread? _thread;
    private bool _running;
    private int _targetFps = 60;

    public bool IsRunning => _running;

    public int TargetFps
    {
        get => _targetFps;
        set => _targetFps = Math.Clamp(value, 1, 240);
    }

    public void Start()
    {
        if (_running)
        {
            return;
        }

        _running = true;
        _thread = new Thread(RenderLoop)
        {
            IsBackground = true,
            Name = "RenderThread"
        };
        _thread.Start();
    }

    public void Stop()
    {
        _running = false;
        _thread?.Join();
        _thread = null;
    }

    public void Register(IRenderable renderable)
    {
        lock (_lock)
        {
            if (!_renderables.Contains(renderable))
            {
                _renderables.Add(renderable);
            }
        }
    }

    public void Unregister(IRenderable renderable)
    {
        lock (_lock)
        {
            _ = _renderables.Remove(renderable);
        }
    }

    private void RenderLoop()
    {
        Debug.WriteLine("[RenderThread] 🟢 Started");
        var stopwatch = Stopwatch.StartNew();
        double targetFrameTime = 1000.0 / TargetFps;

        try
        {
            while (_running)
            {
                long nowTicks = stopwatch.ElapsedTicks;
                float time = (float)(nowTicks / (double)Stopwatch.Frequency);

                lock (_lock)
                {
                    foreach (IRenderable renderable in _renderables.ToArray())
                    {
                        if (!renderable.IsReady)
                        {
                            continue;
                        }

                        try
                        {
                            lock (renderable)
                            {
                                renderable.RenderFrame(time);
                            }
                        }
                        catch (SEHException ex)
                        {
                            Debug.WriteLine($"[RenderThread] ❌ SEHException: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[RenderThread] ❌ Exception: {ex}");
                        }
                    }
                }

                long elapsedMs = (stopwatch.ElapsedTicks - nowTicks) * 1000 / Stopwatch.Frequency;
                int sleepTime = (int)(targetFrameTime - elapsedMs);
                if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RenderThread] ❗ Unhandled exception caused exit: {ex}");
        }

        Debug.WriteLine("[RenderThread] 🔚 Fully exited");
    }

    public void Dispose()
    {
        Stop();
    }
}