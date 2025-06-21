// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.DirectX.Models;

/// <summary>
/// 고정 크기의 Span 기반 순환 버퍼 (RingBuffer)
/// GC 최소화 및 실시간 그래프 데이터 처리용
/// </summary>
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

    /// <summary>
    /// 새로운 데이터를 추가합니다. 필요 시 오래된 데이터를 덮어씁니다.
    /// </summary>
    public void Add(T item)
    {
        _buffer[_head] = item;
        _head = (_head + 1) % _buffer.Length;
        if (_count < _buffer.Length)
        {
            _count++;
        }
    }

    /// <summary>
    /// 여러 항목을 한 번에 추가합니다. 내부에서 순환 구조를 유지하며 초과 시 덮어씁니다.
    /// </summary>
    public void Append(ReadOnlySpan<T> items)
    {
        int n = items.Length;
        if (n == 0)
        {
            return;
        }

        int capacity = _buffer.Length;

        // 전체 추가가 용량을 초과할 경우 가장 최신 항목들만 유지
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

    /// <summary>
    /// 현재 순환 버퍼에서 연속된 최신 데이터를 반환합니다.
    /// 두 개의 Span으로 나눠질 수 있습니다.
    /// </summary>
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
    /// 전체 데이터를 리스트로 반환합니다. (필터링 시 활용)
    /// </summary>
    public List<T> ToList()
    {
        List<T> result = new(_count);
        GetSpans(out Span<T> span1, out Span<T> span2);
        result.AddRange(span1.ToArray());
        result.AddRange(span2.ToArray());
        return result;
    }

    public void Clear()
    {
        _head = 0;
        _count = 0;
    }

    public ReadOnlySpan<T> AsSpanUnsafe() => _buffer.AsSpan(0, _count); // 디버깅 전용
}