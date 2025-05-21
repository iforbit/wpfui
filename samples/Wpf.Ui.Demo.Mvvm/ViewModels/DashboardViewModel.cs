// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using CommunityToolkit.Mvvm.Messaging;

using System.Collections.ObjectModel;
using System.Windows.Controls;

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
    public ObservableCollection<TabItem> MyTabs { get; } = new()
    {
        new TabItem { Header = "로그", Content = new System.Windows.Controls.TextBox { Text = "로그 내용" } },
        new TabItem { Header = "상태", Content = new System.Windows.Controls.TextBlock { Text = "상태 정보 표시" } }
    };
    [RelayCommand]
    private void messageTest()
    {
        _ = WeakReferenceMessenger.Default.Send(new ChildAddMessage("Child"));
    }

    private object? _currentTab;
    public object? CurrentTab
    {
        get => _currentTab;
        set => SetProperty(ref _currentTab, value);
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
        CurrentTab = MyTabs.FirstOrDefault();
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

    private Dock _selectedTabPlacement = Dock.Right;
    public Dock SelectedTabPlacement
    {
        get => _selectedTabPlacement;
        set => SetProperty(ref _selectedTabPlacement, value);
    }
}

