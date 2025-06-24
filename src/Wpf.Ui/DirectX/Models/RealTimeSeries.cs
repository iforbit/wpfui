// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

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
                    _lastX = Math.Max(_lastX, x);
                    _minX = Math.Min(_minX, x);
                    _maxX = Math.Max(_maxX, x);
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

    public ReadOnlySpan<T> GetSpanMergedUnsafe()
    {
        lock (_lock)
        {
            _ringBuffer.GetSpans(out Span<T> span1, out Span<T> span2);

            if (span2.IsEmpty)
            {
                return span1;
            }

            // 두 Span을 병합하여 새로운 버퍼에 복사
            int totalLength = span1.Length + span2.Length;
            T[] merged = GC.AllocateUninitializedArray<T>(totalLength); // .NET 5+ 안전한 할당
            span1.CopyTo(merged);
            span2.CopyTo(merged.AsSpan(span1.Length));

            return merged.AsSpan();
        }
    }
}
