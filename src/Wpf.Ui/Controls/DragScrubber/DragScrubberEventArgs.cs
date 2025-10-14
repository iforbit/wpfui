// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

/// <summary>
/// DragScrubber 이벤트 인자
/// </summary>
public class DragScrubberEventArgs : EventArgs
{
    /// <summary>
    /// Gets 현재 값
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DragScrubberEventArgs"/> class.
    /// 생성자
    /// </summary>
    /// <param name="value">현재 값</param>
    public DragScrubberEventArgs(double value)
    {
        Value = value;
    }
}