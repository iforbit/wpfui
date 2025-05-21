// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Demo.Mvvm.Views.Pages;

/// <summary>
/// Interaction logic for DashboardPage.xaml
/// </summary>
public partial class DashboardPage : INavigableView<ViewModels.DashboardViewModel>, IRibbonProvider
{
    public ViewModels.DashboardViewModel ViewModel { get; }

    public DashboardPage(ViewModels.DashboardViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    // IRibbonProvider 구현 – 여기서는 XAML 리소스에서 정의된 Ribbon을 반환한다고 가정
    public object RibbonContent
    {
        get
        {
            var ribbon = (FrameworkElement)this.FindResource("DashboardPageRibbon");
            ribbon.DataContext = ViewModel; // 페이지의 ViewModel 전달
            // 예를 들어, 리소스에서 Ribbon을 찾거나,
            // 또는 페이지의 특정 이름을 가진 Ribbon 요소를 찾아 반환합니다.
            return ribbon;
        }
    }
}