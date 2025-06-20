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
    private readonly object _renderUpdateLock = new();
    private readonly ChunkedVertexBuffer _chunkBuffer = new();
    public int LastDrawCount => VertexCount;
    public float LastX => _chunkBuffer.LastX;
    public void AppendPoint(float x, float y)
    {
        _chunkBuffer.AppendPoint(x, y, GraphColor); // ✅ 내부 색상 적용
    }

    protected override void OnInitialize(ID3D11Device device)
    {
        BufferSizeInBytes = 16384;
        Span<VertexPositionColor> initial = stackalloc VertexPositionColor[BufferSizeInBytes / VertexSizeInBytes];

        _vertexBuffer = VertexBufferFactory.CreateVertexBuffer<VertexPositionColor>(
            device, Context!, initial, dynamic: true, overrideSizeInBytes: BufferSizeInBytes);

        VertexCount = 0;
    }

    public override void Update(double time)
    {
        if (Context == null || Context.NativePointer == IntPtr.Zero || _chunkBuffer == null || _vertexBuffer == null)
        {
            Debug.WriteLine("🚫 Upload aborted: context or buffer is invalid");
            return;
        }

        float minX = _lastXOffset; // Transform에서 설정된 값 사용
        float maxX = _lastXOffset + _lastXScale * _chunkBuffer.MaxVisibleRange;

        Span<VertexPositionColor> span = stackalloc VertexPositionColor[8192];
        int count = _chunkBuffer.CopyVerticesInRange(minX, maxX, span);
        if (count > 0)
        {
            Debug.WriteLine($"[Update] {Name} VisibleX: {minX:F3}~{maxX:F3}, count={count}");
            UpdateVertices(span[..count]);
        }
        else
        {
            Debug.WriteLine($"[Update] {Name} No visible vertices in range {minX:F3}~{maxX:F3}");
        }
    }

    public override void UpdateVertices(ReadOnlySpan<VertexPositionColor> span)
    {
        lock (_renderUpdateLock)
        {
            if (span.Length == 0 || _vertexBuffer == null || Context == null)
            {
                Debug.WriteLine("🚫 Skipped UpdateVertices: context or buffer not ready");
                return;
            }

            int sizeInBytes = span.Length * VertexSizeInBytes;
            bool needsResize = sizeInBytes > BufferSizeInBytes || sizeInBytes < BufferSizeInBytes / 4;

            if (needsResize)
            {
                _vertexBuffer?.Dispose();
                BufferSizeInBytes = Math.Max((int)(sizeInBytes * 1.25f), BufferSizeInBytes);
                _vertexBuffer = VertexBufferFactory.CreateVertexBuffer(
                    Device!, Context!, span,
                    dynamic: true,
                    overrideSizeInBytes: BufferSizeInBytes);
            }

            try
            {
                VertexBufferFactory.UploadVertices(Device!, Context!, _vertexBuffer!, span);
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


    protected override void OnTransform(float xOffset, float xScale, float yScale)
    {
        // 필요 시 구현하세요 (ex: 변환 캐싱, 렌더링 설정 등)
    }

    protected override ID3D11Buffer? GetVertexBuffer() => _vertexBuffer;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _vertexBuffer?.Dispose();
            _vertexBuffer = null;
            VertexCount = 0;
            BufferSizeInBytes = 0;
        }

        base.Dispose(disposing);
    }
}
