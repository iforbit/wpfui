// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.Concurrent;
using System.Runtime.InteropServices;

using Vortice.Direct3D11;
using Vortice.Mathematics;

using Wpf.Ui.DirectX.Models.VertexTypes;

namespace Wpf.Ui.DirectX.Models;

#pragma warning disable SA1401,SA1306 // Fields should be private
public abstract class GraphItemBase : IDisposable
{
    protected readonly ConcurrentQueue<ReadOnlyMemory<VertexPositionColor>> _updateQueue = new();

    protected bool _disposed = false;
    protected bool _initialized = false;

    public bool IsVisible { get; set; } = true;

    public string Name { get; set; } = string.Empty;

    public bool IsDynamic { get; set; } = true;

    protected ID3D11Device? _device;
    protected ID3D11DeviceContext? _deviceContext;

    protected static readonly int VertexSizeInBytes = Marshal.SizeOf<VertexPositionColor>();

    protected int BufferSizeInBytes = 0;
    protected int VertexCount = 0;

    public Color4 GraphColor { get; set; } = new Color4(1f, 1f, 1f, 1f);

    public void SetDevice(ID3D11Device device) => _device = device;

    public void SetContext(ID3D11DeviceContext context) => _deviceContext = context;

    public void Initialize(ID3D11Device device)
    {
        if (_initialized)
        {
            return;
        }

        OnInitialize(device);
        _initialized = true;
    }

    protected abstract void OnInitialize(ID3D11Device device);

    public virtual void EnqueueVertices(ReadOnlySpan<VertexPositionColor> span)
    {
        var buffer = new VertexPositionColor[span.Length];
        span.CopyTo(buffer);
        _updateQueue.Enqueue(buffer.AsMemory());
    }

    public abstract void UpdateVertices(ReadOnlySpan<VertexPositionColor> span);

    public abstract void Update(double time);

    public abstract void Transform(float xOffset, float xScale, float yScale);

    public abstract void Render(ID3D11DeviceContext context);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _device = null;
        _deviceContext = null;
    }
}
#pragma warning restore SA1401, SA1306 // Fields should be private