// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Diagnostics;

using Vortice.Mathematics;

using Wpf.Ui.DirectX.Models.VertexTypes;

namespace Wpf.Ui.DirectX.Models;

public sealed class ChunkedVertexBuffer : IDisposable
{
    private const int ChunkSize = 16_384;

    private readonly object _lock = new();

    private readonly ReaderWriterLockSlim _rwLock = new();

    private readonly List<List<VertexPositionColor>> _chunks = new();

    private List<VertexPositionColor> _currentChunk = new();

    private float _lastX = 0f;

    public float LastX => _lastX;

    public float XStep { get; set; } = 0.05f;

    public int MaxBufferLength { get; set; } = 100_000;  // 최대 보관 길이

    public bool AutoTrim { get; set; } = true;

    public float MaxVisibleRange { get; set; } = 30f;    // 최대 시야 범위 (초)

    private int _totalCount = 0;

    public int TotalCount => _totalCount;

    public ChunkedVertexBuffer()
    {
    }

    public void AppendPoint(float x, float y, Color4? color = null)
    {
        _rwLock.EnterWriteLock();
        try
        {
            if (x < _lastX)
            {
                Debug.WriteLine($"⚠️ Non-monotonic X detected: {x} < {_lastX}");
                return; // 또는 return false;
            }

            Color4 finalColor = color ?? new Color4(1f, 1f, 1f, 1f);

            _currentChunk.Add(new VertexPositionColor(x, y, 0f, finalColor));
            _lastX = x;
            _totalCount++;

            if (_currentChunk.Count >= ChunkSize)
            {
                _chunks.Add(_currentChunk);
                _currentChunk = new List<VertexPositionColor>(ChunkSize); // 최적화: capacity 명시
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

    public int CopyVerticesInRange(float minX, float maxX, Span<VertexPositionColor> destination)
    {
        int count = 0;

        _rwLock.EnterReadLock();
        try
        {
            if (maxX - minX > MaxVisibleRange)
            {
                minX = maxX - MaxVisibleRange;
            }

            foreach (List<VertexPositionColor> chunk in _chunks)
            {
                if (chunk.Count == 0)
                {
                    continue;
                }

                float chunkMin = chunk[0].Position.X;
                float chunkMax = chunk[^1].Position.X;

                if (chunkMax < minX || chunkMin > maxX)
                {
                    continue;
                }

                foreach (VertexPositionColor v in chunk)
                {
                    if (v.Position.X < minX)
                    {
                        continue;
                    }

                    if (v.Position.X > maxX)
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

            foreach (VertexPositionColor v in _currentChunk)
            {
                if (v.Position.X < minX)
                {
                    continue;
                }

                if (v.Position.X > maxX)
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

    public void Dispose()
    {
        lock (_lock)
        {
            _rwLock.Dispose();
            _chunks.Clear();
            _currentChunk.Clear();
            _totalCount = 0;
            _lastX = 0f;
        }
    }
}