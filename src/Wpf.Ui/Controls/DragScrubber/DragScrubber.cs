// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using System.Windows.Input;

namespace Wpf.Ui.Controls;

[TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]

/// <summary>
/// Unity Inspector 스타일 드래그 스크러버 컨트롤
/// TextBox에 직접 입력하거나 드래그하여 실시간으로 숫자 값을 조절할 수 있습니다.
/// </summary>
public class DragScrubber : Control
{
    private bool _isDragging = false;
    private Point _dragStartPoint;
    private double _dragStartValue;
    private TextBox? _textBox;
    private Border? _labelArea;

    static DragScrubber()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DragScrubber),
            new FrameworkPropertyMetadata(typeof(DragScrubber)));
    }


    /// <summary>
    /// 현재 값
    /// </summary>
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(DragScrubber),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, CoerceValue));

    /// <summary>
    /// 최소값
    /// </summary>
    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(DragScrubber),
            new PropertyMetadata(double.MinValue));

    /// <summary>
    /// 최대값
    /// </summary>
    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(DragScrubber),
            new PropertyMetadata(double.MaxValue));

    /// <summary>
    /// 단계 크기 (드래그 시 값 변화량)
    /// </summary>
    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(
            nameof(Step),
            typeof(double),
            typeof(DragScrubber),
            new PropertyMetadata(1.0));

    /// <summary>
    /// 소수점 자릿수
    /// </summary>
    public static readonly DependencyProperty PrecisionProperty =
        DependencyProperty.Register(
            nameof(Precision),
            typeof(int),
            typeof(DragScrubber),
            new PropertyMetadata(1));

    /// <summary>
    /// 라벨 텍스트
    /// </summary>
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(DragScrubber),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// 라벨 전경 색상
    /// </summary>
    public static readonly DependencyProperty LabelForegroundProperty =
        DependencyProperty.Register(
            nameof(LabelForeground),
            typeof(System.Windows.Media.Brush),
            typeof(DragScrubber),
            new PropertyMetadata(System.Windows.Media.Brushes.Black));

    /// <summary>
    /// 라벨 폰트 크기
    /// </summary>
    public static readonly DependencyProperty LabelFontSizeProperty =
        DependencyProperty.Register(
            nameof(LabelFontSize),
            typeof(double),
            typeof(DragScrubber),
            new PropertyMetadata(12.0));

    /// <summary>
    /// 드래그 민감도 (픽셀당 값 변화량)
    /// </summary>
    public static readonly DependencyProperty DragSensitivityProperty =
        DependencyProperty.Register(
            nameof(DragSensitivity),
            typeof(double),
            typeof(DragScrubber),
            new PropertyMetadata(1.0));

    /// <summary>
    /// 드래그 라인 표시 여부
    /// </summary>
    public static readonly DependencyProperty ShowDragLineProperty =
        DependencyProperty.Register(
            nameof(ShowDragLine),
            typeof(bool),
            typeof(DragScrubber),
            new PropertyMetadata(true));

    /// <summary>
    /// 라벨 영역의 폭
    /// </summary>
    public static readonly DependencyProperty LabelWidthProperty =
        DependencyProperty.Register(
            nameof(LabelWidth),
            typeof(GridLength),
            typeof(DragScrubber),
            new PropertyMetadata(GridLength.Auto));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

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

    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public int Precision
    {
        get => (int)GetValue(PrecisionProperty);
        set => SetValue(PrecisionProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public System.Windows.Media.Brush LabelForeground
    {
        get => (System.Windows.Media.Brush)GetValue(LabelForegroundProperty);
        set => SetValue(LabelForegroundProperty, value);
    }

    public double LabelFontSize
    {
        get => (double)GetValue(LabelFontSizeProperty);
        set => SetValue(LabelFontSizeProperty, value);
    }

    public double DragSensitivity
    {
        get => (double)GetValue(DragSensitivityProperty);
        set => SetValue(DragSensitivityProperty, value);
    }

    public bool ShowDragLine
    {
        get => (bool)GetValue(ShowDragLineProperty);
        set => SetValue(ShowDragLineProperty, value);
    }

    public GridLength LabelWidth
    {
        get => (GridLength)GetValue(LabelWidthProperty);
        set => SetValue(LabelWidthProperty, value);
    }



    /// <summary>
    /// 값이 변경될 때 발생하는 이벤트
    /// </summary>
    public static readonly RoutedEvent ValueChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>),
            typeof(DragScrubber));

    public event RoutedPropertyChangedEventHandler<double> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    /// <summary>
    /// 드래그 시작 이벤트
    /// </summary>
    public event EventHandler<DragScrubberEventArgs>? DragStarted;

    /// <summary>
    /// 드래그 완료 이벤트
    /// </summary>
    public event EventHandler<DragScrubberEventArgs>? DragCompleted;

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DragScrubber scrubber)
        {
            var oldValue = (double)e.OldValue;
            var newValue = (double)e.NewValue;

            // TextBox 텍스트 업데이트 (단, TextBox가 포커스된 상태가 아닐 때만)
            if (scrubber._textBox != null && !scrubber._textBox.IsFocused)
            {
                scrubber._textBox.Text = Math.Round(newValue, scrubber.Precision).ToString();
            }

            scrubber.RaiseEvent(new RoutedPropertyChangedEventArgs<double>(oldValue, newValue, ValueChangedEvent));
        }
    }

    private static object CoerceValue(DependencyObject d, object value)
    {
        if (d is DragScrubber scrubber && value is double doubleValue)
        {
            var coerced = Math.Max(scrubber.Minimum, Math.Min(scrubber.Maximum, doubleValue));
            return Math.Round(coerced, scrubber.Precision);
        }

        return value;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 기존 이벤트 해제
        if (_textBox != null)
        {
            _textBox.LostFocus -= OnTextBoxLostFocus;
            _textBox.KeyDown -= OnTextBoxKeyDown;
        }
        
        if (_labelArea != null)
        {
            _labelArea.MouseLeftButtonDown -= OnLabelAreaMouseLeftButtonDown;
            _labelArea.MouseMove -= OnLabelAreaMouseMove;
            _labelArea.MouseLeftButtonUp -= OnLabelAreaMouseLeftButtonUp;
        }

        // 새 TextBox 및 LabelArea 찾기
        _textBox = GetTemplateChild("PART_TextBox") as TextBox;
        _labelArea = GetTemplateChild("PART_LabelArea") as Border;

        // TextBox 이벤트 연결
        if (_textBox != null)
        {
            _textBox.LostFocus += OnTextBoxLostFocus;
            _textBox.KeyDown += OnTextBoxKeyDown;
        }

        // LabelArea 드래그 이벤트 연결 (Unity 스타일)
        if (_labelArea != null)
        {
            _labelArea.MouseLeftButtonDown += OnLabelAreaMouseLeftButtonDown;
            _labelArea.MouseMove += OnLabelAreaMouseMove;
            _labelArea.MouseLeftButtonUp += OnLabelAreaMouseLeftButtonUp;
        }
    }

    private void OnLabelAreaMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_labelArea != null)
        {
            // 라벨 영역을 드래그하여 값 조절 시작
            _isDragging = true;
            _dragStartPoint = e.GetPosition(this);
            _dragStartValue = Value;

            _ = _labelArea.CaptureMouse();
            _ = VisualStateManager.GoToState(this, "Dragging", true);
            DragStarted?.Invoke(this, new DragScrubberEventArgs(Value));

            e.Handled = true;
        }
    }

    private void OnLabelAreaMouseMove(object sender, MouseEventArgs e)
    {
        if (_isDragging && e.LeftButton == MouseButtonState.Pressed && _labelArea != null)
        {
            Point currentPosition = e.GetPosition(this);
            double deltaX = currentPosition.X - _dragStartPoint.X;

            // 키보드 수정자에 따른 배율 계산
            double multiplier = GetDragMultiplier();

            // 새로운 값 계산
            double deltaValue = deltaX * Step * DragSensitivity * multiplier;
            double newValue = _dragStartValue + deltaValue;

            // 값 업데이트
            Value = newValue; // CoerceValue에서 범위 제한과 정밀도 적용됨

            e.Handled = true;
        }
    }

    private void OnLabelAreaMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging && _labelArea != null)
        {
            _isDragging = false;
            _labelArea.ReleaseMouseCapture();

            _ = VisualStateManager.GoToState(this, IsMouseOver ? "MouseOver" : "Normal", true);
            DragCompleted?.Invoke(this, new DragScrubberEventArgs(Value));

            e.Handled = true;
        }
    }

    private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
    {
        // TextBox에서 직접 입력한 값 파싱
        if (_textBox != null && double.TryParse(_textBox.Text, out double parsedValue))
        {
            Value = parsedValue;
        }
        else
        {
            // 파싱 실패 시 현재 값으로 복원
            if (_textBox != null)
            {
                _textBox.Text = Math.Round(Value, Precision).ToString();
            }
        }
    }

    private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Return)
        {
            // Enter 키로 값 확정
            if (_textBox != null && double.TryParse(_textBox.Text, out double parsedValue))
            {
                Value = parsedValue;
            }

            // 포커스 해제
            _ = (_textBox?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)));
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            // ESC 키로 취소
            if (_textBox != null)
            {
                _textBox.Text = Math.Round(Value, Precision).ToString();
            }

            // 포커스 해제
            _ = (_textBox?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)));
            e.Handled = true;
        }
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        if (!_isDragging)
        {
            _ = VisualStateManager.GoToState(this, "MouseOver", true);
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (!_isDragging)
        {
            _ = VisualStateManager.GoToState(this, "Normal", true);
        }
    }

    private double GetDragMultiplier()
    {
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            return 0.1; // 정밀 제어
        }
        else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            return 10.0; // 빠른 제어
        }

        return 1.0; // 기본
    }
}