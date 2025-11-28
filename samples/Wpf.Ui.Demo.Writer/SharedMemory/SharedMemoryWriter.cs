// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.IO.MemoryMappedFiles;

namespace Wpf.Ui.Demo.Writer.SharedMemory;

public sealed class SharedMemoryWriter : IDisposable
{
    private readonly MemoryMappedFile _mmf;
    private readonly MemoryMappedViewAccessor _accessor;
    private readonly int _maxChannels;
    private readonly int _maxPointsPerChannel;
    private int _updateCounter = 0;

    public SharedMemoryWriter(
        string mapName = "Local\\GraphSignalBuffer",
        int maxChannels = 10,
        int maxPointsPerChannel = 50000
    )
    {
        _maxChannels = maxChannels;
        _maxPointsPerChannel = maxPointsPerChannel;

        // Calculate required buffer size
        // Header: 12 bytes (channelCount + pointsPerChannel + updateCounter)
        // Data: maxChannels * maxPointsPerChannel * 12 bytes (double X + float Y)
        long bufferSize = 12 + (maxChannels * maxPointsPerChannel * 12);

        _mmf = MemoryMappedFile.CreateOrOpen(mapName, bufferSize);
        _accessor = _mmf.CreateViewAccessor();

        // Initialize header
        WriteHeader(0, 0);
    }

    public void WriteHeader(int channelCount, int pointsPerChannel)
    {
        if (channelCount > _maxChannels)
            throw new ArgumentException($"Channel count exceeds maximum: {_maxChannels}");

        if (pointsPerChannel > _maxPointsPerChannel)
            throw new ArgumentException($"Points per channel exceeds maximum: {_maxPointsPerChannel}");

        _accessor.Write(0, channelCount);
        _accessor.Write(4, pointsPerChannel);
        _accessor.Write(8, ++_updateCounter); // Increment and write update counter
    }

    public void WriteChannel(int channelIndex, ReadOnlySpan<(double X, float Y)> points)
    {
        if (channelIndex >= _maxChannels)
            throw new ArgumentException($"Channel index exceeds maximum: {_maxChannels}");

        if (points.Length > _maxPointsPerChannel)
            throw new ArgumentException($"Point count exceeds maximum: {_maxPointsPerChannel}");

        long baseOffset = 12 + channelIndex * _maxPointsPerChannel * 12;

        for (int i = 0; i < points.Length; i++)
        {
            long offset = baseOffset + i * 12;
            _accessor.Write(offset, points[i].X);
            _accessor.Write(offset + 8, points[i].Y);
        }
    }

    public void WriteChannels(ReadOnlySpan<ReadOnlyMemory<(double X, float Y)>> channels)
    {
        if (channels.Length > _maxChannels)
            throw new ArgumentException($"Channel count exceeds maximum: {_maxChannels}");

        int maxPoints = 0;
        for (int i = 0; i < channels.Length; i++)
        {
            maxPoints = Math.Max(maxPoints, channels[i].Length);
        }

        // Update header
        WriteHeader(channels.Length, maxPoints);

        // Write each channel
        for (int i = 0; i < channels.Length; i++)
        {
            WriteChannel(i, channels[i].Span);
        }
    }

    public void Clear()
    {
        WriteHeader(0, 0);
    }

    public void Dispose()
    {
        _accessor?.Dispose();
        _mmf?.Dispose();
    }
}
