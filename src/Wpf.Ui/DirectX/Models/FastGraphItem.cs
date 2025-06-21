// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Vortice.Direct3D11;

using Wpf.Ui.DirectX.Models.VertexTypes;
using Wpf.Ui.DirectX.Rendering;

namespace Wpf.Ui.DirectX.Models;

/// <summary>
/// GPU에서만 변환 행렬을 처리하는 고성능 그래프 아이템
/// </summary>
public sealed class FastGraphItem<T> : GraphItemBase<T>
    where T : unmanaged
{
    private const int MAX_BUFFERS = 4;
    private const float RealTimeThreshold = 1.0f;
    private const int BufferSizeAlignment = 1024;

    private readonly ID3D11Buffer?[] _vertexBuffers = new ID3D11Buffer?[MAX_BUFFERS];
    private int _bufferIndex = 0;

    private readonly object _lock = new();
    private readonly RingBuffer<T> _ringBuffer;
    private readonly List<T> _historyCache = new();

    private float _lastX = 0f;
    private float _minX = float.MaxValue;
    private float _maxX = float.MinValue;

    public float LastX => _lastX;

    public float MinX => _minX;

    public float MaxX => _maxX;

    public int TotalVertexCount => _ringBuffer.Count;

    public bool EnableAutoTrim { get; set; } = true;

    public bool UseHistoryCache { get; set; } = false;

    public int MaxBufferLength { get; set; } = 100_000;

    public bool AutoYAxis { get; set; } = true;

    public override PixelShaderType ShaderType { get; set; } = PixelShaderType.ConstantColor;

    public FastGraphItem(int capacity = 8192)
    {
        _ringBuffer = new RingBuffer<T>(capacity);
    }

    private static float GetX(in T point)
    {
        if (typeof(T) == typeof(VertexPosition))
        {
            return Unsafe.As<T, VertexPosition>(ref Unsafe.AsRef(in point)).Position.X;
        }

        if (typeof(T) == typeof(VertexPositionColor))
        {
            return Unsafe.As<T, VertexPositionColor>(ref Unsafe.AsRef(in point)).Position.X;
        }

        throw new NotSupportedException("Unsupported vertex type");
    }

    public void AppendBatch(ReadOnlySpan<T> samples)
    {
        if (!_initialized)
        {
            Debug.WriteLine($"⚠️ AppendPoint skipped: {Name} is not initialized.");
            return;
        }

        lock (_lock)
        {
            _ringBuffer.Append(samples);
            foreach (ref readonly T point in samples)
            {
                _historyCache.Add(point);
                float x = GetX(in point);
                _lastX = Math.Max(_lastX, x);
                _minX = Math.Min(_minX, x);
                _maxX = Math.Max(_maxX, x);
            }
            //Debug.WriteLine($"➕ {Name}: Appended {samples.Length} pts, LastX={_lastX:F2}");
        }
    }

    public override bool TryGetTransform(out float xOffset, out float xScale)
    {
        xOffset = 0f;
        xScale = 1f;
        return false; // ✅ 사용 금지
    }

    /// <summary>
    /// 현재 시점에 자동 스케일/오프셋 계산
    /// </summary>
    private void UpdateTransformAuto(float visibleX, float visibleY)
    {
        float xEnd = _lastX;
        float xStart = xEnd - visibleX;

        // y-range 계산
        GetYRangeInRange(xStart, xEnd, out float minY, out float maxY);

        float yScale = visibleY / (maxY - minY + 1e-5f);
        _ = -minY * yScale;

        float xScale = 1.0f;
        float xOffset = -xStart;

        this.Transform(xOffset, xScale, yScale);
    }

    /// <summary>
    /// 범위 내 y 최소/최대 계산 (VertexPosition or VertexPositionColor)
    /// </summary>
    public void GetYRangeInRange(float minX, float maxX, out float minY, out float maxY)
    {
        minY = float.MaxValue;
        maxY = float.MinValue;

        _ringBuffer.GetSpans(out Span<T> span1, out Span<T> span2);

        FilterSpanAndExtractY(span1, minX, maxX, ref minY, ref maxY);
        FilterSpanAndExtractY(span2, minX, maxX, ref minY, ref maxY);

        if (minY == float.MaxValue || maxY == float.MinValue)
        {
            minY = -1f;
            maxY = 1f;
        }
    }

    private void ProcessSpan(ReadOnlySpan<T> span, ref float minY, ref float maxY)
    {
        if (typeof(T) == typeof(VertexPosition))
        {
            foreach (ref readonly T v in span)
            {
                VertexPosition p = Unsafe.As<T, VertexPosition>(ref Unsafe.AsRef(v));
                minY = Math.Min(minY, p.Position.Y);
                maxY = Math.Max(maxY, p.Position.Y);
            }
        }
        else if (typeof(T) == typeof(VertexPositionColor))
        {
            foreach (ref readonly T v in span)
            {
                VertexPositionColor p = Unsafe.As<T, VertexPositionColor>(ref Unsafe.AsRef(v));
                minY = Math.Min(minY, p.Position.Y);
                maxY = Math.Max(maxY, p.Position.Y);
            }
        }
    }

    public override void UpdateVertices(ReadOnlySpan<T> span)
    {
        throw new NotSupportedException("Use AppendBatch() instead.");
    }

    public override void Update(double time) { }

    private void FilterSpanAndExtractY(ReadOnlySpan<T> span, float minX, float maxX, ref float minY, ref float maxY)
    {
        if (typeof(T) == typeof(VertexPosition))
        {
            foreach (ref readonly T v in span)
            {
                VertexPosition p = Unsafe.As<T, VertexPosition>(ref Unsafe.AsRef(v));
                if (p.Position.X >= minX && p.Position.X <= maxX)
                {
                    minY = Math.Min(minY, p.Position.Y);
                    maxY = Math.Max(maxY, p.Position.Y);
                }
            }
        }
        else if (typeof(T) == typeof(VertexPositionColor))
        {
            foreach (ref readonly T v in span)
            {
                VertexPositionColor p = Unsafe.As<T, VertexPositionColor>(ref Unsafe.AsRef(v));
                if (p.Position.X >= minX && p.Position.X <= maxX)
                {
                    minY = Math.Min(minY, p.Position.Y);
                    maxY = Math.Max(maxY, p.Position.Y);
                }
            }
        }
    }

    private void FilterHistoryCache(float minX, float maxX, out T[] result)
    {
        List<T> list = new();
        foreach (T point in _historyCache)
        {
            float x = GetX(in point);
            Debug.WriteLine($"🧪 Point X = {x}");
            if (x >= minX && x <= maxX)
            {
                list.Add(point);
            }
        }

        result = list.ToArray();
    }

    private void FilterSpanInRange(Span<T> source, float minX, float maxX, ref List<T> result)
    {
        if (typeof(T) == typeof(VertexPosition))
        {
            foreach (ref readonly T v in source)
            {
                VertexPosition p = Unsafe.As<T, VertexPosition>(ref Unsafe.AsRef(v));
                if (p.Position.X >= minX && p.Position.X <= maxX)
                {
                    result.Add(v);
                }
            }
        }
        else if (typeof(T) == typeof(VertexPositionColor))
        {
            foreach (ref readonly T v in source)
            {
                VertexPositionColor p = Unsafe.As<T, VertexPositionColor>(ref Unsafe.AsRef(v));
                if (p.Position.X >= minX && p.Position.X <= maxX)
                {
                    result.Add(v);
                }
            }
        }
    }

    public override void Render(ID3D11DeviceContext context)
    {
        if (!IsReadyToRender || !IsVisible) return;

        try
        {
            lock (_lock)
            {
                float minX = _lastXOffset;
                float maxX = _lastXOffset + (_lastXScale * 30f);

                Span<T> span = Span<T>.Empty;
                T[]? fullSpan = null;
                int totalCount = 0;

                if (UseHistoryCache)
                {
                    fullSpan = _historyCache.Where(p => {
                        float x = GetX(in p);
                        return x >= minX && x <= maxX;
                    }).ToArray();
                    totalCount = fullSpan.Length;
                }
                else if ((_lastX - maxX) < RealTimeThreshold)
                {
                    List<T> filtered = new();
                    foreach (ref readonly T point in _ringBuffer.AsSpanUnsafe())
                    {
                        float x = GetX(in point);
                        if (x >= minX && x <= maxX)
                        {
                            filtered.Add(point);
                        }
                    }
                    fullSpan = filtered.ToArray();
                    totalCount = fullSpan.Length;
                }
         

                if (totalCount == 0)
                {
                    Debug.WriteLine($"🚫 No vertices to render for {Name}. Computed minX={minX}, maxX={maxX}, but no vertices found.");
                    return;
                }

                Debug.WriteLine($"🎨 Rendering {totalCount} vertices for {Name} in range {minX:F2} ~ {maxX:F2}");
                EnsureBufferIfNeeded(context, totalCount);
                ID3D11Buffer buffer = _vertexBuffers[_bufferIndex]!;
               if (fullSpan == null || fullSpan.Length == 0)
                {
                    Debug.WriteLine($"⚠️ Upload skipped: fullSpan is null or empty for {Name}");
                    VertexCount = 0;
                    return;
                }

                bool uploaded = VertexBufferFactory.TryUploadVertices<T>(Device!, context, buffer, fullSpan,Name);
                if (!uploaded)
                {
                    Debug.WriteLine($"⚠️ Upload failed: VertexCount will be zero for {Name}");
                    VertexCount = 0;
                    return;
                }
                VertexCount = totalCount;
                Draw(context);
                AdvanceBuffer();
            }
        }
        catch (SEHException ex)
        {
            Debug.WriteLine($"💥 SEHException during FastGraphItem.Render: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"💥 General Exception in FastGraphItem.Render: {ex.Message}");
        }
    }

    private void Draw(ID3D11DeviceContext context)
    {
        if (!CanRender()) return;

        int drawBufferIndex = (_bufferIndex - 1 + MAX_BUFFERS) % MAX_BUFFERS;
        ID3D11Buffer? buffer = _vertexBuffers[drawBufferIndex];

        if (buffer == null || buffer.NativePointer == IntPtr.Zero)
        {
            Debug.WriteLine("❌ FastGraphItem.Draw: buffer null or invalid");
            return;
        }

        Debug.WriteLine($"🛠 Uploading to buffer[{_bufferIndex}] for {VertexCount} vertices");

        int stride = VertexSizeInBytes;
        int offset = 0;
        Span<uint> strides = stackalloc uint[] { (uint)stride };
        Span<uint> offsets = stackalloc uint[] { (uint)offset };

        context.IASetVertexBuffers(0, new[] { buffer }, strides, offsets);
        context.IASetPrimitiveTopology(Vortice.Direct3D.PrimitiveTopology.LineStrip);
        context.Draw((uint)VertexCount, 0);
    }


    private void EnsureBufferIfNeeded(ID3D11DeviceContext context, int vertexCount)
    {
        int requiredSize = vertexCount * VertexSizeInBytes;
        int suggestedSize = Align((int)(requiredSize * 1.25f));

        Debug.WriteLine($"[EnsureBuffer] Requested: {vertexCount} → {suggestedSize} bytes");

        if (BufferSizeInBytes >= suggestedSize)
        {
            Debug.WriteLine($"[EnsureBuffer] Existing buffer {BufferSizeInBytes} OK for {requiredSize} bytes");
            return;
        }

        BufferSizeInBytes = suggestedSize;

        for (int i = 0; i < MAX_BUFFERS; i++)
        {
            _vertexBuffers[i]?.Dispose();
            _vertexBuffers[i] = VertexBufferFactory.CreateVertexBuffer<T>(
                Device!, context,
                Span<T>.Empty,
                dynamic: true,
                overrideSizeInBytes: BufferSizeInBytes);

            Debug.WriteLine($"✅ Recreated buffer[{i}] size={_vertexBuffers[i]?.Description.ByteWidth ?? 0}");
        }
    }

    private static int Align(int size)
    {
        return ((size + BufferSizeAlignment - 1) / BufferSizeAlignment) * BufferSizeAlignment;
    }
    private void AdvanceBuffer()
    {
        _bufferIndex = (_bufferIndex + 1) % MAX_BUFFERS;
    }

    protected override ID3D11Buffer? GetVertexBuffer()
    {
        return _vertexBuffers[(_bufferIndex - 1 + MAX_BUFFERS) % MAX_BUFFERS];
    }

    protected override void OnInitialize(ID3D11Device device)
    {
        _bufferIndex = 0;
        for (int i = 0; i < MAX_BUFFERS; i++)
        {
            _vertexBuffers[i]?.Dispose();
            _vertexBuffers[i] = null;
        }

        BufferSizeInBytes = 0;
        VertexCount = 0;
        _ringBuffer.Clear();
        _historyCache.Clear();

        Debug.WriteLine("⚠️ OnInitialize.");
    }

    protected override void OnTransform(float xOffset, float xScale, float yScale) { }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (ID3D11Buffer? buf in _vertexBuffers)
            {
                buf?.Dispose();
            }
        }

        base.Dispose(disposing);
    }
}
