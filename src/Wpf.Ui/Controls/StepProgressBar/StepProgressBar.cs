// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Wpf.Ui.Input;

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// Displays a sequence of steps indicating progress through a multi-step process,
/// optionally showing per-step content via <see cref="StepContent"/>.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;ui:StepProgressBar SelectedIndex="{Binding CurrentStep}"&gt;
///     &lt;ui:StepProgressBar.StepContent&gt;
///         &lt;local:PageA /&gt;
///         &lt;local:PageB /&gt;
///     &lt;/ui:StepProgressBar.StepContent&gt;
///     &lt;ui:StepProgressBarItem Content="Step A" /&gt;
///     &lt;ui:StepProgressBarItem Content="Step B" /&gt;
/// &lt;/ui:StepProgressBar&gt;
/// </code>
/// </example>
[StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(StepProgressBarItem))]
public class StepProgressBar : System.Windows.Controls.ItemsControl
{
    /// <summary>Identifies the <see cref="SelectedIndex"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
        nameof(SelectedIndex),
        typeof(int),
        typeof(StepProgressBar),
        new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedIndexChanged)
    );

    /// <summary>Identifies the <see cref="Orientation"/> dependency property.</summary>
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        nameof(Orientation),
        typeof(Orientation),
        typeof(StepProgressBar),
        new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged)
    );

    /// <summary>Identifies the <see cref="StepSpacing"/> dependency property.</summary>
    public static readonly DependencyProperty StepSpacingProperty = DependencyProperty.Register(
        nameof(StepSpacing),
        typeof(double),
        typeof(StepProgressBar),
        new PropertyMetadata(24.0)
    );

    /// <summary>Identifies the <see cref="TrackThickness"/> dependency property.</summary>
    public static readonly DependencyProperty TrackThicknessProperty = DependencyProperty.Register(
        nameof(TrackThickness),
        typeof(double),
        typeof(StepProgressBar),
        new PropertyMetadata(2.0)
    );

    /// <summary>Identifies the <see cref="IndicatorSize"/> dependency property.</summary>
    public static readonly DependencyProperty IndicatorSizeProperty = DependencyProperty.Register(
        nameof(IndicatorSize),
        typeof(double),
        typeof(StepProgressBar),
        new PropertyMetadata(28.0)
    );

    /// <summary>Identifies the <see cref="CanUserSelect"/> dependency property.</summary>
    public static readonly DependencyProperty CanUserSelectProperty = DependencyProperty.Register(
        nameof(CanUserSelect),
        typeof(bool),
        typeof(StepProgressBar),
        new PropertyMetadata(true)
    );

    /// <summary>Identifies the <see cref="ContentMargin"/> dependency property.</summary>
    public static readonly DependencyProperty ContentMarginProperty = DependencyProperty.Register(
        nameof(ContentMargin),
        typeof(Thickness),
        typeof(StepProgressBar),
        new PropertyMetadata(new Thickness(0, 16, 0, 0))
    );

    /// <summary>Identifies the <see cref="CurrentStepContent"/> dependency property.</summary>
    public static readonly DependencyProperty CurrentStepContentProperty = DependencyProperty.Register(
        nameof(CurrentStepContent),
        typeof(UIElement),
        typeof(StepProgressBar),
        new PropertyMetadata(null)
    );

    /// <summary>Identifies the <see cref="TemplateButtonCommand"/> dependency property.</summary>
    public static readonly DependencyProperty TemplateButtonCommandProperty = DependencyProperty.Register(
        nameof(TemplateButtonCommand),
        typeof(IRelayCommand),
        typeof(StepProgressBar),
        new PropertyMetadata(null)
    );

    /// <summary>Identifies the <see cref="StepClicked"/> routed event.</summary>
    public static readonly RoutedEvent StepClickedEvent = EventManager.RegisterRoutedEvent(
        nameof(StepClicked),
        RoutingStrategy.Bubble,
        typeof(TypedEventHandler<StepProgressBar, StepProgressBarItemClickedEventArgs>),
        typeof(StepProgressBar)
    );

    // ── Backing collection for StepContent ───────────────────────────
    private readonly ObservableCollection<UIElement> _stepContent = [];

    /// <summary>
    /// Gets the per-step content elements displayed below the step indicators.
    /// The element at index <c>i</c> is shown when <see cref="SelectedIndex"/> equals <c>i</c>.
    /// </summary>
    public ObservableCollection<UIElement> StepContent => _stepContent;

    // ── Public properties ─────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the index of the currently active step.
    /// </summary>
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>
    /// Gets or sets the layout direction of the steps.
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum spacing (in pixels) between step indicators.
    /// </summary>
    public double StepSpacing
    {
        get => (double)GetValue(StepSpacingProperty);
        set => SetValue(StepSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the thickness of the connecting track line between steps.
    /// </summary>
    public double TrackThickness
    {
        get => (double)GetValue(TrackThicknessProperty);
        set => SetValue(TrackThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the diameter of the step indicator circles.
    /// </summary>
    public double IndicatorSize
    {
        get => (double)GetValue(IndicatorSizeProperty);
        set => SetValue(IndicatorSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether clicking a step indicator changes <see cref="SelectedIndex"/>.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool CanUserSelect
    {
        get => (bool)GetValue(CanUserSelectProperty);
        set => SetValue(CanUserSelectProperty, value);
    }

    /// <summary>
    /// Gets or sets the margin between the step indicators and the content area.
    /// </summary>
    public Thickness ContentMargin
    {
        get => (Thickness)GetValue(ContentMarginProperty);
        set => SetValue(ContentMarginProperty, value);
    }

    /// <summary>
    /// Gets the <see cref="UIElement"/> from <see cref="StepContent"/> that corresponds to the current
    /// <see cref="SelectedIndex"/>. Bound by the default control template.
    /// </summary>
    public UIElement? CurrentStepContent
    {
        get => (UIElement?)GetValue(CurrentStepContentProperty);
        private set => SetValue(CurrentStepContentProperty, value);
    }

    /// <summary>
    /// Gets the command used by item buttons inside the control template.
    /// </summary>
    public IRelayCommand TemplateButtonCommand => (IRelayCommand)GetValue(TemplateButtonCommandProperty);

    /// <summary>
    /// Occurs when a step indicator is clicked by the user.
    /// </summary>
    public event TypedEventHandler<StepProgressBar, StepProgressBarItemClickedEventArgs> StepClicked
    {
        add => AddHandler(StepClickedEvent, value);
        remove => RemoveHandler(StepClickedEvent, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StepProgressBar"/> class.
    /// </summary>
    public StepProgressBar()
    {
        SetValue(TemplateButtonCommandProperty, new RelayCommand<object>(OnTemplateButtonClick));

        _stepContent.CollectionChanged += OnStepContentChanged;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    // ── ItemsControl overrides ────────────────────────────────────────

    /// <inheritdoc/>
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StepProgressBarItem;

    /// <inheritdoc/>
    protected override DependencyObject GetContainerForItemOverride() => new StepProgressBarItem();

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is StepProgressBarItem stepItem)
        {
            int index = ItemContainerGenerator.IndexFromContainer(element);
            stepItem.SetCurrentValue(StepProgressBarItem.IndexProperty, index);
            stepItem.SetCurrentValue(StepProgressBarItem.OrientationProperty, Orientation);
            UpdateItemStatus(stepItem, index, SelectedIndex);
        }
    }

    /// <summary>
    /// Raises <see cref="StepClicked"/> and updates <see cref="SelectedIndex"/> when
    /// <see cref="CanUserSelect"/> is <see langword="true"/>.
    /// </summary>
    protected virtual void OnStepClicked(object item, int index)
    {
        var args = new StepProgressBarItemClickedEventArgs(StepClickedEvent, this, item, index);
        RaiseEvent(args);

        if (CanUserSelect)
        {
            SetCurrentValue(SelectedIndexProperty, index);
        }
    }

    // ── Private helpers ───────────────────────────────────────────────

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
        ItemContainerGenerator.ItemsChanged += OnGeneratorItemsChanged;

        RefreshAllItemStatuses();
        UpdateCurrentStepContent();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;

        ItemContainerGenerator.StatusChanged -= OnGeneratorStatusChanged;
        ItemContainerGenerator.ItemsChanged -= OnGeneratorItemsChanged;
    }

    private void OnGeneratorStatusChanged(object? sender, EventArgs e)
    {
        if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
        {
            RefreshAllItemStatuses();
        }
    }

    private void OnGeneratorItemsChanged(object sender, ItemsChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset)
        {
            RefreshAllItemStatuses();
        }
    }

    private void OnStepContentChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateCurrentStepContent();
    }

    private void RefreshAllItemStatuses()
    {
        int selectedIndex = SelectedIndex;

        for (int i = 0; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is StepProgressBarItem item)
            {
                item.SetCurrentValue(StepProgressBarItem.IndexProperty, i);
                UpdateItemStatus(item, i, selectedIndex);
            }
        }

        InvalidatePanelVisual();
    }

    private void UpdateCurrentStepContent()
    {
        int idx = SelectedIndex;
        CurrentStepContent = (idx >= 0 && idx < _stepContent.Count) ? _stepContent[idx] : null;
    }

    private void InvalidatePanelVisual()
    {
        if (FindStepProgressBarPanel(this) is StepProgressBarPanel panel)
        {
            panel.InvalidateVisual();
        }
    }

    private static StepProgressBarPanel? FindStepProgressBarPanel(DependencyObject parent)
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);
            if (child is StepProgressBarPanel panel)
            {
                return panel;
            }

            StepProgressBarPanel? found = FindStepProgressBarPanel(child);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    private static void UpdateItemStatus(StepProgressBarItem item, int itemIndex, int selectedIndex)
    {
        StepProgressBarItemStatus status = itemIndex < selectedIndex
            ? StepProgressBarItemStatus.Completed
            : itemIndex == selectedIndex
                ? StepProgressBarItemStatus.Active
                : StepProgressBarItemStatus.NotStarted;

        item.SetCurrentValue(StepProgressBarItem.StatusProperty, status);
    }

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StepProgressBar bar)
        {
            bar.RefreshAllItemStatuses();
            bar.UpdateCurrentStepContent();
        }
    }

    private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StepProgressBar bar)
        {
            var orientation = (Orientation)e.NewValue;
            for (int i = 0; i < bar.Items.Count; i++)
            {
                if (bar.ItemContainerGenerator.ContainerFromIndex(i) is StepProgressBarItem item)
                {
                    item.SetCurrentValue(StepProgressBarItem.OrientationProperty, orientation);
                }
            }

            bar.InvalidatePanelVisual();
        }
    }

    private void OnTemplateButtonClick(object? obj)
    {
        // CommandParameter is bound to StepProgressBarItem.Index (int)
        if (obj is not int index || index < 0 || index >= Items.Count)
        {
            return;
        }

        object item = Items[index];
        OnStepClicked(item, index);
    }
}
