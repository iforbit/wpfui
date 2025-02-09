// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using Wpf.Ui.Controls.Automation.Peers;
using Wpf.Ui.Controls.Helpers;
using Wpf.Ui.Extensions;
using Wpf.Ui.Internal;
using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents ribbon tab item
/// </summary>
[TemplatePart(Name = "PART_HeaderContentHost", Type = typeof(FrameworkElement))]
[TemplatePart(Name = "PART_ContentContainer", Type = typeof(Border))]
[ContentProperty(nameof(Groups))]
[DefaultProperty(nameof(Groups))]
[DebuggerDisplay("{GetType().FullName}: Header = {Header}, Groups.Count = {Groups.Count}, IsSimplified = {IsSimplified}")]

public class RibbonTabItem : Control, IHeaderedControl, ILogicalChildSupport, ISimplifiedStateControl
{
    // Ribbon groups container
    private readonly RibbonGroupsContainer groupsInnerContainer = new();

    // Content container
    private Border? contentContainer;

    // Collection of ribbon groups
    private ObservableCollection<RibbonGroupBox>? groups;

    // Cached width
    private double cachedWidth;

    internal FrameworkElement? HeaderContentHost { get; private set; }

    /// <summary>
    /// Gets or sets the <see cref="Brush"/> which is used to render the background if this <see cref="RibbonTabItem"/> is the currently active/selected one.
    /// </summary>
    public Brush? ActiveTabBackground
    {
        get => (Brush?)GetValue(ActiveTabBackgroundProperty);
        set => SetValue(ActiveTabBackgroundProperty, value);
    }

    /// <summary>Identifies the <see cref="ActiveTabBackground"/> dependency property.</summary>
    public static readonly DependencyProperty ActiveTabBackgroundProperty =
        DependencyProperty.Register(nameof(ActiveTabBackground), typeof(Brush), typeof(RibbonTabItem), new PropertyMetadata());

    /// <summary>
    /// Gets or sets the <see cref="Brush"/> which is used to render the border if this <see cref="RibbonTabItem"/> is the currently active/selected one.
    /// </summary>
    public Brush? ActiveTabBorderBrush
    {
        get => (Brush?)GetValue(ActiveTabBorderBrushProperty);
        set => SetValue(ActiveTabBorderBrushProperty, value);
    }

    /// <summary>Identifies the <see cref="ActiveTabBorderBrush"/> dependency property.</summary>
    public static readonly DependencyProperty ActiveTabBorderBrushProperty =
        DependencyProperty.Register(nameof(ActiveTabBorderBrush), typeof(Brush), typeof(RibbonTabItem), new PropertyMetadata());

    /// <summary>
    /// Gets ribbon groups container
    /// </summary>
    public ScrollViewer GroupsContainer { get; }

    /// <summary>
    /// Gets or sets reduce order
    /// </summary>
    public string? ReduceOrder
    {
        get => groupsInnerContainer.ReduceOrder;
        set => groupsInnerContainer.SetValue(RibbonGroupsContainer.ReduceOrderProperty, value);
    }

    /// <summary>
    /// Gets a value indicating whether gets or sets whether tab item is contextual
    /// </summary>
    public bool IsContextual
    {
        get => (bool)GetValue(IsContextualProperty);
        private set => SetValue(IsContextualPropertyKey, BooleanBoxes.Box(value));
    }

