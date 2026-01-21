// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;

namespace Wpf.Ui.Controls;

/// <summary>
/// 타임라인 트랙 인터페이스 (개별 속성)
/// </summary>
public interface ITimelineTrack : INotifyPropertyChanged
{
    /// <summary>
    /// Gets 속성 이름 (예: "X", "Rotation", "Opacity")
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// Gets 속성 표시 이름 (UI용, 없으면 PropertyName 사용)
    /// </summary>
    string? DisplayName { get; }

    /// <summary>
    /// Gets 값 유형 (Numeric, Color, Enum, Boolean)
    /// </summary>
    TrackValueType ValueType { get; }

    /// <summary>
    /// Gets or sets 현재 값 (스크러버 위치 기준)
    /// </summary>
    object? CurrentValue { get; set; }

    /// <summary>
    /// Gets enum 타입인 경우 선택 가능한 값 목록
    /// </summary>
    IEnumerable<object>? EnumValues { get; }

    /// <summary>
    /// Gets numeric 타입인 경우 최소값
    /// </summary>
    double? MinValue { get; }

    /// <summary>
    /// Gets numeric 타입인 경우 최대값
    /// </summary>
    double? MaxValue { get; }

    /// <summary>
    /// Gets 이 트랙의 키프레임들
    /// </summary>
    ObservableCollection<ITimelineKeyframe> Keyframes { get; }

    /// <summary>
    /// Gets 키프레임 간 세그먼트들 (자동 생성)
    /// </summary>
    ObservableCollection<ITimelineSegment> Segments { get; }

    /// <summary>
    /// Gets or sets a value indicating whether 트랙 선택 상태
    /// </summary>
    bool IsSelected { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether 트랙 잠금 상태 (편집 불가)
    /// </summary>
    bool IsLocked { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether 트랙 표시/숨김
    /// </summary>
    bool IsVisible { get; set; }
}
