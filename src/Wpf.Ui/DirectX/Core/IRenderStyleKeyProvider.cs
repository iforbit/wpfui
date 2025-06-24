// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Vortice.Mathematics;

using Wpf.Ui.DirectX.Models.VertexTypes;

namespace Wpf.Ui.DirectX.Core;

public interface IRenderStyleKeyProvider
{
    DrawKey GetDrawKey();
}

public readonly record struct DrawKey(
    PixelShaderType ShaderType,
    Color4 Color,
    Vortice.Direct3D.PrimitiveTopology Topology,
    string LayoutKey // 예: "POSITION", "POSITION+COLOR"
);