    private static readonly DependencyPropertyKey IsContextualPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsContextual), typeof(bool), typeof(RibbonTabItem), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>Identifies the <see cref="IsContextual"/> dependency property.</summary>
    public static readonly DependencyProperty IsContextualProperty = IsContextualPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether tab item is selected
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);

        set => SetValue(IsSelectedProperty, BooleanBoxes.Box(value));
    }

    /// <summary>
    /// Using a DependencyProperty as the backing store for IsSelected.
    /// This enables animation, styling, binding, etc...
    /// </summary>
    public static readonly DependencyProperty IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(typeof(RibbonTabItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSelectedChanged));

    /// <summary>
    /// Gets ribbon tab control parent
    /// </summary>
    internal RibbonTabControl? TabControlParent => UIHelper.GetParent<RibbonTabControl>(this);

    /// <summary>
    /// Gets or sets the padding for the header.
    /// </summary>
    public Thickness HeaderPadding
    {
        get => (Thickness)GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }

    /// <summary>Identifies the <see cref="HeaderPadding"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderPaddingProperty = DependencyProperty.Register(nameof(HeaderPadding), typeof(Thickness), typeof(RibbonTabItem), new FrameworkPropertyMetadata(new Thickness(9, 3, 9, 6), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>Identifies the <see cref="SeparatorOpacity"/> dependency property.</summary>
    public static readonly DependencyProperty SeparatorOpacityProperty = DependencyProperty.Register(nameof(SeparatorOpacity), typeof(double), typeof(RibbonTabItem), new PropertyMetadata(DoubleBoxes.Zero));

    /// <summary>
    /// Gets or sets the opacity of the separator.
    /// </summary>
    public double SeparatorOpacity
    {
        get => (double)GetValue(SeparatorOpacityProperty);
        set => SetValue(SeparatorOpacityProperty, value);
    }

    /// <summary>
    /// Gets or sets ribbon contextual tab group
    /// </summary>
    public RibbonContextualTabGroup? Group
    {
        get => (RibbonContextualTabGroup?)GetValue(GroupProperty);
        set => SetValue(GroupProperty, value);
    }

    /// <summary>Identifies the <see cref="Group"/> dependency property.</summary>
    public static readonly DependencyProperty GroupProperty =
        DependencyProperty.Register(nameof(Group), typeof(RibbonContextualTabGroup), typeof(RibbonTabItem), new PropertyMetadata(OnGroupChanged));

    // handles Group property chanhged
    private static void OnGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var tab = (RibbonTabItem)d;

        ((RibbonContextualTabGroup?)e.OldValue)?.RemoveTabItem(tab);

        if (e.NewValue is RibbonContextualTabGroup tabGroup)
        {
            tabGroup.AppendTabItem(tab);
            tab.IsContextual = true;
        }
        else
        {
            tab.IsContextual = false;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether tab item has left group border
    /// </summary>
    public bool HasLeftGroupBorder
    {
        get => (bool)GetValue(HasLeftGroupBorderProperty);
        set => SetValue(HasLeftGroupBorderProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="HasLeftGroupBorder"/> dependency property.</summary>
    public static readonly DependencyProperty HasLeftGroupBorderProperty =
        DependencyProperty.Register(nameof(HasLeftGroupBorder), typeof(bool), typeof(RibbonTabItem), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether tab item has right group border
    /// </summary>
    public bool HasRightGroupBorder
    {
        get => (bool)GetValue(HasRightGroupBorderProperty);
        set => SetValue(HasRightGroupBorderProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="HasRightGroupBorder"/> dependency property.</summary>
    public static readonly DependencyProperty HasRightGroupBorderProperty =
        DependencyProperty.Register(nameof(HasRightGroupBorder), typeof(bool), typeof(RibbonTabItem), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Gets get collection of ribbon groups
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<RibbonGroupBox> Groups
    {
        get
        {
            if (groups is null)
            {
                groups = new ObservableCollection<RibbonGroupBox>();
                groups.CollectionChanged += OnGroupsCollectionChanged;
            }

            return groups;
        }
    }

    // handles ribbon groups collection changes
    private void OnGroupsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                var isSimplified = IsSimplified;
                for (var i = 0; i < e.NewItems?.Count; i++)
                {
                    var element = (UIElement?)e.NewItems![i];

                    if (element is not null)
                    {
                        groupsInnerContainer.Children.Insert(e.NewStartingIndex + i, element);
                    }

                    if (element is ISimplifiedStateControl control)
                    {
                        control.UpdateSimplifiedState(isSimplified);
                    }
                }
            }

            break;

            case NotifyCollectionChangedAction.Remove:
            {
                foreach (UIElement item in e.OldItems.NullSafe().OfType<UIElement>())
                {
                    groupsInnerContainer.Children.Remove(item);
                }
            }

            break;

            case NotifyCollectionChangedAction.Replace:
            {
                foreach (UIElement item in e.OldItems.NullSafe().OfType<UIElement>())
                {
                    groupsInnerContainer.Children.Remove(item);
                }

                {
                    var isSimplified = IsSimplified;
                    foreach (UIElement item in e.NewItems.NullSafe().OfType<UIElement>())
                    {
                        _ = groupsInnerContainer.Children.Add(item);

                        if (item is ISimplifiedStateControl control)
                        {
                            control.UpdateSimplifiedState(isSimplified);
                        }
                    }
                }
            }

            break;

            case NotifyCollectionChangedAction.Reset:
            {
                groupsInnerContainer.Children.Clear();
                {
                    var isSimplified = IsSimplified;
                    foreach (RibbonGroupBox group in Groups)
                    {
                        _ = groupsInnerContainer.Children.Add(group);

                        if (group is ISimplifiedStateControl control)
                        {
                            control.UpdateSimplifiedState(isSimplified);
                        }
                    }
                }
            }

            break;
        }
    }

    /// <inheritdoc />
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>Identifies the <see cref="Header"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderProperty = RibbonControl.HeaderProperty.AddOwner(typeof(RibbonTabItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <inheritdoc />
    public DataTemplate? HeaderTemplate
    {
        get => (DataTemplate?)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>Identifies the <see cref="HeaderTemplate"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderTemplateProperty = RibbonControl.HeaderTemplateProperty.AddOwner(typeof(RibbonTabItem), new PropertyMetadata());

    /// <inheritdoc />
    public DataTemplateSelector? HeaderTemplateSelector
    {
        get => (DataTemplateSelector?)GetValue(HeaderTemplateSelectorProperty);
        set => SetValue(HeaderTemplateSelectorProperty, value);
    }

    /// <summary>Identifies the <see cref="HeaderTemplateSelector"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderTemplateSelectorProperty = RibbonControl.HeaderTemplateSelectorProperty.AddOwner(typeof(RibbonTabItem), new PropertyMetadata());

    /// <summary>
    /// Handles Focusable changes
    /// </summary>
    private static void OnFocusableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    /// <summary>
    /// Coerces Focusable
    /// </summary>
    private static object? CoerceFocusable(DependencyObject d, object? basevalue)
    {
        var control = d as RibbonTabItem;
        Ribbon? ribbon = control?.FindParentRibbon();

        if (ribbon is not null
            && basevalue is bool boolValue)
        {
            return BooleanBoxes.Box(boolValue && ribbon.Focusable);
        }

        return basevalue;
    }

    // Find parent ribbon
    private Ribbon? FindParentRibbon()
    {
        DependencyObject? element = Parent;
        while (element is not null)
        {
            if (element is Ribbon ribbon)
            {
                return ribbon;
            }

            element = VisualTreeHelper.GetParent(element);
        }

        return null;
    }

    /// <summary>
    /// Gets a value indicating whether gets or sets whether or not the ribbon is in Simplified mode
    /// </summary>
    public bool IsSimplified
    {
        get => (bool)GetValue(IsSimplifiedProperty);
        private set => SetValue(IsSimplifiedPropertyKey, BooleanBoxes.Box(value));
    }

    private static readonly DependencyPropertyKey IsSimplifiedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsSimplified), typeof(bool), typeof(RibbonTabItem), new PropertyMetadata(BooleanBoxes.FalseBox, OnIsSimplifiedChanged));

    /// <summary>Identifies the <see cref="IsSimplified"/> dependency property.</summary>
    public static readonly DependencyProperty IsSimplifiedProperty = IsSimplifiedPropertyKey.DependencyProperty;

    private static void OnIsSimplifiedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RibbonTabItem ribbonTabItem)
        {
            var isSimplified = (bool)e.NewValue;
            foreach (ISimplifiedStateControl item in ribbonTabItem.Groups.OfType<ISimplifiedStateControl>())
            {
                item.UpdateSimplifiedState(isSimplified);
            }
        }
    }

    /// <summary>
    /// Initializes static members of the <see cref="RibbonTabItem"/> class.
    /// Static constructor
    /// </summary>
    static RibbonTabItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonTabItem), new FrameworkPropertyMetadata(typeof(RibbonTabItem)));
        _ = FocusableProperty.AddOwner(typeof(RibbonTabItem), new FrameworkPropertyMetadata(OnFocusableChanged, CoerceFocusable));
        _ = VisibilityProperty.AddOwner(typeof(RibbonTabItem), new FrameworkPropertyMetadata(OnVisibilityChanged));

        KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(RibbonTabItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
        KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(RibbonTabItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));

        AutomationProperties.IsOffscreenBehaviorProperty.OverrideMetadata(typeof(RibbonTabItem), new FrameworkPropertyMetadata(IsOffscreenBehavior.FromClip));
    }

    // Handles visibility changes
    private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var item = d as RibbonTabItem;

        if (item is null)
        {
            return;
        }

        item.Group?.UpdateInnerVisiblityAndGroupBorders();

        if (item.IsSelected
            && (Visibility)e.NewValue == Visibility.Collapsed)
        {
            if (item.TabControlParent is not null)
            {
                if (item.TabControlParent.IsMinimized)
                {
                    item.IsSelected = false;
                }
                else
                {
                    item.TabControlParent.SetCurrentValue(Selector.SelectedItemProperty, item.TabControlParent.GetFirstVisibleAndEnabledItem());
                }
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonTabItem"/> class.
    /// Default constructor
    /// </summary>
    public RibbonTabItem()
    {
        GroupsContainer = new RibbonGroupsContainerScrollViewer();
        AddLogicalChild(GroupsContainer);
        GroupsContainer.Content = groupsInnerContainer;

        // Force redirection of DataContext. This is needed, because we detach the container from the visual tree and attach it to a diffrent one (the popup/dropdown) when the ribbon is minimized.
        _ = groupsInnerContainer.SetBinding(DataContextProperty, new Binding(nameof(DataContext))
        {
            Source = this
        });

        _ = groupsInnerContainer.SetBinding(MarginProperty, new Binding(nameof(Padding))
        {
            Source = this
        });

        // ContextMenuService.Coerce(this);
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size constraint)
    {
        if (contentContainer is null)
        {
            return base.MeasureOverride(constraint);
        }

        if (IsContextual
            && Group?.Visibility == Visibility.Collapsed)
        {
            return Size.Empty;
        }

        Size baseConstraint = base.MeasureOverride(constraint);

        if (DoubleUtil.AreClose(cachedWidth, baseConstraint.Width) == false
            && IsContextual
            && Group is not null)
        {
            cachedWidth = baseConstraint.Width;

            RibbonContextualGroupsContainer? contextualTabGroupContainer = UIHelper.GetParent<RibbonContextualGroupsContainer>(Group);
            contextualTabGroupContainer?.InvalidateMeasure();

            RibbonTitleBar? ribbonTitleBar = UIHelper.GetParent<RibbonTitleBar>(Group);
            ribbonTitleBar?.ScheduleForceMeasureAndArrange();
        }

        return baseConstraint;
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        Size result = base.ArrangeOverride(arrangeBounds);

        RibbonTitleBar? ribbonTitleBar = UIHelper.GetParent<RibbonTitleBar>(Group);
        ribbonTitleBar?.ScheduleForceMeasureAndArrange();

        return result;
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        HeaderContentHost = GetTemplateChild("PART_HeaderContentHost") as FrameworkElement;

        contentContainer = GetTemplateChild("PART_ContentContainer") as Border;
    }

    /// <inheritdoc />
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (ReferenceEquals(e.Source, this)
            && e.ClickCount == 2)
        {
            e.Handled = true;

            if (TabControlParent is not null)
            {
                var canMinimize = TabControlParent.CanMinimize;
                if (canMinimize)
                {
                    TabControlParent.SetCurrentValue(RibbonTabControl.IsMinimizedProperty, !TabControlParent.IsMinimized);
                }
            }
        }
        else if (ReferenceEquals(e.Source, this)
                 || IsSelected == false)
        {
            if (Visibility == Visibility.Visible)
            {
                if (TabControlParent is not null)
                {
                    var newItem = TabControlParent.ItemContainerGenerator.ItemFromContainerOrContainerContent(this);

                    if (ReferenceEquals(TabControlParent.SelectedItem, newItem))
                    {
                        TabControlParent.SetCurrentValue(RibbonTabControl.IsDropDownOpenProperty, !TabControlParent.IsDropDownOpen);
                    }
                    else
                    {
                        TabControlParent.SetCurrentValue(Selector.SelectedItemProperty, newItem);
                        TabControlParent.SetCurrentValue(RibbonTabControl.IsDropDownOpenProperty, true);
                    }

                    TabControlParent.RaiseRequestBackstageClose();
                }
                else
                {
                    SetCurrentValue(IsSelectedProperty, true);
                }

                e.Handled = true;
            }
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
            case Key.Space:
                if (TabControlParent is not null
                    && TabControlParent.IsMinimized)
                {
                    TabControlParent.SetCurrentValue(RibbonTabControl.IsDropDownOpenProperty, true);

                    e.Handled = true;
                }

                break;
        }

        base.OnKeyDown(e);
    }

    /// <inheritdoc />
    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnGotKeyboardFocus(e);

        SetCurrentValue(IsSelectedProperty, BooleanBoxes.TrueBox);
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer() => new RibbonTabItemAutomationPeer(this);

    // Handles IsSelected property changes
    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var container = (RibbonTabItem)d;
        var newValue = (bool)e.NewValue;

        if (newValue)
        {
            container.OnSelected(new RoutedEventArgs(Selector.SelectedEvent, container));
            container.BringIntoView();
        }
        else
        {
            container.OnUnselected(new RoutedEventArgs(Selector.UnselectedEvent, container));
        }

        // Raise UI automation events on this RibbonTabItem
        if (AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected)
            || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection))
        {
            // SelectorHelper.RaiseIsSelectedChangedAutomationEvent(container.TabControlParent, container, newValue);
            var peer = UIElementAutomationPeer.CreatePeerForElement(container) as RibbonTabItemAutomationPeer;
            peer?.RaiseTabSelectionEvents();
        }
    }

    /// <summary>
    /// Handles selected
    /// </summary>
    /// <param name="e">The event data</param>
    protected virtual void OnSelected(RoutedEventArgs e)
    {
        HandleIsSelectedChanged(e);
    }

    /// <summary>
    /// handles unselected
    /// </summary>
    /// <param name="e">The event data</param>
    protected virtual void OnUnselected(RoutedEventArgs e)
    {
        HandleIsSelectedChanged(e);
    }

    // Handles IsSelected property changes
    private void HandleIsSelectedChanged(RoutedEventArgs e)
    {
        RaiseEvent(e);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        SubscribeEvents();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        UnSubscribeEvents();
    }

    private void SubscribeEvents()
    {
        // Always unsubscribe events to ensure we don't subscribe twice
        UnSubscribeEvents();

        if (groups is not null)
        {
            groups.CollectionChanged += OnGroupsCollectionChanged;
        }
    }

    private void UnSubscribeEvents()
    {
        if (groups is not null)
        {
            groups.CollectionChanged -= OnGroupsCollectionChanged;
        }
    }

    /// <inheritdoc />
    public void UpdateSimplifiedState(bool isSimplified)
    {
        IsSimplified = isSimplified;
    }

    /// <inheritdoc />
    void ILogicalChildSupport.AddLogicalChild(object child)
    {
        AddLogicalChild(child);
    }

    /// <inheritdoc />
    void ILogicalChildSupport.RemoveLogicalChild(object child)
    {
        RemoveLogicalChild(child);
    }

    /// <inheritdoc />
    protected override IEnumerator LogicalChildren
    {
        get
        {
            System.Collections.IEnumerator baseEnumerator = base.LogicalChildren;
            while (baseEnumerator?.MoveNext() == true)
            {
                yield return baseEnumerator.Current;
            }

            yield return GroupsContainer;

            if (Header is not null)
            {
                yield return Header;
            }
        }
    }
}