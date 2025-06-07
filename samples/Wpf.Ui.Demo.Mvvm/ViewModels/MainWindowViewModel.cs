// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.
using CommunityToolkit.Mvvm.Messaging;

using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;

using Wpf.Ui.Controls;
using Wpf.Ui.Demo.Mvvm.Message;
using Wpf.Ui.Demo.Mvvm.Views.Pages;

namespace Wpf.Ui.Demo.Mvvm.ViewModels;

public partial class MainWindowViewModel : ViewModel
{
    readonly INavigationService _navigationService;

    private bool _isInitialized = false;

    [ObservableProperty]
    private string _applicationTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<NavigationViewItem> _navigationItems = [];

    [ObservableProperty]
    private ObservableCollection<object> _tabItems = [];

    [ObservableProperty]
    private ObservableCollection<object> _navigationFooter = [];

    [ObservableProperty]
    private ObservableCollection<MenuItem> _trayMenuItems = [];

    public ObservableCollection<ConnectedIedViewModel> IedViewModels { get; } = new();

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0060:Remove unused parameter",
        Justification = "Demo"
    )]
    public MainWindowViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService ??
            throw new ArgumentNullException(nameof(navigationService));

        if (!_isInitialized)
        {
            InitializeViewModel();
        }

        // IED 선택 메시지 받기
        WeakReferenceMessenger.Default.Register<ChildAddMessage>(this, (r, m) =>
        {
            var parts = m.child.Split('@');
            var ipPort = parts[0].Split(':');
            var host = ipPort[0];
            var port = int.Parse(ipPort[1]);
            var name = parts.Length > 1 ? parts[1] : "Unknown";

            var vm = new ConnectedIedViewModel(name, host, port);

            // 중복 방지
            if (!IedViewModels.Any(x => x.Tag == vm.Tag))
                IedViewModels.Add(vm);

            AddToNavigationCommand.Execute(vm);
        });

        WeakReferenceMessenger.Default.Register<ToggleIedStateMessage>(this, (_, _) =>
        {
            ToggleAllConnectionsCommand.Execute(null);
        });
    }

    [RelayCommand]
    private void ToggleAllConnections()
    {
        foreach (ConnectedIedViewModel vm in IedViewModels)
        {
            vm.IsConnected = !vm.IsConnected;
        }
    }

    private void InitializeViewModel()
    {
        ApplicationTitle = "WPF UI - MVVM Demo";

        NavigationItems =
        [
            new NavigationViewItem()
            {
                Content = "Home",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
                TargetPageType = typeof(Views.Pages.DashboardPage)
            },
            new NavigationViewItem()
            {
                Content = "Data",
                Icon = new SymbolIcon { Symbol = SymbolRegular.DataHistogram24 },
                TargetPageType = typeof(Views.Pages.DataPage)

            },
        ];

        NavigationFooter =
        [
            new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            },
        ];

        TrayMenuItems = [new() { Header = "Home", Tag = "tray_home" }];

        _isInitialized = true;
    }

    [RelayCommand]
    private void AddToNavigation(ConnectedIedViewModel viewModel)
    {
        // “Data” 메뉴 항목 찾기
        NavigationViewItem? parent = NavigationItems
            .FirstOrDefault(n => n.Content?.ToString() == "Data");

        if (parent is null)
            return;

        // 내부 MenuItems 컬렉션을 직접 사용 (MenuItemsSource는 갱신 필요 없음)
        IList children = parent.MenuItems;

        // 중복 방지: 동일 Tag 기준
        if (children.OfType<BindableNavigationViewItem>().Any(x =>
            x.DataContext is ConnectedIedViewModel vm && vm.Tag == viewModel.Tag))
            return;

        // BindableNavigationViewItem 생성
        var item = new BindableNavigationViewItem
        {
            Content = viewModel, // ✅ Content가 템플릿 바인딩의 기준이 된다
            DataContext = viewModel, // ✅ 핵심은 DataContext에 ViewModel만 넣는 것
            ContentTemplate = (DataTemplate)Application.Current.FindResource("IedItemTemplate"),
            Icon = new SymbolIcon { Symbol = SymbolRegular.Server24 },
            TargetPageType = typeof(DataPage),
            TargetPageTag = viewModel.Tag,
            Tag = viewModel.Tag,
            IsMenuElement = true,
            IsDynamicScrollEnabled = false
        };

        _ = children.Add(item);

        parent.IsExpanded = true;

        // NavigationView 등록 (저널/탐색 가능하게)
        _navigationService?.RegisterNavigationViewItem(item);
    }
}

public partial class ConnectedIedViewModel : ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }

    public string NicName { get; set; } = string.Empty;
    public string NicIp { get; set; } = string.Empty;
    public string Tag { get; } = string.Empty;  // ✅ 추가

    //public override string ToString() => $"{Name} ({Host}) via {NicName}";

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isLoading;

    partial void OnIsConnectedChanged(bool oldValue, bool newValue)
    {
        Debug.WriteLine($"[IsConnected Changed] {Name}: {oldValue} → {newValue}");
    }

    [RelayCommand]
    private void Loaded()
    {
        Debug.WriteLine($"[Loaded] Function Call Test");
    }

    public ConnectedIedViewModel(string name, string host, int port)
    {
        Name = name;
        Host = host;
        Port = port;
        Tag = $"{Host}:{Port}@{Name}";
    }
}