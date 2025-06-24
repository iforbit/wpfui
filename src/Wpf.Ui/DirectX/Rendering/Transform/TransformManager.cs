// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Diagnostics;
using System.Runtime.InteropServices;

using Vortice.Direct3D11;

namespace Wpf.Ui.DirectX.Rendering.Transform;

/// <summary>
/// GPU 상수 버퍼로 ViewProjection 정보를 업로드하는 매니저
/// </summary>
public sealed class TransformManager : IDisposable
{
    private readonly ID3D11Device _device;
    private ID3D11Buffer? _viewProjectionBuffer;
    private bool _disposed;
    private readonly object _lock = new();

    public ID3D11Buffer? Buffer => _viewProjectionBuffer;

    public float XScale { get; private set; } = 1.0f;

    public float YScale { get; private set; } = 1.0f;

    public float XOffset { get; private set; } = 0.0f;

    public float YOffset { get; private set; } = 0.0f;

    public bool IsUserControlled { get; set; } = false;

    private bool _isDirty = false;
    private ViewProjectionBuffer _pendingData;

    public TransformManager(ID3D11Device device)
    {
        _device = device;
        CreateBuffer();
    }

    private void CreateBuffer()
    {
        _viewProjectionBuffer = _device.CreateBuffer(new BufferDescription(
            byteWidth: (uint)ViewProjectionBuffer.SizeInBytes,
            BindFlags.ConstantBuffer,
            ResourceUsage.Dynamic,
            CpuAccessFlags.Write
        ));
    }

    public void SetTransform(float xOffset, float xScale, float yScale, float yOffset = 0f)
    {
        if (_disposed)
        {
            return;
        }

        lock (_lock)
        {
            if (xScale == XScale && yScale == YScale && xOffset == XOffset && yOffset == YOffset)
            {
                return;
            }

            XScale = xScale;
            YScale = yScale;
            XOffset = xOffset;
            YOffset = yOffset;

            _pendingData = new ViewProjectionBuffer(xScale, yScale, xOffset, yOffset);
            _isDirty = true;
        }
    }

    public void FollowLatestX(ID3D11DeviceContext context, float latestX, float visibleRange = 30f)
    {
        if (IsUserControlled || _disposed || _viewProjectionBuffer == null || context.NativePointer == IntPtr.Zero)
        {
            return;
        }

        float newOffset = latestX - visibleRange;
        if (Math.Abs(newOffset - XOffset) < 0.0001f)
        {
            return;
        }

        XScale = 0.1f; // 스케일은 고정

        // Debug.WriteLine($"🧭 SyncOffset: {XOffset} → {newOffset}, LastX={latestX}");
        SetTransform(newOffset, XScale, YScale, YOffset);
    }

    // 🧠 Render 루프에서 호출 (1프레임 1회)
    public void ApplyToContext(ID3D11DeviceContext context, int slot = 0)
    {
        if (_disposed || _viewProjectionBuffer == null || context.NativePointer == IntPtr.Zero || !_isDirty)
        {
            return;
        }

        lock (_lock)
        {
            try
            {
                MappedSubresource mapped = context.Map(_viewProjectionBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
                Marshal.StructureToPtr(_pendingData, mapped.DataPointer, false);
                context.Unmap(_viewProjectionBuffer, 0);
                _isDirty = false;

                // Debug.WriteLine("📦 TransformBuffer: " + _pendingData.Transform);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("🛑 ApplyToContext failed: " + ex.Message);
            }
        }

        context.VSSetConstantBuffer((uint)slot, _viewProjectionBuffer);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _viewProjectionBuffer?.Dispose();
        _disposed = true;
    }
}

