// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;
using Wpf.Ui.Demo.Mvvm.ViewModels;
using Wpf.Ui.DirectX.Threading;

namespace Wpf.Ui.Demo.Mvvm.Views.Pages;

public partial class DashboardPage : INavigableView<ViewModels.DashboardViewModel>, IRibbonProvider
{
    public ViewModels.DashboardViewModel ViewModel { get; }
    private readonly IRenderThreadService _renderThread;
    public DashboardPage(DashboardViewModel viewModel, IRenderThreadService renderThread)
    {
        ViewModel = viewModel;
        _renderThread = renderThread;

        DataContext = this;
        InitializeComponent();

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.SetGraphControl(DxGraph); // ✅ RenderThread는 ViewModel이 이미 갖고 있음
    }

    // IRibbonProvider 구현 – 여기서는 XAML 리소스에서 정의된 Ribbon을 반환한다고 가정
    public object RibbonContent
    {
        get
        {

            // 예를 들어, 리소스에서 Ribbon을 찾거나,
            // 또는 페이지의 특정 이름을 가진 Ribbon 요소를 찾아 반환합니다.
            return this.FindResource("DashboardPageRibbon");
        }
    }
}