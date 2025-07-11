// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Buffers;
using System.Runtime.CompilerServices;

using Wpf.Ui.DirectX.Models.Buffers;
using Wpf.Ui.DirectX.Models.VertexTypes;

namespace Wpf.Ui.DirectX.Models;

public sealed class RealTimeSeries<T> : GraphSeries<T>
    where T : unmanaged
{
    private readonly RingBuffer<T> _ringBuffer;
    private readonly List<T> _historyCache = new();
    private readonly object _lock = new();

    private T[]? _uploadBuffer;
    private readonly int _uploadBufferSize;

    public int TotalVertexCount => _ringBuffer.Count;

    public bool EnableAutoTrim { get; set; } = true;

    public bool UseHistoryCache { get; set; } = false;

    public int MaxBufferLength { get; set; } = 100_000;

    public RealTimeSeries(int capacity = 8192)
    {
        _ringBuffer = new RingBuffer<T>(capacity);
    }

    protected override void OnInitialize()
    {
        _ringBuffer.Clear();
        _historyCache.Clear();
        _lastX = 0f;
        _minX = float.MaxValue;
        _maxX = float.MinValue;
    }

    public override void Append(ReadOnlySpan<T> data)
    {
        lock (_lock)
        {
            _ringBuffer.Append(data);
            foreach (ref readonly T point in data)
            {
                if (typeof(T) == typeof(VertexPosition))
                {
                    float x = Unsafe.As<T, VertexPosition>(ref Unsafe.AsRef(in point)).Position.X;
                    _ = Unsafe.As<T, VertexPosition>(ref Unsafe.AsRef(in point)).Position.Y;
                    _lastX = Math.Max(_lastX, x);
                    _minX = Math.Min(_minX, x);
                    _maxX = Math.Max(_maxX, x);

                    // Debug.WriteLine($"🛑 Append Name:{Name}  Data:x={x}, y={y}");
                }
                else if (typeof(T) == typeof(VertexPositionColor))
                {
                    float x = Unsafe.As<T, VertexPositionColor>(ref Unsafe.AsRef(in point)).Position.X;
                    _lastX = Math.Max(_lastX, x);
                    _minX = Math.Min(_minX, x);
                    _maxX = Math.Max(_maxX, x);
                }

                if (UseHistoryCache)
                {
                    _historyCache.Add(point);
                }
            }
        }
    }

    public void GetYRange(float minX, float maxX, out float minY, out float maxY)
    {
        lock (_lock)
        {
            minY = float.MaxValue;
            maxY = float.MinValue;

            _ringBuffer.GetSpans(out Span<T> span1, out Span<T> span2);
            ExtractY(span1, minX, maxX, ref minY, ref maxY);
            ExtractY(span2, minX, maxX, ref minY, ref maxY);

            if (minY == float.MaxValue || maxY == float.MinValue)
            {
                minY = -1f;
                maxY = 1f;
            }
        }
    }

    public void GetRecentYRange(int recentCount, out float minY, out float maxY)
    {
        lock (_lock)
        {
            ReadOnlySpan<T> span = _ringBuffer.AsSpanUnsafe(); // 최신순 정렬 가정
            int count = Math.Min(recentCount, span.Length);
            minY = float.MaxValue;
            maxY = float.MinValue;

            for (int i = span.Length - count; i < span.Length; i++)
            {
                if (typeof(T) == typeof(VertexPosition))
                {
                    float y = Unsafe.As<T, VertexPosition>(ref Unsafe.AsRef(in span[i])).Position.Y;
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y);
                }
                else if (typeof(T) == typeof(VertexPositionColor))
                {
                    float y = Unsafe.As<T, VertexPositionColor>(ref Unsafe.AsRef(in span[i])).Position.Y;
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y);
                }
            }

            if (minY == float.MaxValue || maxY == float.MinValue)
            {
                minY = -1f;
                maxY = 1f;
            }
        }
    }

    // Commented 2025-06-24
    //  속도 문제 발생시 (Strategy 패턴 또는 switch delegate 캐싱 등)
    private void ExtractY(ReadOnlySpan<T> span, float minX, float maxX, ref float minY, ref float maxY)
    {
        foreach (ref readonly T point in span)
        {
            float x, y;
            if (typeof(T) == typeof(VertexPosition))
            {
                VertexPosition p = Unsafe.As<T, VertexPosition>(ref Unsafe.AsRef(in point));
                x = p.Position.X;
                y = p.Position.Y;
            }
            else if (typeof(T) == typeof(VertexPositionColor))
            {
                VertexPositionColor p = Unsafe.As<T, VertexPositionColor>(ref Unsafe.AsRef(in point));
                x = p.Position.X;
                y = p.Position.Y;
            }
            else
            {
                continue;
            }

            if (x >= minX && x <= maxX)
            {
                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);
            }
        }
    }

    public ReadOnlySpan<T> GetSpanUnsafe()
    {
        lock (_lock)
        {
            ReadOnlySpan<T> span = _ringBuffer.AsSpanUnsafe(); // 또는 내부 버퍼 직접 노출

            // Debug.WriteLine($"[GetSpanUnsafe] Count={span.Length}, IsEmpty={span.IsEmpty}");
            return span;
        }
    }

    // 병합이 필요한 이유는 단절 방지를 위한 "시각적 연속성 유지" 때문임
    public ReadOnlySpan<T> GetSpanMergedUnsafeCached()
    {
        lock (_lock)
        {
            _ringBuffer.GetSpans(out Span<T> span1, out Span<T> span2);

            if (span2.IsEmpty)
            {
                return span1;
            }

            int totalLength = span1.Length + span2.Length;

            if (totalLength <= 512)
            {
                // 💡 빠른 경량 병합 (GC는 문제 없음)
                _uploadBuffer = GC.AllocateUninitializedArray<T>(totalLength);
                span1.CopyTo(_uploadBuffer);
                span2.CopyTo(_uploadBuffer.AsSpan(span1.Length));
                return _uploadBuffer.AsSpan(0, totalLength);
            }
            else
            {
                if (_uploadBuffer == null || _uploadBuffer.Length < totalLength)
                {
                    if (_uploadBuffer != null)
                    {
                        ArrayPool<T>.Shared.Return(_uploadBuffer);
                    }

                    _uploadBuffer = ArrayPool<T>.Shared.Rent(totalLength);
                }

                return _ringBuffer.CopyMergedTo(_uploadBuffer.AsSpan(0, totalLength));
            }
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _ringBuffer.Clear();
            _lastX = 0f;
            _minX = float.MaxValue;
            _maxX = float.MinValue;
            _historyCache?.Clear();
        }
    }
}
