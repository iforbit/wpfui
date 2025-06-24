// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.DirectX.Core;

/// <summary>
/// 렌더링 가능한 객체 인터페이스 (예: LineGraphControl 등)
/// </summary>
public interface IRenderable
{
    bool IsReady { get; }

    void RenderFrame(float time);
}
