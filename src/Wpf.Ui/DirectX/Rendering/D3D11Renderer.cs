// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using SharpGen.Runtime;

using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using Wpf.Ui.DirectX.Helpers;
using Wpf.Ui.DirectX.Models;
using Wpf.Ui.DirectX.Services;
using Wpf.Ui.DirectX.Threading;

using MapFlags = Vortice.Direct3D11.MapFlags;

namespace Wpf.Ui.DirectX.Rendering;

public sealed class D3D11Renderer : IRenderable, IDisposable
{
    public bool IsReady =>
     !_disposed &&
     _inputLayout != null &&
     _vertexShader != null &&
     _pixelShader != null &&
     _renderTargetView != null &&
     _swapChain != null &&
     _viewProjectionBuffer != null &&
     _context != null &&
     _context.NativePointer != IntPtr.Zero;
    private bool _disposed = false;

    private readonly ID3DGraphicsService _graphicsService;
    private readonly IRenderThreadService _renderThread;
    private readonly ID3D11Device _device;
    private readonly ID3D11DeviceContext _context;
    private IDXGISwapChain? _swapChain;
    private ID3D11RenderTargetView? _renderTargetView;
    private readonly List<GraphItemBase> _graphItems = new();
    private readonly List<GraphItemBase> _pendingItems = new();

    private ID3D11VertexShader? _vertexShader;
    private ID3D11PixelShader? _pixelShader;
    private ID3D11InputLayout? _inputLayout;
    private ID3D11Buffer? _viewProjectionBuffer;
    public float XOffset { get; private set; } = 0f;

    public float XScale { get; private set; } = 1f;

    public float YScale { get; private set; } = 1f;

    private readonly double _width;
    private readonly double _height;
    private readonly IntPtr _hwnd;

    public double Width => _width;
    public double Height => _height;
    public ID3D11Device Device => _device;
    public ID3D11DeviceContext Context => _context;
    private DateTime _lastReinitTime = DateTime.MinValue;
    private readonly TimeSpan _minReinitInterval = TimeSpan.FromSeconds(5);
    private int _reinitFailCount = 0;
    private const int MaxReinitFailCount = 3;
    private readonly object _reinitLock = new();
    private readonly object _graphItemsLock = new();
    private readonly object _contextLock = new();

    private volatile bool _isReinitializing = false;

    // 생성자: 최초 한 번만 호출, 재초기화는 별도 메서드로 처리
    public D3D11Renderer(ID3DGraphicsService graphicsService, IRenderThreadService renderThread, IntPtr hwnd, double width, double height)
    {
        _graphicsService = graphicsService ?? throw new ArgumentNullException(nameof(graphicsService));
        _renderThread = renderThread ?? throw new ArgumentNullException(nameof(renderThread));
        _hwnd = hwnd;
        _width = width;
        _height = height;

        _device = _graphicsService.Device;
        _context = _graphicsService.Context;

        InitializeResources();

        _renderThread.Register(this);
    }

    private void InitializeResources()
    {
        DisposeResources(); // ✅ 항상 기존 리소스 정리 후 시작

        _swapChain = ((IDXGIFactory1)_graphicsService.Factory).CreateSwapChain(_device, new SwapChainDescription
        {
            BufferDescription = new ModeDescription((uint)_width, (uint)_height, new Rational(60, 1), Format.B8G8R8A8_UNorm),
            SampleDescription = new SampleDescription(1, 0),
            BufferUsage = Usage.RenderTargetOutput,
            BufferCount = 1,
            OutputWindow = _hwnd,
            Windowed = true,
            SwapEffect = SwapEffect.Discard,
            Flags = SwapChainFlags.None
        });

        _renderTargetView = _graphicsService.CreateRenderTargetView(_swapChain);

        InitializeShaders();
        CreateViewProjectionBuffer();
        UpdateViewProjectionBuffer();

        Debug.WriteLine("✅ Renderer resources initialized.");
    }


