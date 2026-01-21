// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Controls;

/// <summary>
/// Extended TextBox with floating label functionality and additional content support.
/// Based on Material Design floating label pattern.
/// </summary>
[TemplatePart(Name = PART_RootElement, Type = typeof(Grid))]
[TemplatePart(Name = PART_LabelElement, Type = typeof(Label))]
[TemplatePart(Name = PART_AdditionalContentHost, Type = typeof(ContentPresenter))]
[StyleTypedProperty(Property = nameof(LabelStyle), StyleTargetType = typeof(Label))]
public class LabelText : TextBox
{
    // Template part names
    private const string PART_RootElement = "PART_RootElement";
    private const string PART_LabelElement = "PART_LabelElement";
    private const string PART_AdditionalContentHost = "PART_AdditionalContentHost";

    // Animation constants
    private const double LABEL_FLOATED_SCALE = 0.75; // 25% 축소
    private const double ANIMATION_DURATION_MS = 400; // 빠른 반응성

    // Template parts
    private Grid? _rootElement;
    private Label? _labelElement;
    private ContentPresenter? _additionalContentHost;

    // Animation state
    private bool _isAnimating;
    private Storyboard? _currentStoryboard;

    static LabelText()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LabelText), new FrameworkPropertyMetadata(typeof(LabelText)));
    }

    /// <summary>Identifies the <see cref="Label"/> dependency property.</summary>
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label),
        typeof(string),
        typeof(LabelText),
        new PropertyMetadata(string.Empty, OnLabelChanged)
    );

    /// <summary>Identifies the <see cref="LabelStyle"/> dependency property.</summary>
    public static readonly DependencyProperty LabelStyleProperty = DependencyProperty.Register(
        nameof(LabelStyle),
        typeof(Style),
        typeof(LabelText),
        new PropertyMetadata(null)
    );

    /// <summary>Identifies the <see cref="KeepLabelFloated"/> dependency property.</summary>
    public static readonly DependencyProperty KeepLabelFloatedProperty = DependencyProperty.Register(
        nameof(KeepLabelFloated),
        typeof(bool),
        typeof(LabelText),
        new PropertyMetadata(false, OnKeepLabelFloatedChanged)
    );

    /// <summary>Identifies the <see cref="AdditionalContent"/> dependency property.</summary>
    public static readonly DependencyProperty AdditionalContentProperty = DependencyProperty.Register(
        nameof(AdditionalContent),
        typeof(object),
        typeof(LabelText),
        new PropertyMetadata(null)
    );

    /// <summary>Identifies the <see cref="AdditionalContentTemplate"/> dependency property.</summary>
    public static readonly DependencyProperty AdditionalContentTemplateProperty = DependencyProperty.Register(
        nameof(AdditionalContentTemplate),
        typeof(DataTemplate),
        typeof(LabelText),
        new PropertyMetadata(null)
    );

    /// <summary>Identifies the <see cref="HasLabel"/> dependency property.</summary>
    public static readonly DependencyProperty HasLabelProperty = DependencyProperty.Register(
        nameof(HasLabel),
        typeof(bool),
        typeof(LabelText),
        new PropertyMetadata(false)
    );

    /// <summary>Identifies the <see cref="IsLabelFloated"/> dependency property.</summary>
    public static readonly DependencyProperty IsLabelFloatedProperty = DependencyProperty.Register(
        nameof(IsLabelFloated),
        typeof(bool),
        typeof(LabelText),
        new PropertyMetadata(false)
    );

    /// <summary>Identifies the <see cref="FloatedFontSize"/> dependency property.</summary>
    public static readonly DependencyProperty FloatedFontSizeProperty = DependencyProperty.Register(
        nameof(FloatedFontSize),
        typeof(double),
        typeof(LabelText),
        new PropertyMetadata(0.0, OnFloatedFontSizeChanged)
    );

    public static readonly DependencyProperty IsSimpleProperty =
    DependencyProperty.Register(nameof(IsSimple), typeof(bool), typeof(LabelText));

    /// <summary>
    /// Gets or sets the text used as a floating label.
    /// </summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// Gets or sets the style of the floating label.
    /// </summary>
    public Style? LabelStyle
    {
        get => (Style?)GetValue(LabelStyleProperty);
        set => SetValue(LabelStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the label should always stay floated on top.
    /// </summary>
    public bool KeepLabelFloated
    {
        get => (bool)GetValue(KeepLabelFloatedProperty);
        set => SetValue(KeepLabelFloatedProperty, value);
    }

    /// <summary>
    /// Gets or sets additional content displayed on the right side of the control.
    /// </summary>
    public object? AdditionalContent
    {
        get => GetValue(AdditionalContentProperty);
        set => SetValue(AdditionalContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the template for additional content.
    /// </summary>
    public DataTemplate? AdditionalContentTemplate
    {
        get => (DataTemplate?)GetValue(AdditionalContentTemplateProperty);
        set => SetValue(AdditionalContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the font size when the label is floated.
    /// If not set (0), defaults to FontSize * 0.75.
    /// </summary>
    public double FloatedFontSize
    {
        get => (double)GetValue(FloatedFontSizeProperty);
        set => SetValue(FloatedFontSizeProperty, value);
    }

    /// <summary>
    /// Gets a value indicating whether the control has a label.
    /// </summary>
    public bool HasLabel
    {
        get => (bool)GetValue(HasLabelProperty);
        private set => SetValue(HasLabelProperty, value);
    }

    /// <summary>
    /// Gets a value indicating whether the label is currently floated.
    /// </summary>
    public bool IsLabelFloated
    {
        get => (bool)GetValue(IsLabelFloatedProperty);
        private set => SetValue(IsLabelFloatedProperty, value);
    }

    public bool IsSimple
    {
        get => (bool)GetValue(IsSimpleProperty);
        set => SetValue(IsSimpleProperty, value);
    }

    /// <summary>
    /// Gets the effective floated scale, considering user setting or default calculation.
    /// </summary>
    internal double EffectiveFloatedScale => FloatedFontSize > 0 ? FloatedFontSize / FontSize : LABEL_FLOATED_SCALE;

    /// <summary>
    /// Gets a value indicating whether the text is empty.
    /// </summary>
    internal bool IsTextEmpty => string.IsNullOrEmpty(Text);

    /// <summary>
    /// Initializes a new instance of the <see cref="LabelText"/> class.
    /// </summary>
    public LabelText()
    {
        Loaded += OnLoaded;
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // Get template parts
        _rootElement = GetTemplateChild(PART_RootElement) as Grid;
        _labelElement = GetTemplateChild(PART_LabelElement) as Label;
        _additionalContentHost = GetTemplateChild(PART_AdditionalContentHost) as ContentPresenter;

        // Initialize label state
        UpdateLabelVisibility();

        // Set initial font size for label (분리된 크기 설정)
        if (_labelElement != null)
        {
            // Label의 FontSize를 부모와 분리
            _labelElement.ClearValue(Control.FontSizeProperty);
            _labelElement.SetCurrentValue(FontSizeProperty, FontSize);

            // Telerik 방식: 초기 Transform 설정
            SetupInitialTransform();
        }
    }

    /// <inheritdoc />
    protected override void OnGotFocus(RoutedEventArgs e)
    {
        base.OnGotFocus(e);

        if (HasLabel && !IsLabelFloated)
        {
            AnimateLabelToFloated();
        }
    }

    /// <inheritdoc />
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);

        if (HasLabel && IsLabelFloated && IsTextEmpty && !KeepLabelFloated)
        {
            AnimateLabelToCenter();
        }
    }

    /// <inheritdoc />
    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);

        if (HasLabel)
        {
            // Float label if text is not empty
            if (!IsTextEmpty && !IsLabelFloated)
            {
                AnimateLabelToFloated();
            }

            // Center label if text is empty and not focused and not keep floated
            else if (IsTextEmpty && IsLabelFloated && !IsKeyboardFocused && !KeepLabelFloated)
            {
                AnimateLabelToCenter();
            }
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateLabelVisibility();
        UpdateLabelState(false);
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LabelText control)
        {
            control.UpdateLabelVisibility();
        }
    }

    private static void OnKeepLabelFloatedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LabelText control)
        {
            var keepFloated = (bool)e.NewValue;

            if (keepFloated && !control.IsLabelFloated)
            {
                control.AnimateLabelToFloated();
            }
            else if (!keepFloated && control.IsLabelFloated && control.IsTextEmpty && !control.IsKeyboardFocused)
            {
                control.AnimateLabelToCenter();
            }
        }
    }

    private void UpdateLabelVisibility()
    {
        SetCurrentValue(HasLabelProperty, !string.IsNullOrEmpty(Label));
    }

    private void UpdateLabelState(bool useAnimation)
    {
        if (!HasLabel || _labelElement == null)
        {
            return;
        }

        var shouldFloat = !IsTextEmpty || IsKeyboardFocused || KeepLabelFloated;

        if (shouldFloat != IsLabelFloated)
        {
            if (useAnimation)
            {
                if (shouldFloat)
                {
                    AnimateLabelToFloated();
                }
                else
                {
                    AnimateLabelToCenter();
                }
            }
            else
            {
                ChangeLabelState(shouldFloat);
            }
        }
    }

    private void AnimateLabelToFloated()
    {
        if (_isAnimating || IsLabelFloated)
        {
            return;
        }

        AnimateLabel(true);
    }

    private void AnimateLabelToCenter()
    {
        if (_isAnimating || !IsLabelFloated)
        {
            return;
        }

        AnimateLabel(false);
    }

    private void AnimateLabel(bool toFloated)
    {
        if (_labelElement == null)
        {
            return;
        }

        // ✅ 기존 애니메이션 즉시 중단
        _currentStoryboard?.Stop();
        _currentStoryboard = null;

        // ✅ 상태 검증 생략하고 바로 시작
        _isAnimating = true;

        // ✅ Transform 미리 준비 (지연 없이)
        EnsureTransformGroup();

        // ✅ 바로 애니메이션 실행
        ExecuteAnimation(toFloated);
    }

    private void EnsureTransformGroup()
    {
        if (_labelElement is null)
        {
            return;
        }

        if (_labelElement.RenderTransform is not TransformGroup)
        {
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform());
            transformGroup.Children.Add(new SkewTransform());
            transformGroup.Children.Add(new RotateTransform());
            transformGroup.Children.Add(new TranslateTransform());
            _labelElement.SetCurrentValue(RenderTransformProperty, transformGroup);
        }
    }

    /// <summary>
    /// Telerik 방식 개선: From 값 명시 + FillBehavior.Stop으로 반복 트리거 문제 해결
    /// </summary>
    /// <summary>
    /// 기존 구조 유지하면서 From 값만 명시하여 반복 트리거 문제 해결
    /// </summary>
    private void ExecuteAnimation(bool toFloated)
    {
        if (_labelElement == null || !_isAnimating)
        {
            return;
        }

        var duration = TimeSpan.FromMilliseconds(ANIMATION_DURATION_MS);

        // TransformGroup 준비
        if (_labelElement.RenderTransform is not TransformGroup transformGroup)
        {
            transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform());
            transformGroup.Children.Add(new SkewTransform());
            transformGroup.Children.Add(new RotateTransform());
            transformGroup.Children.Add(new TranslateTransform());
            _labelElement.SetCurrentValue(RenderTransformProperty, transformGroup);
        }

        var scaleTransform = transformGroup.Children[0] as ScaleTransform;
        var translateTransform = transformGroup.Children[3] as TranslateTransform;

        // ✅ Scale 애니메이션 (직접 실행)
        if (scaleTransform != null)
        {
            var scaleAnimation = new DoubleAnimation
            {
                From = scaleTransform.ScaleX,
                To = toFloated ? EffectiveFloatedScale : 1.0,
                Duration = duration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation.Clone());
        }

        // ✅ Position 애니메이션 (직접 실행)
        if (translateTransform != null)
        {
            // Telerik 방식: 위치 계산
            double currentY = translateTransform.Y;
            double targetY;

            if (toFloated)
            {
                if (_rootElement != null)
                {
                    var rootHeight = _rootElement.ActualHeight;
                    var labelHeight = _labelElement.ActualHeight;
                    var marginTop = _labelElement.Margin.Top;
                    targetY = -((rootHeight / 2) - (labelHeight / 2) - marginTop);
                }
                else
                {
                    targetY = -18;
                }
            }
            else
            {
                targetY = 0;
            }

            var positionAnimation = new DoubleAnimation
            {
                From = currentY,
                To = targetY,
                Duration = duration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            // ✅ 마지막 애니메이션에만 Completed 이벤트
            positionAnimation.Completed += (s, e) =>
            {
                // 애니메이션 완료 처리 (기존과 동일)
                _labelElement.ClearValue(RenderTransformProperty);
                Grid.SetRowSpan(_labelElement, toFloated ? 1 : 2);
                _labelElement.SetCurrentValue(
                    VerticalAlignmentProperty,
                    toFloated ? VerticalAlignment.Bottom : VerticalAlignment.Center);

                var finalScale = toFloated ? EffectiveFloatedScale : 1.0;
                var finalTransformGroup = new TransformGroup();
                finalTransformGroup.Children.Add(new ScaleTransform(finalScale, finalScale));
                finalTransformGroup.Children.Add(new SkewTransform());
                finalTransformGroup.Children.Add(new RotateTransform());
                finalTransformGroup.Children.Add(new TranslateTransform(0, 0));
                _labelElement.SetCurrentValue(RenderTransformProperty, finalTransformGroup);

                SetCurrentValue(IsLabelFloatedProperty, toFloated);
                _isAnimating = false;
                _currentStoryboard = null;
            };

            translateTransform.BeginAnimation(TranslateTransform.YProperty, positionAnimation);
        }
    }

    private void ChangeLabelState(bool isFloated)
    {
        if (_labelElement == null)
        {
            return;
        }

        SetCurrentValue(IsLabelFloatedProperty, isFloated);

        // 🚀 Telerik 핵심: Transform 제거 + Grid 변경
        // _labelElement.ClearValue(RenderTransformProperty);

        // Grid RowSpan 변경 (XAML Trigger와 동일한 효과)
        Grid.SetRowSpan(_labelElement, isFloated ? 1 : 2);
        _labelElement.SetCurrentValue(
            VerticalAlignmentProperty,
            isFloated ? VerticalAlignment.Bottom : VerticalAlignment.Center);

        // 폰트 크기는 Scale Transform으로 최종 설정
        var targetScale = isFloated ? EffectiveFloatedScale : 1.0;
        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(new ScaleTransform(targetScale, targetScale));
        transformGroup.Children.Add(new SkewTransform());
        transformGroup.Children.Add(new RotateTransform());
        transformGroup.Children.Add(new TranslateTransform(0, 0)); // 위치는 초기화

        _labelElement.SetCurrentValue(RenderTransformProperty, transformGroup);

        _isAnimating = false;
    }

    private static void OnFloatedFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // No special handling needed - EffectiveFloatedScale will handle the logic
    }

    /// <summary>
    /// Telerik 방식: 초기 Transform 설정
    /// </summary>
    private void SetupInitialTransform()
    {
        if (_labelElement == null)
        {
            return;
        }

        // 기본 TransformGroup 설정 (Telerik 방식)
        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(new ScaleTransform(1.0, 1.0));      // 기본 스케일
        transformGroup.Children.Add(new SkewTransform());
        transformGroup.Children.Add(new RotateTransform());
        transformGroup.Children.Add(new TranslateTransform(0, 0));      // 기본 위치

        _labelElement.SetCurrentValue(RenderTransformProperty, transformGroup);

        // 초기 상태 설정 (애니메이션 없이)
        UpdateLabelState(false);

        // Debug.WriteLine("LabelText: Initial Transform setup completed");
    }
}