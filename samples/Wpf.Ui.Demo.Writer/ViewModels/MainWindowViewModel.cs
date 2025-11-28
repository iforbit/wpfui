// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.
using System.Collections.ObjectModel;
using System.Diagnostics;

using Wpf.Ui.Controls;

namespace Wpf.Ui.Demo.Writer.ViewModels;

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
                TargetPageType = typeof(Views.Pages.WriteBoardPage)
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