    private void DisposeResources()
    {
        static void SafeDispose<T>(ref T? resource) where T : class, IDisposable
        {
            resource?.Dispose();
            resource = null;
        }

        SafeDispose(ref _vertexShader);
        SafeDispose(ref _pixelShader);
        SafeDispose(ref _inputLayout);
        SafeDispose(ref _viewProjectionBuffer);
        SafeDispose(ref _renderTargetView);
        SafeDispose(ref _swapChain);

        Debug.WriteLine("🧹 All GPU resources disposed.");
    }


    private void InitializeShaders()
    {
        string vsPath = Path.Combine(AppContext.BaseDirectory, "Assets", "LineVertexShader.hlsl");
        string psPath = Path.Combine(AppContext.BaseDirectory, "Assets", "LinePixelShader.hlsl");

        var vsResult = Compiler.CompileFromFile(vsPath, "VSMain", "vs_5_0", out Blob? vsBytecode, out Blob? vsErr);
        var psResult = Compiler.CompileFromFile(psPath, "PSMain", "ps_5_0", out Blob? psBytecode, out Blob? psErr);

        if (vsResult.Failure || vsBytecode == null)
            throw new InvalidOperationException(vsErr?.AsString() ?? "Vertex shader error.");
        if (psResult.Failure || psBytecode == null)
            throw new InvalidOperationException(psErr?.AsString() ?? "Pixel shader error.");

        _vertexShader = _device.CreateVertexShader(vsBytecode);
        _pixelShader = _device.CreatePixelShader(psBytecode);

        _inputLayout = _device.CreateInputLayout(
            new InputElementDescription[]
            {
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, 12, 0)
            },
            vsBytecode
        );

