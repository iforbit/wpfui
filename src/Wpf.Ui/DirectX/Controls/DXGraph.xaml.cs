// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

using Wpf.Ui.DirectX.Models;
using Wpf.Ui.DirectX.Services;

namespace Wpf.Ui.DirectX.Controls;

/// <summary>
/// DXGraph.xaml에 대한 상호 작용 논리
/// </summary>
public partial class DXGraph : UserControl
{
    private DateTime _lastUserScrollTime = DateTime.MinValue;
    private readonly TimeSpan _autoScrollResumeDelay = TimeSpan.FromSeconds(3);
    private bool _suppressScrollUpdate = false;

    private readonly DispatcherTimer _updateTimer;

    public GraphControl GraphControl => Graph;

    public ScrollBar ScrollX => ScrollBarX;

    public DXGraph()
    {
        InitializeComponent();

        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _updateTimer.Tick += (_, _) => OnUpdateTick();
        _updateTimer.Start();

        // PreviewMouseWheel += DXGraph_MouseWheel;
    }

    private void OnUpdateTick()
    {
        if (Graph.EnableAutoScroll)
        {
            float center = Graph.LastX;
            float range = Graph.VisibleRange; // ✅ 사용자 정의 범위 유지
            float min = center - (range * 0.5f);
            float max = center + (range * 0.5f);
            Graph.SetVisibleRange(min, max);
            Graph.ApplyTransformFromView();
        }

        Graph.AutoAdjustTransform();
    }

    public void AttachRenderThread(IRenderThreadService thread)
    {
        Graph.AttachRenderThread(thread);
    }

    public void AddSeries<T>(GraphSeries<T> series)
        where T : unmanaged
    {
        Graph.AddSeries(series);
    }

    public void SetVisibleRange(float min, float max)
    {
        Graph.EnableAutoScroll = false; // 사용자가 직접 조작한 것으로 간주
        _lastUserScrollTime = DateTime.Now;
        Graph.SetVisibleRange(min, max);
    }

    public void SetYScaleLock(bool lockY)
    {
        Graph.LockYScale = lockY;
    }

    public void SetAutoScrollEnabled(bool enabled)
    {
        Graph.EnableAutoScroll = enabled;
    }

    public bool IsAutoScrollEnabled => Graph.EnableAutoScroll;

    public void SyncScrollBarToView()
    {
        if (ScrollBarX == null)
        {
            return;
        }

        _suppressScrollUpdate = true;
        ScrollBarX.Value = Graph.ViewCenter;
        _suppressScrollUpdate = false;
    }

    public void UpdateAutoScroll()
    {
        // 일정 시간 경과 시 자동스크롤 복귀
        // if (!Graph.EnableAutoScroll && (DateTime.Now - _lastUserScrollTime) > _autoScrollResumeDelay)
        // {
        //    Graph.EnableAutoScroll = true;
        // }
        Graph.UpdateAutoScrollLogic();
        SyncScrollBarToView();
    }

    private void DXGraph_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        float center = Graph.ViewCenter;
        float range = Graph.VisibleRange;

        // 줌 비율 조정 (10% 확대/축소)
        float zoomFactor = e.Delta > 0 ? 0.9f : 1.1f;

        // 범위 제한 (최소 100ms ~ 최대 10초)
        float newRange = Math.Clamp(range * zoomFactor, 100f, 20000f);

        float min = center - (newRange * 0.5f);
        float max = center + (newRange * 0.5f);

        Debug.WriteLine($"🧪 VisibleRange:{newRange:F1} ms (center={center:F0})");

        SetVisibleRange(min, max);
        Graph.ApplyTransformFromView();

        e.Handled = true;
    }

    private void ScrollBarX_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded || _suppressScrollUpdate)
        {
            return;
        }

        Graph.EnableAutoScroll = false;
        _lastUserScrollTime = DateTime.Now;

        float center = (float)e.NewValue;
        float range = Graph.VisibleRange;
        Graph.SetVisibleRange(center - (range * 0.5f), center + (range * 0.5f));
        Graph.ApplyTransformFromView(); // 명시적으로 Transform 갱신
    }
}
