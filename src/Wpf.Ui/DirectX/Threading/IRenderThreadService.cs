// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.DirectX.Rendering;

namespace Wpf.Ui.DirectX.Threading;

/// <summary>
/// 전용 RenderThread를 통한 고성능 DirectX 렌더링 서비스 인터페이스
/// </summary>
public interface IRenderThreadService : IDisposable
{
    /// <summary>
    /// 렌더링 가능한 항목 등록
    /// </summary>
    void Register(IRenderable renderable);

    /// <summary>
    /// 렌더링 항목 등록 해제
    /// </summary>
    void Unregister(IRenderable renderable);

    /// <summary>
    /// 렌더링을 즉시 트리거 (전 프레임 요청)
    /// </summary>
    void RequestRender();

    /// <summary>
    /// 렌더링 스레드 시작
    /// </summary>
    void Start();

    /// <summary>
    /// 렌더링 스레드 중지
    /// </summary>
    void Stop();

    /// <summary>
    /// Gets a value indicating whether 현재 활성 상태인지 여부
    /// </summary>
    bool IsRunning { get; }
}
