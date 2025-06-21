// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using SharpGen.Runtime;

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

using Wpf.Ui.DirectX.Models;
using Wpf.Ui.DirectX.Models.VertexTypes;
using Wpf.Ui.DirectX.Services;
using Wpf.Ui.DirectX.Threading;

using MapFlags = Vortice.Direct3D11.MapFlags;
using Path = System.IO.Path;

namespace Wpf.Ui.DirectX.Rendering;

public sealed class D3D11Renderer : IRenderable, IDisposable
{
    private const int MaxReinitFailCount = 3;

    private readonly ID3DGraphicsService _graphicsService;
    private readonly IRenderThreadService _renderThread;
    private readonly ID3D11Device _device;
    private readonly ID3D11DeviceContext _context;
    private readonly List<IGraphItem> _graphItems = new();
    private readonly List<IGraphItem> _pendingItems = new();

    private readonly TimeSpan _minReinitInterval = TimeSpan.FromSeconds(5);

    private readonly object _reinitLock = new();
    private readonly object _graphItemsLock = new();
    private readonly object _contextLock = new();

    public bool IsReady =>
        !_disposed &&
        _context != null &&
        _context.NativePointer != IntPtr.Zero &&
        _renderTargetView != null &&
        _swapChain != null &&
        _viewProjectionBuffer != null &&

        // ✅ PerVertexColor용 셰이더 및 레이아웃
        _inputLayout != null &&
        _vertexShader != null &&
        _pixelShaderVertexColor != null &&

        // ✅ ConstantColor용 셰이더 및 레이아웃
        _inputLayoutConst != null &&
        _vertexShaderConst != null &&
        _pixelShaderConstColor != null &&
        _graphColorBuffer != null;


    public bool IsReadyForVertexColor =>
    _inputLayout != null && _vertexShader != null && _pixelShaderVertexColor != null;

    public bool IsReadyForConstColor =>
        _inputLayoutConst != null && _vertexShaderConst != null && _pixelShaderConstColor != null && _graphColorBuffer != null;

    private bool _disposed = false;

    private IDXGISwapChain? _swapChain;
    private ID3D11RenderTargetView? _renderTargetView;

    private ID3D11VertexShader? _vertexShader;
    private ID3D11VertexShader? _vertexShaderConst;
    
    private ID3D11PixelShader? _pixelShaderVertexColor;
    private ID3D11PixelShader? _pixelShaderConstColor;
    private ID3D11InputLayout? _inputLayout;
    private ID3D11InputLayout? _inputLayoutConst;
    private ID3D11Buffer? _viewProjectionBuffer;
    private ID3D11Buffer? _graphColorBuffer;

    public float XOffset { get; private set; } = 0f;

    public float XScale { get; private set; } = 1f;

    public float YOffset { get; private set; } = 0f;

    public float YScale { get; private set; } = 1f;

    private readonly double _width;
    private readonly double _height;
    private readonly IntPtr _hwnd;

    public double Width => _width;

    public double Height => _height;

    public ID3D11Device Device => _device;

    public ID3D11DeviceContext Context => _context;

    private DateTime _lastReinitTime = DateTime.MinValue;
    private int _reinitFailCount = 0;

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
        CreateGraphColorBuffer();
        UpdateViewProjectionBuffer();

