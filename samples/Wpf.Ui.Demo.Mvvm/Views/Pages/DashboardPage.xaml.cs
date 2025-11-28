// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Media;
using System.Windows.Threading;

using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;
using Wpf.Ui.Demo.Mvvm.ViewModels;
using Wpf.Ui.DirectX.Services;

namespace Wpf.Ui.Demo.Mvvm.Views.Pages;

public partial class DashboardPage : INavigableView<ViewModels.DashboardViewModel>, IRibbonProvider
{
    public ViewModels.DashboardViewModel ViewModel { get; }
    private readonly IRenderThreadService _renderThread;
    private int _frameCount = 0;
    private DateTime _lastFpsUpdate = DateTime.Now;

    private DispatcherTimer _updateTimer = null!;
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
        CompositionTarget.Rendering += OnRendering;
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };

        _updateTimer.Tick += (_, _) =>
        {
            _frameCount++;
            if ((DateTime.Now - _lastFpsUpdate).TotalSeconds >= 1)
            {
                double tick = _frameCount / (DateTime.Now - _lastFpsUpdate).TotalSeconds;
                FpsTextBlock.Text = $"WPF Tick: {tick:F1}";
                _frameCount = 0;
                _lastFpsUpdate = DateTime.Now;
            }
        };

        _updateTimer.Start(); // ✅ Tick 설정 후 Start 한 번만
    }

    private int _renderFrameCount = 0;
    private DateTime _lastUpdate = DateTime.Now;

    // 화면 렌더링 속도.
    private void OnRendering(object? sender, EventArgs e)
    {
        _renderFrameCount++;
        TimeSpan elapsed = DateTime.Now - _lastUpdate;

        if (elapsed.TotalSeconds >= 1)
        {
            double render = _renderFrameCount / elapsed.TotalSeconds;
            RenderTextBlock.Text = $"Render FPS: {render:F1}";
            _renderFrameCount = 0;
            _lastUpdate = DateTime.Now;
        }
    }

    // IRibbonProvider 구현 – 여기서는 XAML 리소스에서 정의된 Ribbon을 반환한다고 가정
    public object RibbonContent
    {
        get
        {
            // 예를 들어, 리소스에서 Ribbon을 찾거나,
            // 또는 페이지의 특정 이름을 가진 Ribbon 요소를 찾아 반환합니다.
            var ribbon = this.FindResource("DashboardPageRibbon");

            // Ribbon의 DataContext를 Page로 설정하여 바인딩이 작동하도록 함
            if (ribbon is FrameworkElement element)
            {
                element.DataContext = this;
                System.Diagnostics.Debug.WriteLine($"[RibbonContent] Ribbon DataContext set to: {this.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"[RibbonContent] ViewModel is null: {ViewModel == null}");
                System.Diagnostics.Debug.WriteLine($"[RibbonContent] NewDocumentCommand is null: {ViewModel?.NewDocumentCommand == null}");
            }

            return ribbon;
        }
    }
}