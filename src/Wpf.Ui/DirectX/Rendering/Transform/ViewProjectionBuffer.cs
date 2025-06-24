// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Numerics;
using System.Runtime.InteropServices;

namespace Wpf.Ui.DirectX.Rendering.Transform;

/// <summary>
/// GPU에 업로드되는 ViewProjection 변환 상수 버퍼 구조체
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ViewProjectionBuffer
{
    public Vector4 Transform; // (ScaleX, ScaleY, OffsetX, OffsetY)

    public ViewProjectionBuffer(float xScale, float yScale, float xOffset, float yOffset)
    {
        Transform = new Vector4(xScale, yScale, xOffset, yOffset);
    }

    /// <summary>
    /// GPU 업로드용 버퍼 크기 (바이트 단위, 16바이트 정렬 보장)
    /// </summary>
    public static readonly int SizeInBytes = (int)System.Runtime.InteropServices.Marshal.SizeOf<ViewProjectionBuffer>();
}
