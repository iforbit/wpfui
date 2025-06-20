// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Numerics;
using System.Runtime.InteropServices;

using Vortice.Mathematics;

namespace Wpf.Ui.DirectX.Models.VertexTypes;

[StructLayout(LayoutKind.Sequential)]
public struct VertexPositionColor
{
    public System.Numerics.Vector3 Position;
    public Vortice.Mathematics.Color4 Color;

    public VertexPositionColor(float x, float y, float z, Color4 color)
    {
        Position = new Vector3(x, y, 0.0f);
        Color = color;
    }

    public VertexPositionColor(float x, float y, Color4 color)
        : this(x, y, 0.0f, color)
    {
    }
}