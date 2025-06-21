// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using SharpGen.Runtime;

using System.Diagnostics;
using System.Runtime.InteropServices;

using Vortice.Direct3D11;

namespace Wpf.Ui.DirectX.Rendering;

public static class VertexBufferFactory
{
    private static readonly object _contextLock = new();
    private const int MIN_ALLOC_SIZE = 4096;

    public static unsafe ID3D11Buffer CreateVertexBuffer<T>(
       ID3D11Device device,
       ID3D11DeviceContext context,
       ReadOnlySpan<T> vertices,
       bool dynamic = false,
       int? overrideSizeInBytes = null)
        where T : unmanaged
    {
        uint sizeInBytes = (uint)(overrideSizeInBytes ?? (vertices.Length * sizeof(T)));
        if (sizeInBytes == 0)
        {
            throw new ArgumentException("Vertex data size cannot be zero");
        }

        sizeInBytes = Math.Max(sizeInBytes, MIN_ALLOC_SIZE);

        var bufferDesc = new BufferDescription(
            sizeInBytes,
            BindFlags.VertexBuffer,
            dynamic ? ResourceUsage.Dynamic : ResourceUsage.Default,
            dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None);

        if (dynamic)
        {
            ID3D11Buffer buffer = device.CreateBuffer(bufferDesc);
            UploadVertices(device, context, buffer, vertices);
            return buffer;
        }
        else
        {
            fixed (T* vertexPtr = vertices)
            {
                var data = new SubresourceData((IntPtr)vertexPtr);
                SharpGen.Runtime.Result result = device.CreateBuffer(bufferDesc, data, out ID3D11Buffer? buffer);
                result.CheckError();
                return buffer!;
            }
        }
    }

    public static unsafe void UploadVertices<T>(
        ID3D11Device device,
        ID3D11DeviceContext context,
        ID3D11Buffer buffer,
        ReadOnlySpan<T> vertices)
        where T : unmanaged
    {
        lock (_contextLock)
        {
            if (vertices.IsEmpty || buffer == null || context == null)
            {
                return;
            }

            uint sizeInBytes = (uint)(vertices.Length * sizeof(T));
            uint bufferSize;
            try
            {
                bufferSize = buffer.Description.ByteWidth;
            }
            catch (SharpGenException ex)
            {
                Debug.WriteLine($"❌ Error accessing buffer.Description: {ex.Message}");
                return;
            }

            if (sizeInBytes > bufferSize)
            {
                Debug.WriteLine($"❌ UploadVertices aborted: data size ({sizeInBytes}) > buffer size ({bufferSize})");
                return;
            }

            try
            {
                MappedSubresource mapped = context.Map(buffer, 0, MapMode.WriteDiscard, MapFlags.None);
                vertices.CopyTo(new Span<T>((void*)mapped.DataPointer, vertices.Length));
                context.Unmap(buffer, 0);
            }
            catch (SEHException ex)
            {
                Debug.WriteLine($"💥 SEHException during Map/Unmap: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UploadVertices] Exception: {ex.Message}");
            }
        }
    }

    public static unsafe void UploadVertices<T>(
    ID3D11Device device,
    ID3D11DeviceContext context,
    ID3D11Buffer buffer,
    ReadOnlySpan<T> span1,
    ReadOnlySpan<T> span2)
    where T : unmanaged
    {
        lock (_contextLock)
        {
            if ((span1.Length + span2.Length) == 0 || buffer == null || context == null)
            {
                Debug.WriteLine("⚠️ UploadVertices skipped: invalid parameters");
                return;
            }

            uint totalSize = (uint)((span1.Length + span2.Length) * sizeof(T));
            uint bufferSize;

            try
            {
                bufferSize = buffer.Description.ByteWidth;
            }
            catch (SharpGenException ex)
            {
                Debug.WriteLine($"❌ Error accessing buffer.Description: {ex.Message}");
                return;
            }

            if (totalSize > bufferSize)
            {
                Debug.WriteLine($"❌❌ UploadVertices aborted: total data size ({totalSize}) > buffer size ({bufferSize})");
                return;
            }

            try
            {
                MappedSubresource mapped = context.Map(buffer, 0, MapMode.WriteDiscard, MapFlags.None);
                T* dst = (T*)mapped.DataPointer;

                span1.CopyTo(new Span<T>(dst, span1.Length));
                span2.CopyTo(new Span<T>(dst + span1.Length, span2.Length));

                context.Unmap(buffer, 0);
            }
            catch (SEHException ex)
            {
                Debug.WriteLine($"💥 SEHException during Map/Unmap (DualSpan): {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UploadVertices - DualSpan] Exception: {ex.Message}");
            }
        }
    }
}
