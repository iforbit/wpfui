// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using SharpGen.Runtime;

using System.Diagnostics;
using System.Runtime.InteropServices;

using Vortice.Direct3D11;

using Wpf.Ui.DirectX.Models.VertexTypes;
using Wpf.Ui.DirectX.Rendering;

namespace Wpf.Ui.DirectX.Models;

public sealed class GraphLineItem : GraphItemBase
{
    private ID3D11Buffer? _vertexBuffer;

    public int LastDrawCount => VertexCount;

    protected override void OnInitialize(ID3D11Device device)
    {
        int initialBufferSize = 16384; // 16KB로 초기 버퍼 크기 확대
        int initialBufferVertexCount = initialBufferSize / Marshal.SizeOf<VertexPositionColor>();

        Span<VertexPositionColor> span = stackalloc VertexPositionColor[initialBufferVertexCount];

        BufferSizeInBytes = initialBufferSize;
        _vertexBuffer = VertexBufferFactory.CreateVertexBuffer<VertexPositionColor>(
            device, _deviceContext!, span, dynamic: true, overrideSizeInBytes: BufferSizeInBytes);

        VertexCount = 0;
    }

    public override void UpdateVertices(ReadOnlySpan<VertexPositionColor> span)
    {
        lock (_renderUpdateLock)
        {
            if (span.Length == 0 || _disposed || _vertexBuffer == null || _device == null || _deviceContext == null || _deviceContext.NativePointer == IntPtr.Zero)
            {
                Debug.WriteLine("🚫 Skipped UpdateVertices: GPU context not ready or disposed");
                return;
            }

            int sizeInBytes = span.Length * VertexSizeInBytes;

            bool needsResize = sizeInBytes > BufferSizeInBytes || sizeInBytes < (BufferSizeInBytes / 4);

            if (needsResize)
            {
                _vertexBuffer?.Dispose();
                BufferSizeInBytes = Math.Max((int)(sizeInBytes * 1.25f), BufferSizeInBytes);
                _vertexBuffer = VertexBufferFactory.CreateVertexBuffer<VertexPositionColor>(
                    _device, _deviceContext, span, dynamic: true, overrideSizeInBytes: BufferSizeInBytes);
            }

            if (_vertexBuffer.Description.ByteWidth < sizeInBytes)
            {
                Debug.WriteLine($"❌ Upload skipped: Data {sizeInBytes} > Buffer {_vertexBuffer.Description.ByteWidth}");
                return;
            }

            try
            {
                VertexBufferFactory.UploadVertices<VertexPositionColor>(_deviceContext, _vertexBuffer!, span);
                VertexCount = span.Length;
            }
            catch (SharpGenException ex) when (ex.ResultCode.Code == unchecked((int)0x887A0005))
            {
                Debug.WriteLine($"⚠️ GPU device removed: {ex.Message}");
            }
            catch (SEHException ex)
            {
                Debug.WriteLine($"💥 SEHException during UploadVertices: {ex.Message}");
            }
        }
    }

    public override void Update(double time) { }

    public override void Transform(float xOffset, float xScale, float yScale) { }

    private readonly object _renderUpdateLock = new();

    public override void Render(ID3D11DeviceContext context)
    {
        if (_disposed)
        {
            return;
        }

        lock (_renderUpdateLock)
        {
            try
            {
                while (_updateQueue.TryDequeue(out ReadOnlyMemory<VertexPositionColor> memory))
                {
                    UpdateVertices(memory.Span);
                }

                if (_vertexBuffer == null)
                {
                    return;
                }

                int stride = Marshal.SizeOf<VertexPositionColor>();
                int offset = 0;

                Span<uint> strides = stackalloc uint[] { (uint)stride };
                Span<uint> offsets = stackalloc uint[] { (uint)offset };

                context.IASetVertexBuffers(0, new[] { _vertexBuffer }, strides, offsets);
                context.IASetPrimitiveTopology(Vortice.Direct3D.PrimitiveTopology.LineStrip);
                context.Draw((uint)VertexCount, 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Item Render Error] {this.Name}: {ex.Message}");
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _vertexBuffer?.Dispose();
            _vertexBuffer = null;
            VertexCount = 0;
            BufferSizeInBytes = 0;
        }

        _disposed = true;
        base.Dispose(disposing);
    }
}
