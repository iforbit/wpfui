// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Vortice.Direct3D11;

using Wpf.Ui.DirectX.Models;

/// <summary>
/// GraphItem의 Device/Context/Transform 초기화 통합 처리 유틸리티
/// </summary>
public static class GraphItemInitializer
{
    public static void Initialize(IGraphItem item, ID3D11Device device, ID3D11DeviceContext context, float xOffset, float xScale, float yScale)
    {
        if (device == null || context == null)
        {
            System.Diagnostics.Debug.WriteLine("🚫 GraphItemInitializer.Initialize: Invalid device or context");
            return;
        }

        item.SetDevice(device);
        item.SetContext(context);
        item.Initialize(device, context);
        item.Transform(xOffset, xScale, yScale);
    }

    public static void Reinitialize(IGraphItem item, ID3D11Device device, ID3D11DeviceContext context, float xOffset, float xScale, float yScale)
    {
        if (item == null || item.IsDisposed)
        {
            return;
        }

        if (item.Device == device && item.Context == context)
        {
            System.Diagnostics.Debug.WriteLine($"♻️ Skip Reinitialize: same device/context for {item.Name}");
            return;
        }

        item.Dispose();
        item.SetDisposedFlag(false); // ✅ 명시적으로 _disposed 해제
        Initialize(item, device, context, xOffset, xScale, yScale);
    }
}