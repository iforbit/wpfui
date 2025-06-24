// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using SharpGen.Runtime;

using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

using Wpf.Ui.DirectX.Core;
using Wpf.Ui.DirectX.Helpers;
using Wpf.Ui.DirectX.Models;
using Wpf.Ui.DirectX.Models.VertexTypes;

namespace Wpf.Ui.DirectX.Rendering;

public sealed class RealTimeSeriesRenderer<T> : SeriesRendererBase<T>
    where T : unmanaged
{
    private ID3D11Buffer? _vertexBuffer;
    private int _vertexCount;
    private int _bufferSizeInBytes = 0;

    // 임사
    private ID3D11VertexShader? _vertexShader;
    private ID3D11VertexShader? _vertexShaderConst;
    private ID3D11PixelShader? _pixelShaderVertexColor;
    private ID3D11PixelShader? _pixelShaderConstColor;
    private ID3D11InputLayout? _inputLayout;
    private ID3D11InputLayout? _inputLayoutConst;
    private ID3D11Buffer? _graphColorBuffer;

    public RealTimeSeriesRenderer(RealTimeSeries<T> series, ID3D11Device device, ID3D11DeviceContext context)
        : base(series, device, context)
    {
    }

    public override void Upload()
    {
        if (Series is not RealTimeSeries<T> realSeries || !Series.IsReady)
        {
            return;
        }

        ReadOnlySpan<T> data = realSeries.GetSpanMergedUnsafe();

        // Debug.WriteLine($"🔍 Span.Length = {data.Length}, Span.IsEmpty = {data.IsEmpty}");
        _vertexCount = data.Length;

        if (_vertexCount == 0 || data.IsEmpty)
        {
            if (_vertexBuffer != null)
            {
                Debug.WriteLine("⚠️ Upload skipped: No data. Disposing existing buffer.");
                _vertexBuffer.Dispose();
                _vertexBuffer = null;
                _bufferSizeInBytes = 0;
            }

            return;
        }

        int sizeInBytes = Marshal.SizeOf<T>() * _vertexCount;
        int recommendedSize = (int)MathF.Ceiling(sizeInBytes * 1.5f);

        if (_vertexBuffer == null || _bufferSizeInBytes < sizeInBytes)
        {
            _vertexBuffer?.Dispose();
            _bufferSizeInBytes = recommendedSize;

            Debug.WriteLine($"📦 Creating new buffer (size={_bufferSizeInBytes})");
            _vertexBuffer = VertexBufferFactory.CreateVertexBuffer<T>(
                Device, Context, Span<T>.Empty, true, _bufferSizeInBytes
            );
        }

        _ = VertexBufferFactory.UploadVertices(Device, Context, _vertexBuffer!, data);
    }

    protected override void InitializeShaders()
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

        _vertexShader = Device.CreateVertexShader(vsBytecode);
        _vertexShaderConst = Device.CreateVertexShader(vsConstBytecode);

        _pixelShaderVertexColor = Device.CreatePixelShader(psVertexBytecode);
        _pixelShaderConstColor = Device.CreatePixelShader(psConstBytecode);

        _inputLayout = Device.CreateInputLayout(
            new InputElementDescription[]
            {
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, 12, 0)
            },
            vsBytecode
        );

        // ✅ 전용 InputLayout: POSITION only
        _inputLayoutConst = Device.CreateInputLayout(
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

    protected override void InitializeBuffers()
    {
        // 초기 VertexBuffer 생성
        CreateGraphColorBuffer();
    }

    private void CreateGraphColorBuffer()
    {
        var desc = new BufferDescription(
            (uint)Marshal.SizeOf<Color4>(),
            BindFlags.ConstantBuffer,
            ResourceUsage.Dynamic,
            CpuAccessFlags.Write);

        _graphColorBuffer = Device.CreateBuffer(desc);
    }

    private void UpdateGraphColorBuffer(Color4 color)
    {
        MappedSubresource mapped = Context.Map(_graphColorBuffer, 0, MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None);
        Marshal.StructureToPtr(color, mapped.DataPointer, false);
        Context.Unmap(_graphColorBuffer, 0);
    }

    public override void Draw()
    {
        if (!Series.IsReady || _vertexBuffer == null || _vertexCount == 0)
        {
            return;
        }

        int stride = Marshal.SizeOf<T>();
        int offset = 0;

        Context.VSSetShader(_vertexShaderConst);
        Context.IASetInputLayout(_inputLayoutConst);
        Context.PSSetShader(_pixelShaderConstColor);
        UpdateGraphColorBuffer(Series.GraphColor);
        Context.PSSetConstantBuffer(1, _graphColorBuffer);
        Context.IASetVertexBuffers(0, new[] { _vertexBuffer }, new uint[] { (uint)stride }, new uint[] { (uint)offset });
        Context.IASetPrimitiveTopology(Vortice.Direct3D.PrimitiveTopology.LineStrip);
        Context.Draw((uint)_vertexCount, 0);
    }

    public override DrawKey GetDrawKey()
    {
        return new DrawKey(
            PixelShaderType.ConstantColor,
            Series.GraphColor,
            Vortice.Direct3D.PrimitiveTopology.LineStrip,
            VertexLayoutKey.Get<T>()
            );
    }
}