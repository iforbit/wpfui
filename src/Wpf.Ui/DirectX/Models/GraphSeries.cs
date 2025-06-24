// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Vortice.Mathematics;

using Wpf.Ui.DirectX.Core;

namespace Wpf.Ui.DirectX.Models;

#pragma warning disable SA1401
public abstract class GraphSeries<T> : IGraphSeries
    where T : unmanaged
{
    private bool _disposed = false;

    public bool IsDisposed => _disposed;

    public string Name { get; set; } = string.Empty;

    public bool IsVisible { get; set; } = true;

    public Color4 GraphColor { get; set; } = new(1f, 1f, 1f, 1f);

    protected bool _initialized = false;

    public bool IsReady => !_disposed && _initialized;

    protected float _minX = float.MaxValue;
    protected float _maxX = float.MinValue;
    protected float _lastX = 0f;

    public virtual float MinX => _minX;

    public virtual float MaxX => _maxX;

    public virtual float LastX => _lastX;

    public void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        OnInitialize();
    }

    protected abstract void OnInitialize();

    public abstract void Append(ReadOnlySpan<T> data);

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _disposed = true;
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
#pragma warning restore SA1401