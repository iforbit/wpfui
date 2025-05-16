// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Xaml.Behaviors;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using System.Windows.Threading;

using Wpf.Ui.Controls;
using Wpf.Ui.Demo.Mvvm.Message;

namespace Wpf.Ui.Demo.Mvvm.ViewModels;

public partial class DashboardViewModel : ViewModel
{
    private readonly bool _isInitialized = false;

    [ObservableProperty]
    private object? _counter = 0.00001458;

    [ObservableProperty]
    private string? _message = string.Empty;

    [ObservableProperty]
    private List<int> _comboIdx = new List<int>
    {
        1, 4
    };

    public ObservableCollection<int> tESTlIST { get; set; } = new ObservableCollection<int>() { 1, 3 };

    [RelayCommand]
    private void messageTest()
    {
        _ = WeakReferenceMessenger.Default.Send(new ChildAddMessage("Child"));
    }
    public override void OnNavigatedTo()
    {
        if (!_isInitialized)
        {
            InitializeViewModel();
        }
    }
    private void InitializeViewModel()
    {
    }

    [RelayCommand]
    private void ComboTest(object param)
    {
        if (param == null)
            return;

        Console.Write(param.ToString());

        if (!int.TryParse(param.ToString(), out _))
            return;

    }
}

public class SelectedItemsChangedBehavior : Behavior<ComboBox>
{
    public ICommand SelectedItemsChangedCommand
    {
        get { return (ICommand)GetValue(SelectedItemsChangedCommandProperty); }
        set { SetValue(SelectedItemsChangedCommandProperty, value); }
    }

    public static readonly DependencyProperty SelectedItemsChangedCommandProperty =
        DependencyProperty.Register(nameof(SelectedItemsChangedCommand), typeof(ICommand), typeof(SelectedItemsChangedBehavior), new PropertyMetadata(null));

    private DispatcherTimer? _debounceTimer;

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject?.SelectedItems is INotifyCollectionChanged notifyCollection)
        {
            notifyCollection.CollectionChanged += OnSelectedItemsChanged;
        }

        _debounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _debounceTimer.Tick += DebounceTimer_Tick;
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject?.SelectedItems is INotifyCollectionChanged notifyCollection)
        {
            notifyCollection.CollectionChanged -= OnSelectedItemsChanged;
        }

        _debounceTimer.Tick -= DebounceTimer_Tick;
        _debounceTimer.Stop();
        base.OnDetaching();
    }

    private void OnSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // 디바운스 타이머를 재시작하여 여러 이벤트를 하나로 모읍니다.
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    private void DebounceTimer_Tick(object sender, EventArgs e)
    {
        _debounceTimer.Stop();
        if (SelectedItemsChangedCommand != null && SelectedItemsChangedCommand.CanExecute(AssociatedObject.SelectedItems))
        {
            SelectedItemsChangedCommand.Execute(AssociatedObject.SelectedItems);
        }
    }
}