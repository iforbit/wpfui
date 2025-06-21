// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Vortice.Direct3D11;

using Wpf.Ui.DirectX.Models.VertexTypes;

namespace Wpf.Ui.DirectX.Models;

#pragma warning disable SA1401,SA1306 // Fields should be private
public abstract class GraphItemBase<T> : IGraphItem
    where T : unmanaged
{
    protected readonly ConcurrentQueue<ReadOnlyMemory<T>> _updateQueue = new();

    protected bool _disposed = false;
    protected bool _initialized = false;

    public bool IsVisible { get; set; } = true;

    public virtual PixelShaderType ShaderType { get; set; } = PixelShaderType.PerVertexColor;

    public string Name { get; set; } = string.Empty;

    public bool IsDynamic { get; set; } = true;

    protected ID3D11Device? _device;
    protected ID3D11DeviceContext? _deviceContext;

    private int _vertexSizeInBytes = 0;

    protected int VertexSizeInBytes => _vertexSizeInBytes == 0 ? (_vertexSizeInBytes = Unsafe.SizeOf<T>()) : _vertexSizeInBytes;

    protected int BufferSizeInBytes = 0;
    protected int VertexCount = 0;

    public Vortice.Mathematics.Color4 GraphColor { get; set; } = new(1f, 1f, 1f, 1f);

    protected float _lastXOffset;
    protected float _lastXScale = 1f;
    protected float _lastYScale = 1f;

    public ID3D11Device? Device => _device;

    public ID3D11DeviceContext? Context => _deviceContext;

    public bool IsDisposed => _disposed;

    public bool IsReadyToRender =>
        !_disposed && _initialized &&
        _device?.NativePointer != IntPtr.Zero &&
        _deviceContext?.NativePointer != IntPtr.Zero;

    public bool CanRender() => IsVisible && IsReadyToRender && VertexCount > 0;

    public void FlushQueuedVertices()
    {
        while (_updateQueue.TryDequeue(out ReadOnlyMemory<T> mem))
        {
            UpdateVertices(mem.Span);
        }
    }

    public void SetDisposedFlag(bool value)
    {
        _disposed = value;
        if (!value)
        {
            _initialized = false;
        }
    }

    public void SetDevice(ID3D11Device device) => _device = device;

    public void SetContext(ID3D11DeviceContext context)
    {
        if (_deviceContext != null && !ReferenceEquals(_deviceContext, context))
        {
            Debug.WriteLine("⚠️ Overwriting GraphItem context with a different instance");
        }

        _deviceContext = context;
    }

    public void Initialize(ID3D11Device device, ID3D11DeviceContext context)
    {
        if (_initialized)
        {
            return;
        }

        _device = device;
        _deviceContext = context;

        OnInitialize(device);
        _initialized = true;
    }

    protected abstract void OnInitialize(ID3D11Device device);

    public virtual void EnqueueVertices(ReadOnlySpan<T> span)
    {
        var buffer = new T[span.Length];
        span.CopyTo(buffer);
        _updateQueue.Enqueue(buffer.AsMemory());
    }

    public abstract void UpdateVertices(ReadOnlySpan<T> span);

    public abstract void Update(double time);

    public abstract bool TryGetTransform(out float xOffset, out float xScale);

    public void Transform(float xOffset, float xScale, float yScale, bool force = false)
    {
        if (!force && xOffset == _lastXOffset && xScale == _lastXScale && yScale == _lastYScale)
        {
            return;
        }

        _lastXOffset = xOffset;
        _lastXScale = xScale;
        _lastYScale = yScale;

        OnTransform(xOffset, xScale, yScale);
    }

    protected abstract void OnTransform(float xOffset, float xScale, float yScale);

    public virtual void Render(ID3D11DeviceContext context)
    {
        FlushQueuedVertices();

        if (!CanRender())
        {
            return;
        }

        int stride = VertexSizeInBytes;
        int offset = 0;

        Span<uint> strides = stackalloc uint[] { (uint)stride };
        Span<uint> offsets = stackalloc uint[] { (uint)offset };

        ID3D11Buffer buffer = GetVertexBuffer() ?? throw new InvalidOperationException("VertexBuffer is null.");
        context.IASetVertexBuffers(0, new[] { buffer }, strides, offsets);
        context.IASetPrimitiveTopology(Vortice.Direct3D.PrimitiveTopology.LineStrip);
        context.Draw((uint)VertexCount, 0);
    }

    protected abstract ID3D11Buffer? GetVertexBuffer();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _device = null;
        _deviceContext = null;
        _disposed = true;
    }
}

#pragma warning restore SA1401, SA1306 // Fields should be private