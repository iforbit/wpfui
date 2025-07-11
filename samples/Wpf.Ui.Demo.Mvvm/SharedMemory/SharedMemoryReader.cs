// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.IO.MemoryMappedFiles;

namespace Wpf.Ui.Demo.Mvvm.SharedMemory;

public readonly struct SignalPoint
{
    public double X { get; init; }
    public float Y { get; init; }

    public SignalPoint(double x, float y)
    {
        X = x;
        Y = y;
    }
}

public sealed class SharedMemoryReader : IDisposable
{
    private readonly MemoryMappedFile _mmf;
    private readonly MemoryMappedViewAccessor _accessor;

    public SharedMemoryReader(string mapName = "Local\\GraphSignalBuffer")
    {
        _mmf = MemoryMappedFile.CreateOrOpen(mapName, 65536); // 충분한 크기 확보
        _accessor = _mmf.CreateViewAccessor();
    }

    public (int ChannelCount, int PointsPerChannel) ReadHeader()
    {
        int channelCount = _accessor.ReadInt32(0);
        int pointsPerChannel = _accessor.ReadInt32(4);
        return (channelCount, pointsPerChannel);
    }

    public void ReadChannel(int channelIndex, int pointCount, Span<(double X, float Y)> buffer)
    {
        long baseOffset = 8 + channelIndex * pointCount * 12;

        for (int i = 0; i < pointCount; i++)
        {
            long offset = baseOffset + i * 12;
            double x = _accessor.ReadDouble(offset);
            float y = _accessor.ReadSingle(offset + 8);
            buffer[i] = (x, y);
        }
    }

    public void Dispose()
    {
        _accessor?.Dispose();
        _mmf?.Dispose();
    }
}