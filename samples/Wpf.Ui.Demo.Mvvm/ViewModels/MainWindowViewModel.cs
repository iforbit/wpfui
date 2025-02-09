// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Demo.Mvvm.ViewModels;

public partial class MainWindowViewModel : ViewModel
{
    private bool _isInitialized = false;

    [ObservableProperty]
    private string _applicationTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<object> _navigationItems = [];

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

        TabItems =
        [
            new RibbonTabItem()
            {
                Header = "Home",
                //Groups = new ObservableCollection<RibbonGroupBox>()
                //{
                //    new RibbonGroupBox
                //    {
                //        Header = "Group 1",
                //        ItemsSource = new ObservableCollection<object>()
                //        {
                //            new Button()
                //            {
                //                Content = "Button 1",
                //                Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 }
                //            },
                //            new Button()
                //            {
                //                Content = "Button 2",
                //                Icon = new SymbolIcon { Symbol = SymbolRegular.DataHistogram24 }
                //            },
                //        }
                //    },
                //}
            },
             new RibbonTabItem()
            {
                Header = "Home1",
            },
             new RibbonTabItem()
            {
                Header = "Home2",
            },
             new RibbonTabItem()
            {
                Header = "Home3",
            },
             new RibbonTabItem()
            {
                Header = "Home4",
            },
             new RibbonTabItem()
            {
                Header = "Home5",
            },
             new RibbonTabItem()
            {
                Header = "Home6",
            },
        ];

        _isInitialized = true;
    }
}