        Debug.WriteLine("✅ Renderer resources initialized.");
    }

    private void DisposeResources()
    {
        static void SafeDispose<T>(ref T? resource)
            where T : class, IDisposable
        {
            resource?.Dispose();
            resource = null;
        }

        SafeDispose(ref _vertexShader);
        SafeDispose(ref _vertexShaderConst);
        SafeDispose(ref _pixelShaderVertexColor);
        SafeDispose(ref _pixelShaderConstColor);
        SafeDispose(ref _inputLayout);
        SafeDispose(ref _inputLayoutConst);
        SafeDispose(ref _viewProjectionBuffer);
        SafeDispose(ref _graphColorBuffer);
        SafeDispose(ref _renderTargetView);
        SafeDispose(ref _swapChain);

        Debug.WriteLine("🧹 All GPU resources disposed.");
    }

    private void InitializeShaders()
    {
        string vsPath = Path.Combine(AppContext.BaseDirectory, "Assets", "LineVertexShader.hlsl");
        string vsConstPath = Path.Combine(AppContext.BaseDirectory, "Assets", "ConstVertexShader.hlsl");
        string psVertexPath = Path.Combine(AppContext.BaseDirectory, "Assets", "LinePixelShader.hlsl");
        string psConstPath = Path.Combine(AppContext.BaseDirectory, "Assets", "ConstPixelShader.hlsl");

        Result vsResult = Compiler.CompileFromFile(vsPath, "VSMain", "vs_5_0", out Blob? vsBytecode, out Blob? vsErr);
        Result vsConstResult = Compiler.CompileFromFile(vsConstPath, "VSMain", "vs_5_0", out Blob? vsConstBytecode, out Blob? vsConstErr);

        Result psResult = Compiler.CompileFromFile(psVertexPath, "PSMain", "ps_5_0", out Blob? psVertexBytecode, out Blob? psErr);
        Result constResult = Compiler.CompileFromFile(psConstPath, "PSMain", "ps_5_0", out Blob? psConstBytecode, out Blob? constErr);

        if (vsConstResult.Failure || vsConstBytecode == null)
        {
            throw new InvalidOperationException(vsConstErr?.AsString() ?? "Const Vertex shader error.");
        }

        if (vsResult.Failure || vsBytecode == null)
        {
            throw new InvalidOperationException(vsErr?.AsString() ?? "Vertex shader error.");
        }

        if (psResult.Failure || psVertexBytecode == null)
        {
            throw new InvalidOperationException(psErr?.AsString() ?? "Pixel shader error.");
        }

        if (constResult.Failure || psConstBytecode == null)
        {
            throw new InvalidOperationException(constErr?.AsString() ?? "Const Pixel shader error.");
        }

        _vertexShader = _device.CreateVertexShader(vsBytecode);
        _vertexShaderConst = _device.CreateVertexShader(vsConstBytecode);

        _pixelShaderVertexColor = _device.CreatePixelShader(psVertexBytecode);
        _pixelShaderConstColor = _device.CreatePixelShader(psConstBytecode);

        _inputLayout = _device.CreateInputLayout(
            new InputElementDescription[]
            {
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, 12, 0)
            },
            vsBytecode
        );

        // ✅ 전용 InputLayout: POSITION only
        _inputLayoutConst = _device.CreateInputLayout(
            new InputElementDescription[]
            {
        new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0)
            },
            vsConstBytecode
        );

        vsBytecode.Dispose();
        psVertexBytecode.Dispose();
        psConstBytecode.Dispose();
        vsErr?.Dispose();
        psErr?.Dispose();
        constErr?.Dispose();
    }

    private void CreateGraphColorBuffer()
    {
        var desc = new BufferDescription(
            (uint)Marshal.SizeOf<Color4>(),
            BindFlags.ConstantBuffer,
            ResourceUsage.Dynamic,
            CpuAccessFlags.Write);

        _graphColorBuffer = _device.CreateBuffer(desc);
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

    private void UpdateGraphColorBuffer(Color4 color)
    {
        MappedSubresource mapped = _context.Map(_graphColorBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
        Marshal.StructureToPtr(color, mapped.DataPointer, false);
        _context.Unmap(_graphColorBuffer, 0);
    }

    public void SetTransform(float xOffset, float xScale, float yScale, float yOffset = 0f)
    {
        XOffset = xOffset;
        XScale = xScale;
        YScale = yScale;
        YOffset = yOffset;
        UpdateViewProjectionBuffer();
    }

    private int _isUpdatingViewProjection = 0;

    public void UpdateViewProjectionBuffer()
    {
        if (Interlocked.Exchange(ref _isUpdatingViewProjection, 1) == 1)
        {
            return; // 이미 실행 중
        }

        try
        {
            if (_disposed || _viewProjectionBuffer == null || _context?.NativePointer == IntPtr.Zero)
            {
                Debug.WriteLine("⚠️ Skipped UpdateViewProjectionBuffer: disposed or buffer/context invalid.");
                return;
            }

            if (float.IsNaN(XScale) || float.IsNaN(YScale) || float.IsNaN(XOffset) || float.IsNaN(YOffset))
            {
                Debug.WriteLine("⚠️ Invalid transform values: NaN detected.");
                return;
            }

            try
            {
                var data = new Vector4(XScale, YScale, XOffset, YOffset);
                MappedSubresource mapped = _context.Map(_viewProjectionBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
                Marshal.StructureToPtr(data, mapped.DataPointer, false);
                _context.Unmap(_viewProjectionBuffer, 0);
            }
            catch (SharpGenException ex)
            {
                Debug.WriteLine($"💥 SharpGenException during Map/Unmap: {ex.Message}");
            }
            catch (SEHException ex)
            {
                Debug.WriteLine($"💥 SEHException during Map/Unmap: {ex.Message}");
            }
        }
        finally
        {
            _ = Interlocked.Exchange(ref _isUpdatingViewProjection, 0);
        }
    }

    public bool IsContextValid()
    {
        try
        {
            if (_context == null || _context.NativePointer == IntPtr.Zero || _disposed)
            {
                Debug.WriteLine("❌ Invalid context");
            }

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
            _context.RSSetViewport(new Viewport(0, 0, (int)_width, (int)_height, 0, 1));
            _context.VSSetConstantBuffer(0, _viewProjectionBuffer);

            foreach (IGraphItem item in _graphItems)
            {
                RenderGraphItem(item);
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

    private void RenderGraphItem(IGraphItem item)
    {
        if (!item.IsVisible || !item.IsReadyToRender)
            return;

        // ✅ 픽셀 셰이더 설정
        switch (item.ShaderType)
        {
            case PixelShaderType.ConstantColor:
                _context.VSSetShader(_vertexShaderConst);
                _context.IASetInputLayout(_inputLayoutConst);
                _context.PSSetShader(_pixelShaderConstColor);
                UpdateGraphColorBuffer(item.GraphColor);
                _context.PSSetConstantBuffer(1, _graphColorBuffer);
                break;

            case PixelShaderType.PerVertexColor:
                _context.VSSetShader(_vertexShader);
                _context.IASetInputLayout(_inputLayout);
                _context.PSSetShader(_pixelShaderVertexColor);
                break;

            default:
                Debug.WriteLine($"⚠️ Unknown shader type for {item.Name}");
                return;
        }

        // ❌ Transform은 GraphControl에서만 설정됨
        // ✅ 여기선 렌더링만
        item.Render(_context);
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

    public void AddGraphItem(IGraphItem item)
    {
        lock (_graphItemsLock)
        {
            if (_graphItems.Contains(item) || _pendingItems.Contains(item))
            {
                return;
            }

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

    public void OnRendererReady()
    {
        lock (_graphItemsLock)
        {
            foreach (IGraphItem item in _pendingItems)
            {
                GraphItemInitializer.Initialize(item, _device, _context, XOffset, XScale, YScale);
                _graphItems.Add(item);
            }

            _pendingItems.Clear();
        }
    }
    /*
    public void TryRecover()
    {
        try
        {
            Dispose();
            Initialize(Device); // 또는 TryInitialize()
            _isFaulted = false;
        }
        catch
        {
            _isFaulted = true;
        }
    }
    */
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