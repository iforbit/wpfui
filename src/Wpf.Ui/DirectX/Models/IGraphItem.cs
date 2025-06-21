// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Vortice.Direct3D11;
using Vortice.Mathematics;

using Wpf.Ui.DirectX.Models.VertexTypes;

namespace Wpf.Ui.DirectX.Models;

public interface IGraphItem : IDisposable
{
    bool IsVisible { get; set; }

    bool IsReadyToRender { get; }

    bool IsDisposed { get; }

    string Name { get; set; }

    PixelShaderType ShaderType { get; set; }

    Color4 GraphColor { get; set; }

    ID3D11Device? Device { get; }

    ID3D11DeviceContext? Context { get; }

    void SetDevice(ID3D11Device device);

    void SetContext(ID3D11DeviceContext context);

    void SetDisposedFlag(bool value);

    void Initialize(ID3D11Device device, ID3D11DeviceContext context);

    void Transform(float xOffset, float xScale, float yScale, bool force = false);

    bool TryGetTransform(out float xOffset, out float xScale);

    void Render(ID3D11DeviceContext context);
}