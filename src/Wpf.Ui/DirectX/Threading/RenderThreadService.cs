// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Diagnostics;
using System.Runtime.InteropServices;

using Wpf.Ui.DirectX.Rendering;

namespace Wpf.Ui.DirectX.Threading;

/// <summary>
/// 고성능 전용 렌더링 스레드 서비스
/// </summary>
public sealed class RenderThreadService : IRenderThreadService
{
    private readonly List<IRenderable> _renderables = new();
    private readonly AutoResetEvent _renderEvent = new(false);
    private readonly object _lock = new();

    private Thread? _thread;
    private bool _running;

    public bool IsRunning => _running;

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
        _ = _renderEvent.Set();
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

    public void RequestRender()
    {
        _ = _renderEvent.Set();
    }

    private void RenderLoop()
    {
        Debug.WriteLine("[RenderThread] 🟢 Started");

        var stopwatch = Stopwatch.StartNew();

        try
        {
            while (_running)
            {
                _ = _renderEvent.WaitOne();

                if (!_running)
                {
                    Debug.WriteLine("[RenderThread] 🛑 Exit signal received");
                    break;
                }

                float time = (float)stopwatch.Elapsed.TotalSeconds;

                lock (_lock)
                {
                    foreach (IRenderable renderable in _renderables.ToArray())
                    {
                        // 렌더링 전 유효성 재검증
                        if (renderable is D3D11Renderer renderer &&
                            !renderer.IsContextValid())
                        {
                            continue;
                        }

                        if (!renderable.IsReady)
                        {
                            continue;
                        }

                        try
                        {
                            renderable.RenderFrame(time);
                        }
                        catch (SEHException ex)
                        {
                            Debug.WriteLine($"SEH in render loop: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[RenderThread] ❌ Exception in renderable.RenderFrame: {ex}");
                        }
                    }
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
        _renderEvent.Dispose();
    }
}
