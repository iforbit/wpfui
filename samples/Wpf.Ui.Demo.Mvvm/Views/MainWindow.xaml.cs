// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.Abstractions;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Demo.Mvvm.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : INavigationWindow
{
    public ViewModels.MainWindowViewModel ViewModel { get; }

    private readonly INavigationService _navigationService;
    public MainWindow(ViewModels.MainWindowViewModel viewModel, INavigationService navigationService)
    {
        ViewModel = viewModel;
        DataContext = this;

        Appearance.SystemThemeWatcher.Watch(this);

        InitializeComponent();
        // 내비게이션 컨트롤 설정
        _navigationService = navigationService;
        navigationService.SetNavigationControl(RootNavigation);

        // 내비게이션 이벤트 구독 (페이지 전환 시 Ribbon 업데이트)
        _navigationService.GetNavigationControl().Navigated += OnNavigated;
    }

    public INavigationView GetNavigation() => RootNavigation;

    public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

    public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) =>
        RootNavigation.SetPageProviderService(navigationViewPageProvider);

    public void ShowWindow() => Show();

    public void CloseWindow() => Close();

    /// <summary>
    /// Raises the closed event.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // 내비게이션 이벤트 구독 해제
        _navigationService.GetNavigationControl().Navigated -= OnNavigated;

        // Make sure that closing this window will begin the process of closing the application.
        Application.Current.Shutdown();
    }

    INavigationView INavigationWindow.GetNavigation()
    {
        throw new NotImplementedException();
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 내비게이션이 완료되면 호출됩니다.
    /// 현재 페이지가 IRibbonProvider를 구현하면 해당 Ribbon 탭 정보를 MainRibbon에 업데이트합니다.
    /// </summary>
    private void OnNavigated(object sender, NavigatedEventArgs e)
    {
        /*
        // 내비게이션된 페이지가 INavigableView<T> (T가 무엇이든)인지 공변성 덕분에 INavigableView<object>로 체크할 수 있습니다.
        if (e.Page is INavigableView<object> navigableView)
        {
            // navigableView.ViewModel은 실제로 해당 페이지의 뷰모델(구체적인 타입)이 됩니다.
            var viewModel = navigableView.ViewModel;

            // e.Page는 내비게이션을 통해 전환된 페이지 인스턴스입니다.
            if (viewModel is IRibbonProvider ribbonProvider)
            {
                // MainRibbon은 XAML에 선언된 Ribbon 컨트롤이며, ItemsSource를 통해 탭 목록을 설정합니다.
                MainRibbon.ItemsSource = ribbonProvider.RibbonTabs;
            }
            else
            {
                // IRibbonProvider를 구현하지 않는 페이지의 경우 Ribbon 탭을 비웁니다.
                MainRibbon.ItemsSource = null;
            }

            ViewModel.CurrentViewModel = viewModel;

        }
        else
        {
            // 해당 페이지가 INavigableView<T>를 구현하지 않는 경우 처리
            MainRibbon.ItemsSource = null;
        }
        */

        // e.Page는 전환된 페이지 인스턴스입니다.
        if (e.Page is IRibbonProvider ribbonProvider)
        {
            // MainRibbonPlaceholder의 Content를 페이지가 제공하는 RibbonContent로 대체합니다.
            MainRibbonPlaceholder.Content = ribbonProvider.RibbonContent;
        }
        else
        {
            MainRibbonPlaceholder.Content = null;
        }
    }
}