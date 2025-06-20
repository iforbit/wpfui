// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Diagnostics;
using System.Runtime.InteropServices;

using Vortice.Direct3D11;

using Wpf.Ui.DirectX.Models.VertexTypes;

namespace Wpf.Ui.DirectX.Rendering;

public static class VertexBufferFactory
{
    private static readonly int VertexSizeInBytes = Marshal.SizeOf<VertexPositionColor>();
    private static readonly object _contextLock = new();

    public static unsafe ID3D11Buffer CreateVertexBuffer<T>(
       ID3D11Device device,
       ID3D11DeviceContext context,
       ReadOnlySpan<T> vertices,
       bool dynamic = false,
       int? overrideSizeInBytes = null)
        where T : unmanaged
    {
        if (device == null)
        {
            throw new ArgumentNullException(nameof(device));
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Context 유효성 사전 검증
        if (!IsContextSafe(context))
        {
            throw new InvalidOperationException("Context is not safe for operation");
        }

        if (vertices.IsEmpty)
        {
            vertices = new[] { default(T) };
        }

        uint sizeInBytes = (uint)(overrideSizeInBytes ?? (vertices.Length * sizeof(T)));
        if (sizeInBytes == 0)
        {
            throw new ArgumentException("Vertex data size cannot be zero");
        }

        Debug.WriteLine($"[CreateVertexBuffer] Creating buffer with size {sizeInBytes} bytes (override: {overrideSizeInBytes})");

        var bufferDesc = new BufferDescription(
            sizeInBytes,
            BindFlags.VertexBuffer,
            dynamic ? ResourceUsage.Dynamic : ResourceUsage.Default,
            dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None);

        if (dynamic)
        {
            ID3D11Buffer? buffer = null;
            try
            {
                buffer = device.CreateBuffer(bufferDesc);
                if (buffer == null)
                {
                    throw new InvalidOperationException("Failed to create DirectX buffer");
                }

                MappedSubresource mapped;
                try
                {
                    mapped = context.Map(buffer, 0, MapMode.WriteDiscard, MapFlags.None);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Map failed: {ex.Message}");
                    buffer.Dispose();
                    throw;
                }

                if (mapped.DataPointer == IntPtr.Zero)
                {
                    Debug.WriteLine("❌ CreateVertexBuffer: mapped.DataPointer is null.");
                    buffer.Dispose();
                    throw new InvalidOperationException("Failed to map GPU buffer - DataPointer is null");
                }

                try
                {
                    if ((uint)(vertices.Length * sizeof(T)) > bufferDesc.ByteWidth)
                    {
                        Debug.WriteLine("❌ CreateVertexBuffer: vertex data size exceeds buffer size");
                        buffer.Dispose();
                        throw new InvalidOperationException("Vertex data exceeds allocated GPU buffer size");
                    }

                    vertices.CopyTo(new Span<T>((void*)mapped.DataPointer, vertices.Length));
                }
                finally
                {
                    context.Unmap(buffer, 0);
                }

                return buffer;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Dynamic buffer creation failed: {ex.Message}");
                buffer?.Dispose();
                throw;
            }
        }
        else
        {
            try
            {
                fixed (T* vertexPtr = vertices)
                {
                    if (vertexPtr == null)
                    {
                        throw new InvalidOperationException("Source vertex pointer is null");
                    }

                    var data = new SubresourceData((IntPtr)vertexPtr);
                    SharpGen.Runtime.Result result = device.CreateBuffer(bufferDesc, data, out ID3D11Buffer? staticBuffer);
                    result.CheckError();

                    if (staticBuffer == null)
                    {
                        throw new InvalidOperationException("Failed to create static buffer");
                    }

                    return staticBuffer;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Static buffer creation failed: {ex.Message}");
                throw;
            }
        }
    }

    private static bool IsContextSafe(ID3D11DeviceContext context)
    {
        try
        {
            return context != null &&
                   context.NativePointer != IntPtr.Zero;
        }
        catch
        {
            return false;
        }
    }

    public static unsafe void UploadVertices<T>(
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
            uint bufferSize = buffer.Description.ByteWidth;

            if (sizeInBytes > bufferSize)
            {
                Debug.WriteLine($"❌ UploadVertices aborted: data size ({sizeInBytes}) > buffer size ({bufferSize})");
                return;
            }

            MappedSubresource mapped;
            try
            {
                if (!IsContextSafe(context))
                {
                    throw new InvalidOperationException("Context is not safe for operation");
                }

                mapped = context.Map(buffer, 0, MapMode.WriteDiscard, MapFlags.None);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Map failed: {ex.Message}");
                return;
            }

            try
            {
                if (mapped.DataPointer == IntPtr.Zero)
                {
                    Debug.WriteLine("❌ mapped.DataPointer is null");
                    return;
                }

                vertices.CopyTo(new Span<T>((void*)mapped.DataPointer, vertices.Length));
            }
            finally
            {
                try
                {
                    context.Unmap(buffer, 0);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unmap failed: {ex.Message}");
                }
            }
        }
    }
}