        vsBytecode.Dispose();
        psBytecode.Dispose();
        vsErr?.Dispose();
        psErr?.Dispose();
    }

    private void CreateViewProjectionBuffer()
    {
        var bufferDesc = new BufferDescription(
            (uint)Marshal.SizeOf<Vector4>(),
            BindFlags.ConstantBuffer,
            ResourceUsage.Dynamic,
            CpuAccessFlags.Write,
            ResourceOptionFlags.None,
            0
        );

        _viewProjectionBuffer = _device.CreateBuffer(bufferDesc);
    }

    public void SetTransform(float xOffset, float xScale, float yScale)
    {
        XOffset = xOffset;
        XScale = xScale;
        YScale = yScale;

        UpdateViewProjectionBuffer();
    }

    public void UpdateViewProjectionBuffer()
    {
        lock (_contextLock) // 전체 Context 사용을 보호
        {
            if (_viewProjectionBuffer == null || _context == null || _disposed)
            {
                Debug.WriteLine("⚠️ UpdateViewProjectionBuffer skipped: Buffer or context is null or disposed.");
                return;
            }

            // 디바이스 및 컨텍스트가 여전히 유효한지 확인
            if (!IsDeviceValid() || !IsContextValid())
            {
                Debug.WriteLine("⚠️ UpdateViewProjectionBuffer skipped: Invalid device or context.");
                return;
            }

            try
            {
                // Context 유효성 재검증
                if (!IsContextValid())
                {
                    Debug.WriteLine("Context is invalid, skipping update");
                    return;
                }

                var data = new Vector4(XScale, YScale, XOffset, 0.0f);
                MappedSubresource mapped = _context.Map(_viewProjectionBuffer, 0,
                    MapMode.WriteDiscard, MapFlags.None);
                Marshal.StructureToPtr(data, mapped.DataPointer, false);
                _context.Unmap(_viewProjectionBuffer, 0);
            }
            catch (SEHException ex)
            {
                Debug.WriteLine($"💥 SEHException in Map: {ex.Message}");
            }
            catch (SharpGenException ex)
            {
                Debug.WriteLine($"💥 SharpGenException in Map: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 General Exception in Map: {ex.Message}");
            }
        }
    }

    public bool IsContextValid()
    {
        try
        {
            // Context가 여전히 유효한지 확인
            return _context != null &&
                   _context.NativePointer != IntPtr.Zero &&
                   !_disposed;
        }
        catch
        {
            return false;
        }
    }


    public bool IsDeviceValid()
    {
        try
        {
            return _device != null &&
                   _device.NativePointer != IntPtr.Zero &&
                   !_disposed;
        }
        catch
        {
            return false;
        }
    }
    public void RenderFrame(float time)
    {
        if (_disposed)
        {
            return;
        }

        Render(time);
    }

    public void Render(float time)
    {
        if (!IsReady)
        {
            return;
        }

        lock (_graphItemsLock)
        {
            _context.OMSetRenderTargets(_renderTargetView);
            _context.ClearRenderTargetView(_renderTargetView, new Color4(0, 0, 0, 1));

            _context.IASetInputLayout(_inputLayout);
            _context.VSSetShader(_vertexShader);
            _context.VSSetConstantBuffer(0, _viewProjectionBuffer);

            _context.PSSetShader(_pixelShader);
            _context.RSSetViewport(new Viewport(0, 0, (int)_width, (int)_height, 0, 1));

            foreach (GraphItemBase item in _graphItems)
            {
                if (item.IsVisible)
                {
                    try
                    {
                        //item.Update(time);
                        item.Render(_context);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[Render Error] {item.Name}: {ex.Message}");
                    }
                }
            }
        }

        try
        {
            _ = _swapChain.Present(0, PresentFlags.None);
        }
        catch (SharpGenException ex) when (
            ex.ResultCode.Code == unchecked((int)0x887A0005) ||
            ex.ResultCode.Code == unchecked((int)0x887A0006) ||
            ex.ResultCode.Code == unchecked((int)0x887A000D))
        {
            Debug.WriteLine("💥 GPU device removed/reset/hung detected. Reinitializing renderer.");
            ReinitializeRenderer();
        }
        catch (SEHException ex)
        {
            Debug.WriteLine($"💥 SEHException caught during Render(): {ex.Message}. Reinitializing renderer.");
            ReinitializeRenderer();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"💥 Unexpected exception during Render(): {ex.Message}");
        }
    }

    private void ReinitializeRenderer()
    {
        if (_isReinitializing)
        {
            return;
        }

        lock (_reinitLock)
        {
            if (_isReinitializing)
            {
                return;
            }

            _isReinitializing = true;

            if ((DateTime.Now - _lastReinitTime) < _minReinitInterval)
            {
                Debug.WriteLine("⚠️ Reinitialize skipped to prevent rapid loop.");
                return;
            }

            _lastReinitTime = DateTime.Now;

            Debug.WriteLine("⚠️ Reinitializing renderer due to device removal or reset...");

            try
            {
                DisposeResources();
                InitializeResources();

                _reinitFailCount = 0;
                Debug.WriteLine("✅ Renderer reinitialized successfully.");
            }
            catch (Exception ex)
            {
                _reinitFailCount++;
                Debug.WriteLine($"❌ Failed to reinitialize renderer: {ex.Message}");
                if (_reinitFailCount >= MaxReinitFailCount)
                {
                    Debug.WriteLine("❌ Maximum reinitialization attempts reached. Consider application restart or user notification.");

                    // 필요시 앱 종료 혹은 알림 로직 삽입
                }
            }
            finally
            {
                _isReinitializing = false;
            }
        }

        _renderThread?.Unregister(this);
        _renderThread?.Register(this);
    }

    public void AddGraphItem(GraphItemBase item)
    {
        lock (_graphItemsLock)
        {
            if (_graphItems.Contains(item) || _pendingItems.Contains(item))
                return;

            if (IsReady)
            {
                GraphItemInitializer.Initialize(item, _device, _context, XOffset, XScale, YScale);
                _graphItems.Add(item);
            }
            else
            {
                _pendingItems.Add(item);
            }
        }
    }

    public  void OnRendererReady()
    {
        lock (_graphItemsLock)
        {
            foreach (var item in _pendingItems)
            {
                GraphItemInitializer.Initialize(item, _device, _context, XOffset, XScale, YScale);
                _graphItems.Add(item);
            }


            _pendingItems.Clear();
        }
    }


    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _renderThread.Unregister(this);
        DisposeResources();
    }
}