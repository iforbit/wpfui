// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Wpf.Ui.Controls;

/// <summary>
/// 다중 선택 기능을 지원하는 ComboBox 컨트롤.
/// 단일 선택 모드와 다중 선택 모드를 모두 지원하며, 외부 바인딩을 통해 초기 선택 상태를 지정할 수 있습니다.
/// </summary>
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

    public static readonly DependencyProperty FinalSelectedItemsProperty =
        DependencyProperty.Register(
            nameof(FinalSelectedItems),
            typeof(IList),
            typeof(ComboBox),
            new FrameworkPropertyMetadata(null, OnFinalSelectedItemsChanged));

    // 기본값이 null인 경우에도 null이 아닌 새 List<object>를 반환하도록 함
    private static object CoerceFinalSelectedItems(DependencyObject d, object baseValue)
    {
        return baseValue ?? new List<object>();
    }

    public IList? FinalSelectedItems
    {
        get => (IList?)GetValue(FinalSelectedItemsProperty);
        set => SetValue(FinalSelectedItemsProperty, value);
    }

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

    /// <summary>
    /// Gets 내부 선택 상태를 관리하는 ObservableCollection.
    /// </summary>
    public ObservableCollection<object> SelectedItems { get; } = new ObservableCollection<object>();

    // 이벤트 중첩 발생을 방지하기 위한 플래그
    private bool _suppressCollectionChanged = false;

    private bool _updatingFinalCollection = false;

    public ComboBox()
    {
        // 각 인스턴스마다 FinalSelectedItems의 기본값 할당
        SetValue(FinalSelectedItemsProperty, new List<object>());

        Loaded += ComboBox_Loaded;
        Unloaded += ComboBox_Unloaded;
        SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
    }

    private void ComboBox_Loaded(object? sender, RoutedEventArgs e)
    {
        if (AllowMultipleSelection)
        {
            SetCurrentValue(SelectedItemProperty, null);
        }

        if (FinalSelectedItems is { Count: > 0 })
        {
            UpdateSelectedItemsFromFinal(FinalSelectedItems);
        }
        else
        {
            UpdateSelectedItems();
        }
    }

    private static void OnFinalSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ComboBox comboBox)
        {
            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= comboBox.FinalSelectedItems_CollectionChanged;
            }

            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += comboBox.FinalSelectedItems_CollectionChanged;
            }

            if (e.NewValue is IList newList)
            {
                comboBox.UpdateSelectedItemsFromFinal(newList);
            }
        }
    }

    // 외부 컬렉션 변경 이벤트 핸들러: 외부 컬렉션의 변화가 있을 때 내부 선택 상태를 업데이트
    private void FinalSelectedItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_updatingFinalCollection)
        {
            return;
        }

        if (FinalSelectedItems != null)
        {
            UpdateSelectedItemsFromFinal(FinalSelectedItems);
        }
    }

    private void ItemContainerGenerator_StatusChanged(object? sender, EventArgs e)
    {
        // 각 항목의 컨테이너를 가져와 선택 상태를 업데이트
        foreach (var item in Items)
        {
            if (ItemContainerGenerator.ContainerFromItem(item) is MultiSelectComboBoxItem container)
            {
                // 비교 로직 수정: FinalSelectedItems의 값이 int일 경우
                bool isSelected = false;
                foreach (var sel in FinalSelectedItems ?? new List<object>())
                {
                    if (sel is int selectedInt)
                    {
                        // container.Content가 int형이거나, 문자열로 변환하여 비교
                        if (container.Content is int containerInt && selectedInt == containerInt)
                        {
                            isSelected = true;
                            break;
                        }
                        else if (int.TryParse(container.Content?.ToString(), out int containerParsed) &&
                                 selectedInt == containerParsed)
                        {
                            isSelected = true;
                            break;
                        }
                    }
                    else
                    {
                        // 기본 문자열 비교
                        if ((sel?.ToString() ?? string.Empty) == (container.Content?.ToString() ?? string.Empty))
                        {
                            isSelected = true;
                            break;
                        }
                    }
                }

                container.IsMultiSelected = isSelected;
                if (isSelected && !SelectedItems.Contains(container.Content!))
                {
                    SelectedItems.Add(container.Content!); // ✅ null-forgiving
                }
            }
        }

        if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
        {
            UpdateSelectedItems();
        }
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        if (!AllowMultipleSelection)
        {
            SelectedItems.Clear();
            if (SelectedItem is ComboBoxItem comboBoxItem)
            {
                SelectedItems.Add(comboBoxItem.Content);
            }
            else if (SelectedItem != null)
            {
                SelectedItems.Add(SelectedItem);
            }

            base.OnSelectionChanged(e);
            UpdateSelectionBoxItem();
        }
        else
        {
            // 다중 선택 모드에서는 기본 SelectedItem 로직을 무시
            SetCurrentValue(SelectedItemProperty, null);
            UpdateSelectedItems();
            base.OnSelectionChanged(e);
        }
    }

    protected override void OnDropDownOpened(EventArgs e)
    {
        base.OnDropDownOpened(e);

        // 드롭다운이 열릴 때 외부 컬렉션의 최신 값으로 내부 상태를 업데이트
        if (FinalSelectedItems != null)
        {
            UpdateSelectedItemsFromFinal(FinalSelectedItems);
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
            SetCurrentValue(SelectedItemProperty, null);
            UpdateSelectedItems();

            _updatingFinalCollection = true;
            try
            {
                // FinalSelectedItems가 ObservableCollection이면 직접 업데이트
                if (FinalSelectedItems is ObservableCollection<object> observable)
                {
                    observable.Clear();
                    foreach (var item in SelectedItems)
                    {
                        // 만약 item이 숫자여야 한다면, int 변환 시도
                        if (int.TryParse(item?.ToString(), out int num))
                        {
                            observable.Add(num);
                        }
                        else if (item is not null)
                        {
                            observable.Add(item);
                        }
                    }
                }
                else
                {
                    // 그렇지 않다면 새로운 리스트로 설정
                    SetCurrentValue(FinalSelectedItemsProperty, SelectedItems.Cast<object>().ToList());
                }
            }
            finally
            {
                _updatingFinalCollection = false;
            }

            UpdateSelectionBoxItem();
            base.OnDropDownClosed(e);
        }
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (AllowMultipleSelection)
        {
            if (e.OriginalSource is DependencyObject original)
            {
                MultiSelectComboBoxItem? multiItem = FindVisualParent<MultiSelectComboBoxItem>(original);
                if (multiItem != null)
                {
                    multiItem.IsMultiSelected = !multiItem.IsMultiSelected;
                    e.Handled = true;

                    if (!IsDropDownOpen)
                    {
                        SetCurrentValue(IsDropDownOpenProperty, true);
                    }

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
            // 다중 선택 모드에서는 드롭다운이 닫히지 않도록 기본 동작 차단
            e.Handled = true;
            return;
        }

        base.OnPreviewMouseLeftButtonUp(e);
    }

    private void SelectedItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_suppressCollectionChanged)
        {
            return;
        }

        UpdateSelectionBoxItem();
    }

    private void ComboBox_Unloaded(object sender, RoutedEventArgs e)
    {
        // 구독한 모든 이벤트 해제 (메모리 누수 방지)
        Loaded -= ComboBox_Loaded;
        Unloaded -= ComboBox_Unloaded;
        SelectedItems.CollectionChanged -= SelectedItems_CollectionChanged;
        ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
    }

    // 선택 텍스트를 업데이트하는 헬퍼 메서드
    private void UpdateSelectionBoxItem()
    {
        SetCurrentValue(CustomSelectionBoxItemProperty, string.Join(", ", SelectedItems));
    }

    // 내부 SelectedItems를 현재 컨테이너 상태에 맞게 업데이트
    private void UpdateSelectedItems()
    {
        var oldSelected = SelectedItems.ToList();
        var newSelected = Items.Cast<object>()
             .Select(item =>
             {
                 MultiSelectComboBoxItem? container = item as MultiSelectComboBoxItem
                     ?? ItemContainerGenerator.ContainerFromItem(item) as MultiSelectComboBoxItem;
                 return container;
             })
             .Where(container => container is not null && container.IsMultiSelected && container.Content is not null)
             .Select(container => container!.Content!)
             .ToList();

        if (newSelected.Count == oldSelected.Count && !newSelected.Except(oldSelected).Any())
        {
            return;
        }

        _suppressCollectionChanged = true;
        SelectedItems.Clear();
        foreach (var item in newSelected)
        {
            SelectedItems.Add(item);
        }

        _suppressCollectionChanged = false;

        UpdateSelectionBoxItem();

        // SelectionChanged 이벤트 발생 (이전 값과 새 값을 전달)
        var args = new SelectionChangedEventArgs(
            SelectionChangedEvent,
            oldSelected,
            SelectedItems.ToList()
        );
        RaiseEvent(args);
    }

    // 외부에서 전달받은 FinalSelectedItems 값을 내부 SelectedItems에 반영
    private void UpdateSelectedItemsFromFinal(IList externalSelected)
    {
        _suppressCollectionChanged = true;
        SelectedItems.Clear();
        foreach (var item in externalSelected)
        {
            SelectedItems.Add(item);
        }

        _suppressCollectionChanged = false;

        UpdateSelectionBoxItem();

        // 여기서 아이템 컨테이너의 IsMultiSelected 상태를 강제로 갱신
        foreach (var item in Items)
        {
            if (ItemContainerGenerator.ContainerFromItem(item) is MultiSelectComboBoxItem container)
            {
                bool isSelected = false;
                foreach (var sel in externalSelected)
                {
                    if (sel is int selectedInt)
                    {
                        if (container.Content is int containerInt && selectedInt == containerInt)
                        {
                            isSelected = true;
                            break;
                        }
                        else if (int.TryParse(container.Content?.ToString(), out int containerParsed) &&
                                 selectedInt == containerParsed)
                        {
                            isSelected = true;
                            break;
                        }
                    }
                    else
                    {
                        if ((sel?.ToString() ?? string.Empty) == (container.Content?.ToString() ?? string.Empty))
                        {
                            isSelected = true;
                            break;
                        }
                    }
                }

                container.IsMultiSelected = isSelected;
            }
        }

        var args = new SelectionChangedEventArgs(
            SelectionChangedEvent,
            new List<object>(),
            SelectedItems.ToList()
        );
        RaiseEvent(args);
    }

    // Visual Tree에서 부모 컨테이너를 찾는 재귀 헬퍼 메서드
    private T? FindVisualParent<T>(DependencyObject child)
        where T : DependencyObject
    {
        DependencyObject parent = VisualTreeHelper.GetParent(child);
        if (parent == null)
        {
            return null;
        }

        if (parent is T typedParent)
        {
            return typedParent;
        }

        return FindVisualParent<T>(parent);
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        if (AllowMultipleSelection)
        {
            var container = new MultiSelectComboBoxItem();
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
}
