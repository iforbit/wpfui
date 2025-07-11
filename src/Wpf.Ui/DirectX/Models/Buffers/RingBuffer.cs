// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.DirectX.Models.Buffers;

/// <summary>
/// 고정 크기의 Span 기반 순환 버퍼 (RingBuffer)
/// GC 최소화 및 실시간 그래프 데이터 처리용
/// </summary>
/// <typeparam name="T">The type</typeparam>
public sealed class RingBuffer<T>
    where T : unmanaged
{
    private readonly T[] _buffer;
    private int _head;
    private int _count;

    public RingBuffer(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        _buffer = new T[capacity];
    }

    public int Count => _count;

    public int Capacity => _buffer.Length;

    public void Append(ReadOnlySpan<T> items)
    {
        int n = items.Length;
        if (n == 0)
        {
            return;
        }

        int capacity = _buffer.Length;
        if (n >= capacity)
        {
            items = items.Slice(n - capacity);
            n = capacity;
        }

        int firstCopy = Math.Min(capacity - _head, n);
        items.Slice(0, firstCopy).CopyTo(_buffer.AsSpan(_head, firstCopy));

        int remaining = n - firstCopy;
        if (remaining > 0)
        {
            items.Slice(firstCopy).CopyTo(_buffer.AsSpan(0, remaining));
        }

        _head = (_head + n) % capacity;
        _count = Math.Min(_count + n, capacity);
    }

    public void GetSpans(out Span<T> span1, out Span<T> span2)
    {
        if (_count == 0)
        {
            span1 = span2 = Span<T>.Empty;
            return;
        }

        int start = (_head - _count + Capacity) % Capacity;
        if (start + _count <= Capacity)
        {
            span1 = _buffer.AsSpan(start, _count);
            span2 = Span<T>.Empty;
        }
        else
        {
            int right = Capacity - start;
            int left = _count - right;
            span1 = _buffer.AsSpan(start, right);
            span2 = _buffer.AsSpan(0, left);
        }
    }

    /// <summary>
    /// 외부 제공 버퍼에 병합 데이터를 복사합니다. 반환된 Span은 병합된 연속 메모리입니다.
    /// </summary>
    public ReadOnlySpan<T> CopyMergedTo(Span<T> target)
    {
        GetSpans(out Span<T> span1, out Span<T> span2);
        int total = span1.Length + span2.Length;

        if (target.Length < total)
        {
            throw new ArgumentException("Target span too small.");
        }

        span1.CopyTo(target);
        span2.CopyTo(target.Slice(span1.Length));
        return target.Slice(0, total);
    }

    public void Clear()
    {
        _head = 0;
        _count = 0;
    }

    public ReadOnlySpan<T> AsSpanUnsafe() => _buffer.AsSpan(0, _count); // 디버깅 전용
}