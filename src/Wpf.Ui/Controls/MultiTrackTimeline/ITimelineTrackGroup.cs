// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;

namespace Wpf.Ui.Controls;

/// <summary>
/// 타임라인 트랙 그룹 인터페이스 (Transform, Appearance 등)
/// </summary>
public interface ITimelineTrackGroup : INotifyPropertyChanged
{
    /// <summary>
    /// Gets 그룹 이름 (예: "Transform", "Appearance")
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets or sets a value indicating whether 그룹 펼침/접힘 상태
    /// </summary>
    bool IsExpanded { get; set; }

    /// <summary>
    /// Gets 이 그룹에 속한 트랙들
    /// </summary>
    ObservableCollection<ITimelineTrack> Tracks { get; }
}
