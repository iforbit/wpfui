// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Wpf.Ui.Controls;

/// <summary>
/// Range Slider with two thumbs for selecting a sub-range within [Minimum, Maximum].
/// Supports MaxRangeSpan constraint enforced via CoerceValue — the value is clamped
/// before it is ever set, so the visual never shows an invalid range (no jitter).
/// </summary>
[TemplatePart(Name = PART_Track, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PART_SelectedRange, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PART_StartThumb, Type = typeof(Thumb))]
[TemplatePart(Name = PART_MiddleThumb, Type = typeof(Thumb))]
[TemplatePart(Name = PART_EndThumb, Type = typeof(Thumb))]
public class RangeSlider : Control
{
    private const string PART_Track = "PART_Track";
    private const string PART_SelectedRange = "PART_SelectedRange";
    private const string PART_StartThumb = "PART_StartThumb";
    private const string PART_MiddleThumb = "PART_MiddleThumb";
    private const string PART_EndThumb = "PART_EndThumb";

    private FrameworkElement? _track;
    private FrameworkElement? _selectedRange;
    private Thumb? _startThumb;
    private Thumb? _middleThumb;
    private Thumb? _endThumb;

    /// <summary>
    /// Gets a value indicating whether 사용자가 썸을 드래그 중인지 여부 — 드래그 중에는 외부 프로그래밍 업데이트 차단 가능
    /// </summary>
    public bool IsDragging { get; private set; }

