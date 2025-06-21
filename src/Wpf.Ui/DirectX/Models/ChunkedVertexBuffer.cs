// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Diagnostics;

namespace Wpf.Ui.DirectX.Models;

public sealed class ChunkedVertexBuffer<T> : IDisposable
    where T : unmanaged
{
    private const int ChunkSize = 16_384;

    private readonly ReaderWriterLockSlim _rwLock = new();
    private readonly List<List<T>> _chunks = new();
    private List<T> _currentChunk = new();

    private readonly Func<T, float> _xSelector;
    private float _lastX = 0f;
    private int _totalCount = 0;

    public float LastX => _lastX;

    public float XStep { get; set; } = 0.05f;

    public int TotalCount => _totalCount;

    public int MaxBufferLength { get; set; } = 100_000;

    public float MaxVisibleRange { get; set; } = 30f;

    public bool AutoTrim { get; set; } = true;

    public ChunkedVertexBuffer(int capacity, Func<T, float> xSelector)
    {
        MaxBufferLength = capacity;
        _xSelector = xSelector;
    }

    public void Append(T point)
    {
        _rwLock.EnterWriteLock();
        try
        {
            float x = _xSelector(point);
            if (x < _lastX)
            {
                Debug.WriteLine($"⚠️ Non-monotonic X detected: {x} < {_lastX}");
                return;
            }

            _currentChunk.Add(point);
            _lastX = x;
            _totalCount++;

            if (_currentChunk.Count >= ChunkSize)
            {
                _chunks.Add(_currentChunk);
                _currentChunk = new List<T>(ChunkSize);
            }

            if (AutoTrim && _totalCount > MaxBufferLength)
            {
                TrimOldest();
            }
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public int CopyInRange(float minX, float maxX, Span<T> destination)
    {
        int count = 0;

        _rwLock.EnterReadLock();
        try
        {
            if (maxX - minX > MaxVisibleRange)
            {
                minX = maxX - MaxVisibleRange;
            }

            foreach (List<T> chunk in _chunks)
            {
                if (chunk.Count == 0)
                {
                    continue;
                }

                float chunkMin = _xSelector(chunk[0]);
                float chunkMax = _xSelector(chunk[^1]);

                if (chunkMax < minX || chunkMin > maxX)
                {
                    continue;
                }

                foreach (T v in chunk)
                {
                    float x = _xSelector(v);
                    if (x < minX)
                    {
                        continue;
                    }

                    if (x > maxX)
                    {
                        break;
                    }

                    if (count >= destination.Length)
                    {
                        return count;
                    }

                    destination[count++] = v;
                }
            }

            foreach (T v in _currentChunk)
            {
                float x = _xSelector(v);
                if (x < minX)
                {
                    continue;
                }

                if (x > maxX)
                {
                    break;
                }

                if (count >= destination.Length)
                {
                    return count;
                }

                destination[count++] = v;
            }
        }
        finally
        {
            _rwLock.ExitReadLock();
        }

        return count;
    }

    private void TrimOldest()
    {
        while (_chunks.Count > 0 && _totalCount > MaxBufferLength)
        {
            _totalCount -= _chunks[0].Count;
            _chunks.RemoveAt(0);
        }

        if (_totalCount > MaxBufferLength && _currentChunk.Count > 0)
        {
            int trimCount = _totalCount - MaxBufferLength;
            _currentChunk.RemoveRange(0, Math.Min(trimCount, _currentChunk.Count));
            _totalCount = MaxBufferLength;
        }
    }

    public void Clear()
    {
        _rwLock.EnterWriteLock();
        try
        {
            _chunks.Clear();
            _currentChunk.Clear();
            _totalCount = 0;
            _lastX = 0f;
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        _rwLock.Dispose();
        _chunks.Clear();
        _currentChunk.Clear();
        _totalCount = 0;
    }
}
