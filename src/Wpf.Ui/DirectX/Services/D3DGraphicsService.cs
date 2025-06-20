// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using SharpGen.Runtime;

using System.Diagnostics;
using System.IO;

using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Wpf.Ui.DirectX.Services;

/// <summary>
/// Direct3D11 디바이스 및 공용 자원 제공을 위한 기본 구현.
/// </summary>
public sealed class D3DGraphicsService : ID3DGraphicsService
{
    public ID3D11Device Device { get; }

    public ID3D11DeviceContext Context { get; }

    public IDXGIFactory Factory => _factory;

    private readonly IDXGIFactory _factory;

    public D3DGraphicsService()
    {
        // 기본 플래그 설정: BGRA 지원
        DeviceCreationFlags flags = DeviceCreationFlags.BgraSupport;
#if DEBUG
        flags |= DeviceCreationFlags.Debug;
#endif

        FeatureLevel[] featureLevels = new[]
{
        FeatureLevel.Level_11_1,
        FeatureLevel.Level_11_0,
        FeatureLevel.Level_10_1,
        FeatureLevel.Level_10_0
};

        // 1. 팩토리 생성
        IDXGIFactory6? factory6 = null;
        IDXGIAdapter1? adapter = null;

        try
        {
            DXGI.CreateDXGIFactory2(debug: false, out factory6).CheckError();
            factory6!.EnumAdapterByGpuPreference(0, GpuPreference.HighPerformance, out adapter).CheckError();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"⚠️ DXGI 팩토리 생성 또는 어댑터 선택 실패: {ex}");
        }

        // 디바이스 + 컨텍스트 생성
        Result result = D3D11.D3D11CreateDevice(
          adapter: adapter,
          driverType: DriverType.Unknown,
          flags: flags,
          featureLevels: featureLevels,
          out ID3D11Device device,
          out ID3D11DeviceContext context
      );

        try
        {
            result.CheckError();
        }
        catch (SharpGenException ex) when (ex.ResultCode.Code == unchecked((int)0x887A0005))// DXGI_ERROR_DEVICE_REMOVED
        {
            Debug.WriteLine("💥 GPU 장치가 제거됨. 재초기화 필요.");
            throw;

            // TODO: 재생성 로직 또는 사용자 알림
        }
        catch (SharpGenException ex)
        {
            Debug.WriteLine($"💥 D3D11 디바이스 생성 실패: {ex.Message}");
            throw;
        }

        Device = device;
        Context = context;

        // 3. 팩토리는 IDXGIFactory1로 캐스팅 또는 새로 생성
        _factory = factory6?.QueryInterfaceOrNull<IDXGIFactory1>()
                    ?? DXGI.CreateDXGIFactory1<IDXGIFactory1>();
    }

    // 스왑체인 버퍼에서 RTV를 생성할 때 사용
    public ID3D11RenderTargetView CreateRenderTargetView(IDXGISwapChain swapChain)
    {
        using ID3D11Texture2D backBuffer = swapChain.GetBuffer<ID3D11Texture2D>(0);
        return Device.CreateRenderTargetView(backBuffer);
    }

    public byte[] LoadShaderBytes(string shaderFileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", shaderFileName);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Shader file not found: {path}");
        }

        return File.ReadAllBytes(path);
    }

    public void Dispose()
    {
        Context?.Dispose();
        Device?.Dispose();
        _factory?.Dispose();
    }
}