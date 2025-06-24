// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.
using Wpf.Ui.DirectX.Core;

namespace Wpf.Ui.DirectX.Services;

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
    /// 렌더링 스레드 시작
    /// </summary>
    void Start();

    /// <summary>
    /// 렌더링 스레드 중지
    /// </summary>
    void Stop();

    /// <summary>
    /// Gets a value indicating whether 현재 렌더 스레드가 실행 중인지 여부
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets or sets 타겟 FPS 설정 또는 조회
    /// </summary>
    int TargetFps { get; set; }
}