    static RangeSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(typeof(RangeSlider)));
    }

    /// <summary>
    /// Minimum value of the slider range.
    /// </summary>
    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.AffectsArrange,
                OnRangePropertyChanged));

    /// <summary>
    /// Maximum value of the slider range.
    /// </summary>
    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(
                100.0,
                FrameworkPropertyMetadataOptions.AffectsArrange,
                OnRangePropertyChanged));

    /// <summary>
    /// Start of the selected range. Coerced to [Minimum, SelectionEnd] and MaxRangeSpan.
    /// </summary>
    public static readonly DependencyProperty SelectionStartProperty =
        DependencyProperty.Register(
            nameof(SelectionStart),
            typeof(double),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsArrange,
                OnSelectionStartChanged,
                CoerceSelectionStart));

    /// <summary>
    /// End of the selected range. Coerced to [SelectionStart, Maximum] and MaxRangeSpan.
    /// </summary>
    public static readonly DependencyProperty SelectionEndProperty =
        DependencyProperty.Register(
            nameof(SelectionEnd),
            typeof(double),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(
                100.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsArrange,
                OnSelectionEndChanged,
                CoerceSelectionEnd));

    /// <summary>
    /// Maximum allowed span between SelectionStart and SelectionEnd.
    /// 0 means no constraint. Enforced via CoerceValue on both properties.
    /// </summary>
    public static readonly DependencyProperty MaxRangeSpanProperty =
        DependencyProperty.Register(
            nameof(MaxRangeSpan),
            typeof(double),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(
                0.0,
                OnMaxRangeSpanChanged));

    /// <summary>
    /// Minimum allowed span between SelectionStart and SelectionEnd.
    /// </summary>
    public static readonly DependencyProperty MinRangeSpanProperty =
        DependencyProperty.Register(
            nameof(MinRangeSpan),
            typeof(double),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(1.0));

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double SelectionStart
    {
        get => (double)GetValue(SelectionStartProperty);
        set => SetValue(SelectionStartProperty, value);
    }

    public double SelectionEnd
    {
        get => (double)GetValue(SelectionEndProperty);
        set => SetValue(SelectionEndProperty, value);
    }

    public double MaxRangeSpan
    {
        get => (double)GetValue(MaxRangeSpanProperty);
        set => SetValue(MaxRangeSpanProperty, value);
    }

    public double MinRangeSpan
    {
        get => (double)GetValue(MinRangeSpanProperty);
        set => SetValue(MinRangeSpanProperty, value);
    }

    /// <summary>
    /// Raised when SelectionStart or SelectionEnd changes.
    /// </summary>
    public static readonly RoutedEvent SelectionChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(SelectionChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(RangeSlider));

    public event RoutedEventHandler SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    private static object CoerceSelectionStart(DependencyObject d, object? baseValue)
    {
        var slider = (RangeSlider)d;
        double value = (double)baseValue;

        // Clamp to [Minimum, SelectionEnd]
        value = Math.Max(value, slider.Minimum);
        value = Math.Min(value, slider.SelectionEnd);

        // MaxRangeSpan: if span would exceed max, push start forward
        double maxSpan = slider.MaxRangeSpan;
        if (maxSpan > 0)
        {
            double minAllowedStart = slider.SelectionEnd - maxSpan;
            value = Math.Max(value, minAllowedStart);
        }

        // MinRangeSpan: ensure minimum span
        double minSpan = slider.MinRangeSpan;
        if (minSpan > 0)
        {
            double maxAllowedStart = slider.SelectionEnd - minSpan;
            value = Math.Min(value, Math.Max(slider.Minimum, maxAllowedStart));
        }

        return value;
    }

    private static object CoerceSelectionEnd(DependencyObject d, object? baseValue)
    {
        var slider = (RangeSlider)d;
        double value = (double)baseValue;

        // Clamp to [SelectionStart, Maximum]
        value = Math.Min(value, slider.Maximum);
        value = Math.Max(value, slider.SelectionStart);

        // MaxRangeSpan: if span would exceed max, push end backward
        double maxSpan = slider.MaxRangeSpan;
        if (maxSpan > 0)
        {
            double maxAllowedEnd = slider.SelectionStart + maxSpan;
            value = Math.Min(value, maxAllowedEnd);
        }

        // MinRangeSpan: ensure minimum span
        double minSpan = slider.MinRangeSpan;
        if (minSpan > 0)
        {
            double minAllowedEnd = slider.SelectionStart + minSpan;
            value = Math.Max(value, Math.Min(slider.Maximum, minAllowedEnd));
        }

        return value;
    }

    private static void OnRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        slider.CoerceValue(SelectionStartProperty);
        slider.CoerceValue(SelectionEndProperty);
        slider.UpdateTrackLayout();
    }

    private static void OnSelectionStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        slider.CoerceValue(SelectionEndProperty);
        slider.UpdateTrackLayout();
        slider.RaiseEvent(new RoutedEventArgs(SelectionChangedEvent, slider));
    }

    private static void OnSelectionEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        slider.CoerceValue(SelectionStartProperty);
        slider.UpdateTrackLayout();
        slider.RaiseEvent(new RoutedEventArgs(SelectionChangedEvent, slider));
    }

    private static void OnMaxRangeSpanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        slider.CoerceValue(SelectionStartProperty);
        slider.CoerceValue(SelectionEndProperty);
        slider.UpdateTrackLayout();
    }

    public override void OnApplyTemplate()
    {
        // Detach old handlers
        if (_startThumb != null)
        {
            _startThumb.DragStarted -= OnThumbDragStarted;
            _startThumb.DragCompleted -= OnThumbDragCompleted;
            _startThumb.DragDelta -= OnStartThumbDragDelta;
        }

        if (_middleThumb != null)
        {
            _middleThumb.DragStarted -= OnThumbDragStarted;
            _middleThumb.DragCompleted -= OnThumbDragCompleted;
            _middleThumb.DragDelta -= OnMiddleThumbDragDelta;
        }

        if (_endThumb != null)
        {
            _endThumb.DragStarted -= OnThumbDragStarted;
            _endThumb.DragCompleted -= OnThumbDragCompleted;
            _endThumb.DragDelta -= OnEndThumbDragDelta;
        }

        base.OnApplyTemplate();

        _track = GetTemplateChild(PART_Track) as FrameworkElement;
        _selectedRange = GetTemplateChild(PART_SelectedRange) as FrameworkElement;
        _startThumb = GetTemplateChild(PART_StartThumb) as Thumb;
        _middleThumb = GetTemplateChild(PART_MiddleThumb) as Thumb;
        _endThumb = GetTemplateChild(PART_EndThumb) as Thumb;

        if (_startThumb != null)
        {
            _startThumb.DragStarted += OnThumbDragStarted;
            _startThumb.DragCompleted += OnThumbDragCompleted;
            _startThumb.DragDelta += OnStartThumbDragDelta;
        }

        if (_middleThumb != null)
        {
            _middleThumb.DragStarted += OnThumbDragStarted;
            _middleThumb.DragCompleted += OnThumbDragCompleted;
            _middleThumb.DragDelta += OnMiddleThumbDragDelta;
        }

        if (_endThumb != null)
        {
            _endThumb.DragStarted += OnThumbDragStarted;
            _endThumb.DragCompleted += OnThumbDragCompleted;
            _endThumb.DragDelta += OnEndThumbDragDelta;
        }

        UpdateTrackLayout();
    }

    /// <summary>
    /// Convert pixel drag delta to value delta.
    /// Formula: delta = pixelDrag × (Maximum - Minimum) / trackPixelLength
    /// </summary>
    private double PixelToValueDelta(double horizontalChange)
    {
        double trackLength = _track?.ActualWidth ?? ActualWidth;
        if (trackLength <= 0)
        {
            return 0;
        }

        double range = Maximum - Minimum;
        if (range <= 0)
        {
            return 0;
        }

        return horizontalChange * range / trackLength;
    }

    /// <summary>
    /// 사용자가 썸 드래그를 완료했을 때 발생 — 외부에서 뷰포트 재동기화 트리거 가능
    /// </summary>
    public event EventHandler? SliderDragCompleted;

    private void OnThumbDragStarted(object sender, DragStartedEventArgs e) => IsDragging = true;

    private void OnThumbDragCompleted(object sender, DragCompletedEventArgs e)
    {
        IsDragging = false;
        SliderDragCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void OnStartThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        double delta = PixelToValueDelta(e.HorizontalChange);
        SetCurrentValue(SelectionStartProperty, SelectionStart + delta);
    }

    private void OnEndThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        double delta = PixelToValueDelta(e.HorizontalChange);
        SetCurrentValue(SelectionEndProperty, SelectionEnd + delta);
    }

    private void OnMiddleThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        double delta = PixelToValueDelta(e.HorizontalChange);
        double newStart = SelectionStart + delta;
        double newEnd = SelectionEnd + delta;

        // Clamp to bounds while preserving span
        if (newStart < Minimum)
        {
            double shift = Minimum - newStart;
            newStart = Minimum;
            newEnd += shift;
        }
        else if (newEnd > Maximum)
        {
            double shift = newEnd - Maximum;
            newEnd = Maximum;
            newStart -= shift;
        }

        // drag 방향에 따라 설정 순서 결정 — 순서가 틀리면 MaxRangeSpan coercion이
        // 반대쪽 값을 당겨 span이 줄어드는 cascade 발생
        if (delta < 0)
        {
            // 좌이동: End 먼저 줄여야 Start coercion이 End를 추가로 당기지 않음
            SetCurrentValue(SelectionEndProperty, newEnd);
            SetCurrentValue(SelectionStartProperty, newStart);
        }
        else
        {
            // 우이동: Start 먼저 늘려야 End coercion이 Start를 추가로 밀지 않음
            SetCurrentValue(SelectionStartProperty, newStart);
            SetCurrentValue(SelectionEndProperty, newEnd);
        }
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        Size result = base.ArrangeOverride(arrangeBounds);
        UpdateTrackLayout();
        return result;
    }

    /// <summary>
    /// Position thumbs and selected range based on current values.
    /// </summary>
    private void UpdateTrackLayout()
    {
        if (_track == null && _selectedRange == null)
        {
            return;
        }

        double trackWidth = _track?.ActualWidth ?? ActualWidth;
        if (trackWidth <= 0)
        {
            return;
        }

        double range = Maximum - Minimum;
        if (range <= 0)
        {
            return;
        }

        double coef = trackWidth / range;
        double startPos = (SelectionStart - Minimum) * coef;
        double endPos = (SelectionEnd - Minimum) * coef;
        double rangeWidth = Math.Max(0, endPos - startPos);

        // Position selected range indicator
        if (_selectedRange != null)
        {
            _selectedRange.SetCurrentValue(MarginProperty, new Thickness(startPos, 0, 0, 0));
            _selectedRange.SetCurrentValue(WidthProperty, rangeWidth);
        }

        // Position start thumb (center on startPos)
        if (_startThumb != null)
        {
            double thumbHalf = _startThumb.ActualWidth > 0 ? _startThumb.ActualWidth / 2.0 : 6;
            Canvas.SetLeft(_startThumb, startPos - thumbHalf);
        }

        // Position end thumb (center on endPos)
        if (_endThumb != null)
        {
            double thumbHalf = _endThumb.ActualWidth > 0 ? _endThumb.ActualWidth / 2.0 : 6;
            Canvas.SetLeft(_endThumb, endPos - thumbHalf);
        }

        // Position middle thumb (fill between start and end)
        if (_middleThumb != null)
        {
            double startThumbW = _startThumb?.ActualWidth ?? 12;
            double endThumbW = _endThumb?.ActualWidth ?? 12;
            double middleLeft = startPos + (startThumbW / 2.0);
            double middleWidth = Math.Max(0, rangeWidth - (startThumbW / 2.0) - (endThumbW / 2.0));

            Canvas.SetLeft(_middleThumb, middleLeft);
            _middleThumb.SetCurrentValue(WidthProperty, middleWidth);
        }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (_track == null)
        {
            return;
        }

        // Click on track area (not on a thumb) → move nearest thumb
        Point pos = e.GetPosition(_track);
        double clickValue = PixelToValue(pos.X);

        double distToStart = Math.Abs(clickValue - SelectionStart);
        double distToEnd = Math.Abs(clickValue - SelectionEnd);

        if (distToStart <= distToEnd)
        {
            SetCurrentValue(SelectionStartProperty, clickValue);
        }
        else
        {
            SetCurrentValue(SelectionEndProperty, clickValue);
        }
    }

    private double PixelToValue(double pixelX)
    {
        double trackWidth = _track?.ActualWidth ?? ActualWidth;
        if (trackWidth <= 0)
        {
            return Minimum;
        }

        double ratio = Math.Clamp(pixelX / trackWidth, 0, 1);
        return Minimum + (ratio * (Maximum - Minimum));
    }
}
