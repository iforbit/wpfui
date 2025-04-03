// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace Wpf.Ui.Controls;
public class ComboBox : System.Windows.Controls.ComboBox
{
    public static readonly DependencyProperty AllowMultipleSelectionProperty =
        DependencyProperty.Register(
            nameof(AllowMultipleSelection),
            typeof(bool),
            typeof(ComboBox),
            new PropertyMetadata(false));

    public bool AllowMultipleSelection
    {
        get => (bool)GetValue(AllowMultipleSelectionProperty);
        set => SetValue(AllowMultipleSelectionProperty, value);
    }

    // 선택된 항목들을 저장하는 컬렉션
    public ObservableCollection<object> SelectedItems { get; } = new ObservableCollection<object>();

    // 선택 결과 텍스트를 표시하기 위한 사용자 정의 속성
    public static readonly DependencyProperty CustomSelectionBoxItemProperty =
        DependencyProperty.Register(
            nameof(CustomSelectionBoxItem),
            typeof(string),
            typeof(ComboBox),
            new PropertyMetadata(string.Empty));

    public string CustomSelectionBoxItem
    {
        get => (string)GetValue(CustomSelectionBoxItemProperty);
        set => SetValue(CustomSelectionBoxItemProperty, value);
    }

    // CustomSelectionBoxItem에 적용할 DataTemplate (기본 템플릿을 자동 설정)
    public static readonly DependencyProperty CustomSelectionBoxItemTemplateProperty =
        DependencyProperty.Register(
            nameof(CustomSelectionBoxItemTemplate),
            typeof(DataTemplate),
            typeof(ComboBox),
            new PropertyMetadata(null));

    public DataTemplate? CustomSelectionBoxItemTemplate
    {
        get => (DataTemplate?)GetValue(CustomSelectionBoxItemTemplateProperty);
        set => SetValue(CustomSelectionBoxItemTemplateProperty, value);
    }

    private DispatcherTimer? _updateTimer;

    public ComboBox()
    {
        Loaded += ComboBox_Loaded;
        Unloaded += ComboBox_Unloaded;  // Unloaded 이벤트 구독 추가
        SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
    }

    private void ComboBox_Loaded(object sender, RoutedEventArgs e)
    {
        // 다중 선택 모드에서는 기본 SelectedItem을 무시하기 위해 null로 강제 설정합니다.
        if (AllowMultipleSelection)
        {
            SetCurrentValue(SelectedItemProperty, null);
        }

        UpdateSelectedItems();
    }

    private void ItemContainerGenerator_StatusChanged(object? sender, EventArgs e)
    {
        if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
        {
            UpdateSelectedItems();
        }
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        if (AllowMultipleSelection)
        {
            var container = new MultiSelectComboBoxItem();

            // MultiSelectComboBoxItem 전용 스타일을 명시적으로 할당합니다.
            var style = TryFindResource("MultiSelectComboBoxItemStyle") as Style;
            if (style != null)
            {
                container.Style = style;
            }

            return container;
        }

        return base.GetContainerForItemOverride();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        if (!AllowMultipleSelection)
        {
            // 단일 선택 모드일 경우 기본 동작 그대로
            SelectedItems.Clear();
            if (SelectedItem is ComboBoxItem cbi)
            {
                SelectedItems.Add(cbi.Content);
            }
            else
            {
                SelectedItems.Add(SelectedItem);
            }

            base.OnSelectionChanged(e);
            SetCurrentValue(CustomSelectionBoxItemProperty, string.Join(", ", SelectedItems));
        }
        else
        {
            // 다중 선택 모드에서는 SelectedItem을 무효화하여 기본 로직이 기존 선택을 지우지 않도록 합니다.
            SetCurrentValue(SelectedItemProperty, null);
            UpdateSelectedItems();
        }
    }

    protected override void OnDropDownClosed(EventArgs e)
    {
        if (!AllowMultipleSelection)
        {
            base.OnDropDownClosed(e);
        }
        else
        {
            // 다중 선택 모드에서는 드롭다운 닫힘 시 기본 동작이 개입하지 않도록 SelectedItem을 null로 유지합니다.
            SetCurrentValue(SelectedItemProperty, null);
            UpdateSelectedItems();
        }
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (AllowMultipleSelection)
        {
            if (e.OriginalSource is DependencyObject originalSource)
            {
                // var comboBoxItem = ItemsControl.ContainerFromElement(this, originalSource) as ComboBoxItem;
                // if (comboBoxItem != null)
                // {
                //    // 기본 동작 대신 토글 방식으로 선택 상태를 변경합니다.
                //    comboBoxItem.IsSelected = !comboBoxItem.IsSelected;
                //    e.Handled = true;
                //    if (!IsDropDownOpen)
                //    {
                //        SetCurrentValue(IsDropDownOpenProperty, true);
                //    }

                // UpdateSelectedItems();
                //    return;
                // }

                // 여기서 ComboBoxItem 대신 MultiSelectComboBoxItem로 캐스팅
                var multiItem = ItemsControl.ContainerFromElement(this, originalSource) as MultiSelectComboBoxItem;
                if (multiItem != null)
                {
                    // 기존 선택 상태를 토글합니다.
                    multiItem.IsMultiSelected = !multiItem.IsMultiSelected;
                    e.Handled = true;
                    if (!IsDropDownOpen)
                    {
                        SetCurrentValue(IsDropDownOpenProperty, true);
                    }

                    UpdateSelectedItems();
                    return;
                }
            }
        }

        base.OnPreviewMouseLeftButtonDown(e);
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (AllowMultipleSelection)
        {
            // 다중 선택 모드에서는 마우스 버튼을 놓을 때 드롭다운이 닫히지 않도록 기본 동작을 차단합니다.
            e.Handled = true;
            return;
        }

        base.OnPreviewMouseLeftButtonUp(e);
    }

    private void UpdateSelectedItems()
    {
        if (_updateTimer == null)
        {
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _updateTimer.Tick += (s, e) =>
            {
                _updateTimer.Stop();
                var newSelectedItems = new List<object>();
                foreach (var item in Items)
                {
                    if (ItemContainerGenerator.ContainerFromItem(item) is MultiSelectComboBoxItem container)
                    {
                        if (container.IsMultiSelected)
                        {
                            newSelectedItems.Add(item);
                        }
                    }
                }

                SelectedItems.Clear();
                foreach (var selectedItem in newSelectedItems)
                {
                    SelectedItems.Add(selectedItem);
                }

                SetCurrentValue(CustomSelectionBoxItemProperty, string.Join(", ", SelectedItems));
            };
        }

        _updateTimer.Stop();
        _updateTimer.Start();
    }

    private void SelectedItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SetCurrentValue(CustomSelectionBoxItemProperty, string.Join(", ", SelectedItems));
    }

    private void ComboBox_Unloaded(object sender, RoutedEventArgs e)
    {
        // 구독한 이벤트들을 해제합니다.
        Loaded -= ComboBox_Loaded;
        Unloaded -= ComboBox_Unloaded;
        SelectedItems.CollectionChanged -= SelectedItems_CollectionChanged;
        ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
    }
}
