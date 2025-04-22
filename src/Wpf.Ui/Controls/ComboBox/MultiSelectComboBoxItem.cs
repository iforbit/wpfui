// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;

namespace Wpf.Ui.Controls;

// 커스텀 ComboBoxItem: 다중 선택 모드일 때 기본 선택 해제 동작을 막습니다.
public class MultiSelectComboBoxItem : ComboBoxItem
{
    public static readonly DependencyProperty IsMultiSelectedProperty =
          DependencyProperty.Register(
              nameof(IsMultiSelected),
              typeof(bool),
              typeof(MultiSelectComboBoxItem),
              new PropertyMetadata(false));

    public bool IsMultiSelected
    {
        get => (bool)GetValue(IsMultiSelectedProperty);
        set => SetValue(IsMultiSelectedProperty, value);
    }

    protected override void OnSelected(RoutedEventArgs e)
    {
        if (TemplatedParent is ComboBox combo && combo.AllowMultipleSelection)
        {
            // 다중 선택 모드에서는 기본 OnSelected 호출하지 않음
            return;
        }

        base.OnSelected(e);
    }

    protected override void OnUnselected(RoutedEventArgs e)
    {
        if (TemplatedParent is ComboBox combo && combo.AllowMultipleSelection)
        {
            // 다중 선택 모드에서는 기본 OnUnselected 호출하지 않음
            return;
        }

        base.OnUnselected(e);
    }

    public override string ToString()
    {
        return Content?.ToString() ?? base.ToString();
    }
}
