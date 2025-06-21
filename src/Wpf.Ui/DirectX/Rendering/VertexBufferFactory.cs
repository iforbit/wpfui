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

            // ✅ Span이 비어 있으면 업로드 생략
            if (!vertices.IsEmpty)
            {
                UploadVertices(device, context, buffer, vertices);
            }
            else
            {
                Debug.WriteLine($"⚠️ Skipped Upload: Empty span in CreateVertexBuffer<{typeof(T).Name}>");
            }

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

        public static bool TryUploadVertices<T>(ID3D11Device device, ID3D11DeviceContext context, ID3D11Buffer buffer, T[]? source, string name) where T : unmanaged
        {
            if (source == null || source.Length == 0)
            {
                Debug.WriteLine($"⚠️ Upload skipped: fullSpan is null or empty for {name}");
                return false;
            }

            ReadOnlySpan<T> span = source.AsSpan();
            if (span.IsEmpty)
            {
                Debug.WriteLine($"⚠️ Upload skipped: span is empty for {name}");
                return false;
            }

            return UploadVertices(device, context, buffer, span);
        }

    public static unsafe bool UploadVertices<T>(
        ID3D11Device device,
        ID3D11DeviceContext context,
        ID3D11Buffer buffer,
        ReadOnlySpan<T> vertices)
        where T : unmanaged
    {
        lock (_contextLock)
        {
            if (vertices.IsEmpty)
            {
                Debug.WriteLine("⚠️ UploadVertices skipped: vertices.IsEmpty");
                return false;
            }
            if (buffer == null)
            {
                Debug.WriteLine("⚠️ UploadVertices skipped: buffer is null");
                return false;
            }
            if (context == null)
            {
                Debug.WriteLine("⚠️ UploadVertices skipped: context is null");
                return false;
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
                return false;
            }

            if (sizeInBytes > bufferSize)
            {
                Debug.WriteLine($"❌ UploadVertices aborted: data size ({sizeInBytes}) > buffer size ({bufferSize})");
                return false;
            }

            try
            {
                MappedSubresource mapped = context.Map(buffer, 0, MapMode.WriteDiscard, MapFlags.None);
                vertices.CopyTo(new Span<T>((void*)mapped.DataPointer, vertices.Length));
                context.Unmap(buffer, 0);
                return true;
            }
            catch (SEHException ex)
            {
                Debug.WriteLine($"💥 SEHException during Map/Unmap: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UploadVertices] Exception: {ex.Message}");
                return false;
            }
        }
    }

    public static unsafe bool UploadVertices<T>(
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
                return false;
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
                return false;
            }

            if (totalSize > bufferSize)
            {
                Debug.WriteLine($"❌❌ UploadVertices aborted: total data size ({totalSize}) > buffer size ({bufferSize})");
                return false;
            }

            try
            {
                MappedSubresource mapped = context.Map(buffer, 0, MapMode.WriteDiscard, MapFlags.None);
                T* dst = (T*)mapped.DataPointer;

                span1.CopyTo(new Span<T>(dst, span1.Length));
                span2.CopyTo(new Span<T>(dst + span1.Length, span2.Length));

                context.Unmap(buffer, 0);
                return true;
            }
            catch (SEHException ex)
            {
                Debug.WriteLine($"💥 SEHException during Map/Unmap (DualSpan): {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UploadVertices - DualSpan] Exception: {ex.Message}");
                return false;
            }
        }
    }
}
