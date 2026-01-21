// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Wpf.Ui.Controls;

/// <summary>
/// Multi-Track Timeline 컨트롤 v1.0
///
/// Unity/Blender 스타일의 속성별 다중 트랙 타임라인
/// - 좌측: 트랙 목록 패널 (TreeView 기반 계층 구조)
/// - 우측: 타임라인 캔버스 (키프레임 배치/편집)
/// - 상단: 타임 룰러 + 전역 컨트롤
///
/// 주요 기능:
/// - 트랙 그룹 접기/펼치기
/// - 속성별 키프레임 편집
/// - 값 유형별 UI (Numeric, Color, Enum)
/// - 줌/팬 지원
/// - Scrubber 드래그
/// </summary>
[TemplatePart(Name = PART_TimeRuler, Type = typeof(Canvas))]
[TemplatePart(Name = PART_TrackListPanel, Type = typeof(ItemsControl))]
[TemplatePart(Name = PART_TrackCanvas, Type = typeof(Canvas))]
[TemplatePart(Name = PART_Scrubber, Type = typeof(Thumb))]
[TemplatePart(Name = PART_TrackScrollViewer, Type = typeof(ScrollViewer))]
[TemplatePart(Name = PART_KeyframeScrollViewer, Type = typeof(DynamicScrollViewer))]
[TemplatePart(Name = PART_ViewportThumb, Type = typeof(Thumb))]
public partial class MultiTrackTimeline : Control
{
    private const string PART_TimeRuler = "PART_TimeRuler";
    private const string PART_TrackListPanel = "PART_TrackListPanel";
    private const string PART_TrackCanvas = "PART_TrackCanvas";
    private const string PART_Scrubber = "PART_Scrubber";
    private const string PART_TrackScrollViewer = "PART_TrackScrollViewer";
    private const string PART_KeyframeScrollViewer = "PART_KeyframeScrollViewer";
    private const string PART_ViewportThumb = "PART_ViewportThumb";
    private const double CollisionMargin = 0.02; // 충돌 여유 (20ms)

    // Template parts
    private Canvas? _timeRuler;
    private ItemsControl? _trackListPanel;
    private Canvas? _trackCanvas;
    private Thumb? _scrubber;
    private ScrollViewer? _trackScrollViewer;
    private DynamicScrollViewer? _keyframeScrollViewer;
    private Thumb? _viewportThumb;
    private FrameworkElement? _viewportTrack;

    // Scroll synchronization
    private bool _isSyncingScroll;

    // Dragging state
    private bool _isDraggingKeyframe;
    private bool _isDraggingSegmentLeft;
    private bool _isDraggingSegmentRight;
    private bool _isDraggingWaypoint;
    private ITimelineKeyframe? _draggingKeyframe;
    private ITimelineSegment? _draggingSegment;
    private SegmentWaypoint? _draggingWaypoint;
    private ITimelineTrack? _draggingTrack;

    // Panning state (TrackCanvas - right click)
    private bool _isPanning;
    private Point _panStartPoint;
    private double _panStartViewportStart;

    // Panning state (TimeRuler - left click)
    private bool _isRulerPanning;
    private Point _rulerPanStartPoint;
    private double _rulerPanStartViewportStart;

    static MultiTrackTimeline()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MultiTrackTimeline),
            new FrameworkPropertyMetadata(typeof(MultiTrackTimeline)));
    }

    public MultiTrackTimeline()
    {
        TrackGroups = new ObservableCollection<ITimelineTrackGroup>();
    }
}
