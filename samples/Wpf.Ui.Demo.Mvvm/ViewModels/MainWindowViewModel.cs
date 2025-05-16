// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.
using CommunityToolkit.Mvvm.Messaging;

using System.Collections;
using System.Collections.ObjectModel;

using Wpf.Ui.Controls;
using Wpf.Ui.Demo.Mvvm.Message;
using Wpf.Ui.Demo.Mvvm.Views.Pages;

namespace Wpf.Ui.Demo.Mvvm.ViewModels;

public partial class MainWindowViewModel : ViewModel
{
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0060:Remove unused parameter",
        Justification = "Demo"
    )]
    public MainWindowViewModel(INavigationService navigationService)
    {
        if (!_isInitialized)
        {
            InitializeViewModel();
        }

        // IED 선택 메시지 받기
        WeakReferenceMessenger.Default.Register<ChildAddMessage>(this, (r, m) =>
        {
            AddToNavigation(m.child);
        });
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
    private void AddToNavigation(string child)
    {
        // “ElementSetting” 메뉴 아이템 찾기
        NavigationViewItem? parent = NavigationItems
            .FirstOrDefault(n => n.Content?.ToString() == "Home");
        if (parent == null)
            return;

        // 기존 하위 메뉴가 컬렉션이면 꺼내고, 아니면 새로 생성
        IList children = (parent.MenuItemsSource as IList)
                      ?? new ObservableCollection<NavigationViewItem>();

        // 이미 같은 IED 항목이 있는지 중복 방지
        if (children.Cast<NavigationViewItem>().Any(x => (string)x.Content == child))
            return;

        // 새로운 메뉴 아이템
        var iedNode = new NavigationViewItem(child, SymbolRegular.Server24, typeof(DataPage))
        {
            Tag = child,
            IsMenuElement = true, // 메뉴 아이템으로 설정
            // 자식 메뉴들 설정
        };
        _ = children.Add(iedNode);

        parent.IsExpanded = true; // 메뉴 확장

        // 다시 할당해야 UI가 갱신됩니다
        parent.MenuItemsSource = children;
    }
}