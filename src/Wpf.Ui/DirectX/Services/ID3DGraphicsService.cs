// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Wpf.Ui.DirectX.Services;

/// <summary>
/// 공통 D3D11 디바이스 및 셰이더/렌더링 리소스를 제공하는 그래픽 서비스.
/// 모든 D3D 기반 컨트롤/렌더러는 이 서비스를 통해 자원을 공유합니다.
/// </summary>
public interface ID3DGraphicsService : IDisposable
{
    /// <summary>
    /// Gets 생성된 D3D11 디바이스.
    /// </summary>
    ID3D11Device Device { get; }

    /// <summary>
    /// Gets 디바이스에서 생성된 디바이스 컨텍스트.
    /// </summary>
    ID3D11DeviceContext Context { get; }

    IDXGIFactory Factory { get; }

    /// <summary>
    /// 스왑체인에서 렌더 타겟 뷰를 생성합니다.
    /// </summary>
    /// <param name="swapChain">대상 스왑체인</param>
    /// <returns>렌더 타겟 뷰</returns>
    ID3D11RenderTargetView CreateRenderTargetView(IDXGISwapChain swapChain);

    /// <summary>
    /// 셰이더 바이너리 파일(.cso)을 Assets 폴더에서 로드합니다.
    /// </summary>
    /// <param name="shaderFileName">예: "graph_vs.cso"</param>
    /// <returns>바이트 배열</returns>
    byte[] LoadShaderBytes(string shaderFileName);
}