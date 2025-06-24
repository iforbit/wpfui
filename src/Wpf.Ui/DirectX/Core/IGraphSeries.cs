// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.DirectX.Core;

public interface IGraphSeries : IDisposable
{
    string Name { get; set; }

    bool IsVisible { get; set; }

    bool IsReady { get; }

    bool IsDisposed { get; }

    void Initialize();

    // Transform 계산용 (optional, null 시 무시 가능)
    float MinX { get; }

    float MaxX { get; }

    float LastX { get; }
}

