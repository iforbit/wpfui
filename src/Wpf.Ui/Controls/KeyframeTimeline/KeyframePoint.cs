// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Wpf.Ui.Controls;

/// <summary>
/// 키프레임 포인트 - 동일 시간에 여러 Snapshot을 담는 Container
/// X축의 특정 시간에 배치되며, 여러 종류의 Snapshot을 포함할 수 있음
/// </summary>
public class KeyframePoint : INotifyPropertyChanged
{
    private double _time;
    private bool _isSelected;
    private double _canvasX;
    private object? _tag;

    /// <summary>
    /// Gets or sets 키프레임 시간 (초)
    /// </summary>
    public double Time
    {
        get => _time;
        set => SetProperty(ref _time, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether 선택 여부
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>
    /// Gets or sets canvas X 좌표 (내부 계산용)
    /// </summary>
    public double CanvasX
    {
        get => _canvasX;
        set => SetProperty(ref _canvasX, value);
    }

    /// <summary>
    /// Gets or sets 사용자 데이터 (Keyframe DTO 참조 등)
    /// </summary>
    public object? Tag
    {
        get => _tag;
        set => SetProperty(ref _tag, value);
    }

    /// <summary>
    /// Gets 이 포인트에 포함된 Snapshot 이름들 (Transform, Visibility, Color)
    /// </summary>
    public ObservableCollection<string> SnapshotNames { get; } = new();

    /// <summary>
    /// Gets 툴팁 텍스트
    /// </summary>
    public string TooltipText
    {
        get
        {
            var snapshots = SnapshotNames.Count > 0 ? string.Join(", ", SnapshotNames) : "Empty";
            return $"Time: {Time:F2}s\nSnapshots: {snapshots}";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
