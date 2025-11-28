// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls;

[ContentProperty(nameof(Tabs))]
[DefaultProperty(nameof(Tabs))]
[TemplatePart(Name = "PART_LayoutRoot", Type = typeof(Panel))]
[TemplatePart(Name = "PART_RibbonTabControl", Type = typeof(RibbonTabControl))]

public class Ribbon : Control, ILogicalChildSupport
{
    private IRibbonStateStorage? ribbonStateStorage;

    /// <summary>
    /// Gets the current instance for storing the state of this control.
    /// </summary>
    public IRibbonStateStorage RibbonStateStorage => this.ribbonStateStorage ??= this.CreateRibbonStateStorage();

    /// <summary>
    /// Create a new instance for storing the state of this control.
    /// </summary>
    /// <returns>Instance of a state storage class.</returns>
    protected virtual IRibbonStateStorage CreateRibbonStateStorage()
    {
        return new RibbonStateStorage(this);
    }

    /// <summary>
    /// Minimal width of ribbon parent window
    /// </summary>
    public const double MinimalVisibleWidth = 300;

    /// <summary>
    /// Minimal height of ribbon parent window
    /// </summary>
    public const double MinimalVisibleHeight = 250;

    /// <summary>
    /// gets collection of ribbon tabs
    /// </summary>
    private ObservableCollection<RibbonTabItem>? tabs;

    /// <summary>
    /// gets collection of ribbon tabs
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<RibbonTabItem> Tabs
    {
        get
        {
            if (this.tabs is null)
            {
                this.tabs = new ObservableCollection<RibbonTabItem>();
                this.tabs.CollectionChanged += this.OnTabItemsCollectionChanged;
            }

            return this.tabs;
        }
    }

    /// <summary>
    /// Gets or sets the source of items used to generate the content of the Ribbon.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set
        {
            if (value is null)
            {
                ClearValue(ItemsSourceProperty);
            }
            else
            {
                SetValue(ItemsSourceProperty, value);
            }
        }
    }

    /// <summary>
    /// ItemsSource 속성을 추가하여 RibbonTabItem 리스트를 바인딩할 수 있도록 합니다.
    /// </summary>
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(Ribbon),
            new PropertyMetadata(null, OnItemsSourceChanged));

    private static void OnItemsSourceChanged(DependencyObject? d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Ribbon ribbon)
        {
            return;
        }

        // 기존 탭 초기화
        ribbon.Tabs.Clear();

        // 새 ItemsSource가 IEnumerable일 경우, RibbonTabItem만 필터링하여 추가
        if (e.NewValue is IEnumerable newItems)
        {
            foreach (var item in newItems)
            {
                if (item is RibbonTabItem tabItem)
                {
                    ribbon.Tabs.Add(tabItem);
                }
            }

            // 만약 ItemsSource가 INotifyCollectionChanged를 구현한다면,
            // CollectionChanged 이벤트를 구독하여 동적으로 변경 내용을 반영하도록 할 수 있습니다.
            if (e.NewValue is INotifyCollectionChanged notifyCollection)
            {
                // 이벤트 구독 예제 (구독 해제 로직도 필요)
                notifyCollection.CollectionChanged += ribbon.OnItemsSourceCollectionChanged;
            }
        }
    }

    /// <summary>
    /// Handles CollectionChanged events raised from ItemsSource.
    /// </summary>
    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // 간단한 예시로 Reset 시 전체 새로고침, Add 시 개별 추가 등으로 처리합니다.
        // 실제 구현 시 추가/제거, 교체, 이동 등의 경우를 세분화해서 처리할 수 있습니다.
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            this.Tabs.Clear();
            if (sender is IEnumerable newItems)
            {
                foreach (RibbonTabItem item in newItems.OfType<RibbonTabItem>())
                {
                    this.Tabs.Add(item);
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            foreach (RibbonTabItem item in e.NewItems.OfType<RibbonTabItem>())
            {
                this.Tabs.Add(item);
            }
        }

        // 필요한 경우 Remove, Replace, Move에 대한 처리 추가 가능
    }

    /// <summary>
    /// Handles changes in the Tabs collection.
    /// </summary>
    private void OnTabItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        bool isSimplified = this.IsSimplified;
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        if (item is ISimplifiedStateControl simplifiedControl)
                        {
                            simplifiedControl.UpdateSimplifiedState(isSimplified);
                        }
                    }
                }

                break;
            case NotifyCollectionChangedAction.Reset:
                foreach (RibbonTabItem item in this.Tabs)
                {
                    if (item is ISimplifiedStateControl simplifiedControl)
                    {
                        simplifiedControl.UpdateSimplifiedState(isSimplified);
                    }
                }

                break;

            // 필요에 따라 Remove, Move 액션에 대한 처리 추가 가능
            default:
                break;
        }
    }

    private CollectionSyncHelper<RibbonTabItem>? tabsSync;

    /// <summary>
    /// Occurs when selected tab has been changed (be aware that SelectedTab can be null)
    /// </summary>
    public event SelectionChangedEventHandler? SelectedTabChanged;

    /// <summary>
    /// Occurs when IsMinimized property is changing
    /// </summary>
    public event DependencyPropertyChangedEventHandler? IsMinimizedChanged;

    /// <summary>
    /// Occurs when IsCollapsed property is changing
    /// </summary>
    public event DependencyPropertyChangedEventHandler? IsCollapsedChanged;

    // Ribbon layout root
    private Panel? layoutRoot;

    private Window? ownerWindow;

    /// <summary>
    /// Gets property for defining the TabControl.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public RibbonTabControl? TabControl
    {
        get => (RibbonTabControl?)this.GetValue(TabControlProperty);
        private set => this.SetValue(TabControlPropertyKey, value);
    }

    // ReSharper disable once InconsistentNaming
    private static readonly DependencyPropertyKey TabControlPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(TabControl), typeof(RibbonTabControl), typeof(Ribbon), new FrameworkPropertyMetadata(default(RibbonTabControl), FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure, LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <summary>Identifies the <see cref="TabControl"/> dependency property.</summary>
    public static readonly DependencyProperty TabControlProperty = TabControlPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets or sets the application menu (Backstage).
    /// </summary>
    public Backstage? Menu
    {
        get => (Backstage?)this.GetValue(MenuProperty);
        set => this.SetValue(MenuProperty, value);
    }

    /// <summary>Identifies the <see cref="Menu"/> dependency property.</summary>
    public static readonly DependencyProperty MenuProperty =
        DependencyProperty.Register(nameof(Menu), typeof(Backstage), typeof(Ribbon), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the right-side common panel content (like Copilot in Outlook).
    /// This panel is always visible regardless of which tab is selected.
    /// </summary>
    public object? CommonPanel
    {
        get => this.GetValue(CommonPanelProperty);
        set => this.SetValue(CommonPanelProperty, value);
    }

    /// <summary>Identifies the <see cref="CommonPanel"/> dependency property.</summary>
    public static readonly DependencyProperty CommonPanelProperty =
        DependencyProperty.Register(nameof(CommonPanel), typeof(object), typeof(Ribbon), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets a value indicating whether the backstage or start screen is currently open.
    /// </summary>
    public bool IsBackstageOrStartScreenOpen
    {
        get => (bool)this.GetValue(IsBackstageOrStartScreenOpenProperty);
        set => this.SetValue(IsBackstageOrStartScreenOpenProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsBackstageOrStartScreenOpen"/> dependency property.</summary>
    public static readonly DependencyProperty IsBackstageOrStartScreenOpenProperty =
        DependencyProperty.Register(nameof(IsBackstageOrStartScreenOpen), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Gets or sets the layout mode of the ribbon (Classic or Simplified).
    /// This property automatically sets <see cref="IsSimplified"/> based on the selected mode.
    /// </summary>
    public RibbonLayoutMode LayoutMode
    {
        get => (RibbonLayoutMode)this.GetValue(LayoutModeProperty);
        set => this.SetValue(LayoutModeProperty, value);
    }

    /// <summary>Identifies the <see cref="LayoutMode"/> dependency property.</summary>
    public static readonly DependencyProperty LayoutModeProperty =
        DependencyProperty.Register(
            nameof(LayoutMode),
            typeof(RibbonLayoutMode),
            typeof(Ribbon),
            new PropertyMetadata(RibbonLayoutMode.Classic, OnLayoutModeChanged));

    private static void OnLayoutModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Controls.Ribbon ribbon)
        {
            // Automatically set IsSimplified based on LayoutMode
            var newLayoutMode = (RibbonLayoutMode)e.NewValue;
            ribbon.SetCurrentValue(IsSimplifiedProperty, newLayoutMode == RibbonLayoutMode.Simplified);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether or not the ribbon is in Simplified mode
    /// </summary>
    public bool IsSimplified
    {
        get => (bool)this.GetValue(IsSimplifiedProperty);
        set => this.SetValue(IsSimplifiedProperty, value);
    }

    /// <summary>Identifies the <see cref="IsSimplified"/> dependency property.</summary>
    public static readonly DependencyProperty IsSimplifiedProperty = DependencyProperty.Register(nameof(IsSimplified), typeof(bool), typeof(Ribbon), new PropertyMetadata(false, OnIsSimplifiedChanged));

    private static void OnIsSimplifiedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Controls.Ribbon ribbon)
        {
            var isSimplified = ribbon.IsSimplified;

            // Update all tabs
            // Note: RibbonTabControl.IsSimplified is automatically updated via binding in XAML
            // (See RibbonTabControl.xaml Style - IsSimplified binding to ancestor Ribbon)
            foreach (ISimplifiedStateControl item in ribbon.Tabs.OfType<ISimplifiedStateControl>())
            {
                item.UpdateSimplifiedState(isSimplified);
            }

            // Update CommonPanel content if it contains ISimplifiedStateControl items
            if (ribbon.CommonPanel is ISimplifiedStateControl simplifiedControl)
            {
                simplifiedControl.UpdateSimplifiedState(isSimplified);
            }
            else if (ribbon.CommonPanel is Panel panel)
            {
                foreach (ISimplifiedStateControl child in panel.Children.OfType<ISimplifiedStateControl>())
                {
                    child.UpdateSimplifiedState(isSimplified);
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets selected tab item
    /// </summary>
    public RibbonTabItem? SelectedTabItem
    {
        get => (RibbonTabItem?)this.GetValue(SelectedTabItemProperty);
        set => this.SetValue(SelectedTabItemProperty, value);
    }

    /// <summary>Identifies the <see cref="SelectedTabItem"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedTabItemProperty =
        DependencyProperty.Register(nameof(SelectedTabItem), typeof(RibbonTabItem), typeof(Ribbon), new PropertyMetadata(OnSelectedTabItemChanged));

    private static void OnSelectedTabItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ribbon = (Ribbon)d;
        if (ribbon.TabControl is not null)
        {
            ribbon.TabControl.SetCurrentValue(System.Windows.Controls.Primitives.Selector.SelectedItemProperty, e.NewValue);
        }

        if (e.NewValue is RibbonTabItem selectedItem
            && ribbon.Tabs.Contains(selectedItem))
        {
            ribbon.SelectedTabIndex = ribbon.Tabs.IndexOf(selectedItem);
        }
        else
        {
            ribbon.SelectedTabIndex = -1;
        }
    }

    /// <summary>
    /// Gets or sets selected tab index
    /// </summary>
    public int SelectedTabIndex
    {
        get => (int)this.GetValue(SelectedTabIndexProperty);
        set => this.SetValue(SelectedTabIndexProperty, value);
    }

    /// <summary>Identifies the <see cref="SelectedTabIndex"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedTabIndexProperty =
        DependencyProperty.Register(nameof(SelectedTabIndex), typeof(int), typeof(Ribbon), new PropertyMetadata(-1, OnSelectedTabIndexChanged));

    private static void OnSelectedTabIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ribbon = (Ribbon)d;
        var selectedIndex = (int)e.NewValue;

        if (ribbon.TabControl is not null)
        {
            ribbon.TabControl.SetCurrentValue(System.Windows.Controls.Primitives.Selector.SelectedIndexProperty, selectedIndex);
        }

        if (selectedIndex >= 0
            && selectedIndex < ribbon.Tabs.Count)
        {
            ribbon.SelectedTabItem = ribbon.Tabs[selectedIndex];
        }
        else
        {
            ribbon.SelectedTabItem = null;
        }
    }

    private static void AddOrRemoveLogicalChildOnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ribbon = (Ribbon)d;
        if (e.OldValue is not null)
        {
            ribbon.RemoveLogicalChild(e.OldValue);
        }

        if (e.NewValue is not null)
        {
            ribbon.AddLogicalChild(e.NewValue);
        }
    }

    /// <summary>
    /// Gets the first visible TabItem
    /// </summary>
    public RibbonTabItem? FirstVisibleItem => this.GetFirstVisibleItem();

    /// <summary>
    /// Gets the last visible TabItem
    /// </summary>
    public RibbonTabItem? LastVisibleItem => this.GetLastVisibleItem();

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether ribbon can be minimized
    /// </summary>
    public bool CanMinimize
    {
        get => (bool)this.GetValue(CanMinimizeProperty);
        set => this.SetValue(CanMinimizeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether ribbon is minimized
    /// </summary>
    public bool IsMinimized
    {
        get => (bool)this.GetValue(IsMinimizedProperty);
        set => this.SetValue(IsMinimizedProperty, value);
    }

    /// <summary>Identifies the <see cref="IsMinimized"/> dependency property.</summary>
    public static readonly DependencyProperty IsMinimizedProperty =
        DependencyProperty.Register(
            nameof(IsMinimized),
            typeof(bool),
            typeof(Ribbon),
            new PropertyMetadata(false, OnIsMinimizedChanged));

    /// <summary>Identifies the <see cref="CanMinimize"/> dependency property.</summary>
    public static readonly DependencyProperty CanMinimizeProperty =
        DependencyProperty.Register(nameof(CanMinimize), typeof(bool), typeof(Ribbon), new PropertyMetadata(true));

    private static void OnIsMinimizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ribbon = (Ribbon)d;

        var oldValue = (bool)e.OldValue;
        var newValue = (bool)e.NewValue;

        ribbon.IsMinimizedChanged?.Invoke(ribbon, e);
    }

    /// <summary>Identifies the <see cref="AreTabHeadersVisible"/> dependency property.</summary>
    public static readonly DependencyProperty AreTabHeadersVisibleProperty = DependencyProperty.Register(nameof(AreTabHeadersVisible), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets a value indicating whether defines whether tab headers are visible or not.
    /// </summary>
    public bool AreTabHeadersVisible
    {
        get => (bool)this.GetValue(AreTabHeadersVisibleProperty);
        set => this.SetValue(AreTabHeadersVisibleProperty, BooleanBoxes.Box(value));
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether ribbon can be switched
    /// </summary>
    public bool CanUseSimplified
    {
        get => (bool)this.GetValue(CanUseSimplifiedProperty);
        set => this.SetValue(CanUseSimplifiedProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="CanUseSimplified"/> dependency property.</summary>
    public static readonly DependencyProperty CanUseSimplifiedProperty =
        DependencyProperty.Register(nameof(CanUseSimplified), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Gets or sets the height of the gap between the ribbon and the regular window content
    /// </summary>
    public double ContentGapHeight
    {
        get => (double)this.GetValue(ContentGapHeightProperty);
        set => this.SetValue(ContentGapHeightProperty, value);
    }

    /// <summary>Identifies the <see cref="ContentGapHeight"/> dependency property.</summary>
    public static readonly DependencyProperty ContentGapHeightProperty =
        DependencyProperty.Register(nameof(ContentGapHeight), typeof(double), typeof(Ribbon), new PropertyMetadata(RibbonTabControl.DefaultContentGapHeight));

    /// <summary>
    /// Gets or sets the height of the ribbon content area
    /// </summary>
    public double ContentHeight
    {
        get => (double)this.GetValue(ContentHeightProperty);
        set => this.SetValue(ContentHeightProperty, value);
    }

    /// <summary>Identifies the <see cref="ContentHeight"/> dependency property.</summary>
    public static readonly DependencyProperty ContentHeightProperty =
        DependencyProperty.Register(nameof(ContentHeight), typeof(double), typeof(Ribbon), new PropertyMetadata(RibbonTabControl.DefaultContentHeight));

    /// <summary>
    /// Gets or sets a value indicating whether gets whether ribbon is collapsed
    /// </summary>
    public bool IsCollapsed
    {
        get => (bool)this.GetValue(IsCollapsedProperty);
        set => this.SetValue(IsCollapsedProperty, value);
    }

    /// <summary>Identifies the <see cref="IsCollapsed"/> dependency property.</summary>
    public static readonly DependencyProperty IsCollapsedProperty =
        DependencyProperty.Register(
            nameof(IsCollapsed),
            typeof(bool),
            typeof(Ribbon),
            new PropertyMetadata(false, OnIsCollapsedChanged));

    private static void OnIsCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ribbon = (Ribbon)d;
        ribbon.IsCollapsedChanged?.Invoke(ribbon, e);
    }

    /// <summary>
    /// Gets or sets a value indicating whether defines if the Ribbon should automatically set <see cref="IsCollapsed"/> when the width or height of the owner window drop under <see cref="MinimalVisibleWidth"/> or <see cref="MinimalVisibleHeight"/>
    /// </summary>
    public bool IsAutomaticCollapseEnabled
    {
        get => (bool)this.GetValue(IsAutomaticCollapseEnabledProperty);
        set => this.SetValue(IsAutomaticCollapseEnabledProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsAutomaticCollapseEnabled"/> dependency property.</summary>
    public static readonly DependencyProperty IsAutomaticCollapseEnabledProperty =
        DependencyProperty.Register(nameof(IsAutomaticCollapseEnabled), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.TrueBox, OnIsAutomaticCollapseEnabledChanged));

    /// <summary>
    /// Gets toggle ribbon minimize command
    /// </summary>
    public static readonly RoutedCommand ToggleMinimizeTheRibbonCommand = new(nameof(ToggleMinimizeTheRibbonCommand), typeof(Ribbon));

    /// <summary>
    /// Gets Toggle simplified ribbon command (toggles between Classic and Simplified)
    /// </summary>
    public static readonly RoutedCommand ToggleSimplifiedCommand = new(nameof(ToggleSimplifiedCommand), typeof(Ribbon));

    static Ribbon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Ribbon), new FrameworkPropertyMetadata(typeof(Ribbon)));

        // Subscribe to menu commands
        CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(ToggleMinimizeTheRibbonCommand, OnToggleMinimizeTheRibbonCommandExecuted, OnToggleMinimizeTheRibbonCommandCanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(ToggleSimplifiedCommand, OnToggleSimplifiedCommandExecuted, OnSwitchTheRibbonCommandCanExecute));

        // CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(CustomizeTheRibbonCommand, OnCustomizeTheRibbonCommandExecuted, OnCustomizeTheRibbonCommandCanExecute));
    }

    // Default constructor
    public Ribbon()
    {
        this.VerticalAlignment = VerticalAlignment.Top;
        this.Loaded += this.OnLoaded;
        this.Unloaded += this.OnUnloaded;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        this.MaintainIsCollapsed();
    }

    private static void OnIsAutomaticCollapseEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((Ribbon)d).MaintainIsCollapsed();
    }

    private void MaintainIsCollapsed()
    {
        if (this.IsAutomaticCollapseEnabled == false
            || this.ownerWindow is null)
        {
            return;
        }

        if (this.ownerWindow.ActualWidth < MinimalVisibleWidth
            || this.ownerWindow.ActualHeight < MinimalVisibleHeight)
        {
            this.SetCurrentValue(IsCollapsedProperty, BooleanBoxes.TrueBox);
        }
        else
        {
            this.SetCurrentValue(IsCollapsedProperty, BooleanBoxes.FalseBox);
        }
    }

    /// <inheritdoc />
    protected override void OnGotFocus(RoutedEventArgs e)
    {
        var ribbonTabItem = (RibbonTabItem?)this.TabControl?.SelectedItem;
        _ = ribbonTabItem?.Focus();
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        // 기존 TabControl 이벤트 해제 및 CollectionSyncHelper Dispose
        if (this.TabControl != null)
        {
            this.TabControl.SelectionChanged -= this.OnTabControlSelectionChanged;
            this.tabsSync = null;
        }

        this.layoutRoot = this.GetTemplateChild("PART_LayoutRoot") as Panel;
        this.TabControl = this.GetTemplateChild("PART_RibbonTabControl") as RibbonTabControl;

        if (this.TabControl == null)
        {
            throw new InvalidOperationException("PART_RibbonTabControl not found in the template.");
        }

        this.TabControl.SelectionChanged += this.OnTabControlSelectionChanged;
        this.tabsSync = new CollectionSyncHelper<RibbonTabItem>(this.Tabs, this.TabControl.Items);

        // 이전 선택된 탭 복원 (있다면)
        RibbonTabItem? selectedTab = this.SelectedTabItem ?? this.TabControl.SelectedItem as RibbonTabItem;
        if (selectedTab != null)
        {
            this.TabControl.SetCurrentValue(System.Windows.Controls.Primitives.Selector.SelectedItemProperty, selectedTab);
        }
    }

    public static void OnLogicalChildPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        ILogicalChildSupport logicalChildSupport = d as ILogicalChildSupport ?? throw new ArgumentException("Argument must be of type ILogicalChildSupport.", nameof(d));

        if (e.OldValue is DependencyObject oldValue)
        {
            logicalChildSupport.RemoveLogicalChild(oldValue);
        }

        if (e.NewValue is DependencyObject newValue
            && LogicalTreeHelper.GetParent(newValue) is null)
        {
            logicalChildSupport.AddLogicalChild(newValue);
        }
    }

    // Handles tab control selection changed
    private void OnTabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ReferenceEquals(e.OriginalSource, this.TabControl) == false)
        {
            return;
        }

        this.SetCurrentValue(SelectedTabItemProperty, this.TabControl?.SelectedItem as RibbonTabItem);
        this.SetCurrentValue(SelectedTabIndexProperty, this.TabControl?.SelectedIndex ?? -1);

        this.SelectedTabChanged?.Invoke(this, e);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        this.AttachToWindow();

        this.LoadInitialState();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        this.RibbonStateStorage.Save();

        if (this.ownerWindow is not null)
        {
        }
    }

    /// <summary>
    /// Called when the <see cref="ownerWindow"/> is closed, so that we set it to null.
    /// </summary>
    private void OnOwnerWindowClosed(object? sender, EventArgs e)
    {
        this.DetachFromWindow();
    }

    private void AttachToWindow()
    {
        this.DetachFromWindow();

        this.ownerWindow = Window.GetWindow(this);

        if (this.ownerWindow is not null)
        {
            this.ownerWindow.Closed += this.OnOwnerWindowClosed;
            this.ownerWindow.SizeChanged += this.OnSizeChanged;
        }
    }

    private void DetachFromWindow()
    {
        if (this.ownerWindow is not null)
        {
            this.RibbonStateStorage.Save();
            this.RibbonStateStorage.Dispose();
            this.ribbonStateStorage = null;

            this.ownerWindow.Closed -= this.OnOwnerWindowClosed;
            this.ownerWindow.SizeChanged -= this.OnSizeChanged;
        }

        this.ownerWindow = null;
    }

    private RibbonTabItem? GetFirstVisibleItem()
    {
        return this.Tabs.FirstOrDefault(item => item.Visibility == Visibility.Visible);
    }

    private RibbonTabItem? GetLastVisibleItem()
    {
        return this.Tabs.LastOrDefault(item => item.Visibility == Visibility.Visible);
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether Quick Access ToolBar can
    /// save and load its state automatically
    /// </summary>
    public bool AutomaticStateManagement
    {
        get => (bool)this.GetValue(AutomaticStateManagementProperty);
        set => this.SetValue(AutomaticStateManagementProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="AutomaticStateManagement"/> dependency property.</summary>
    public static readonly DependencyProperty AutomaticStateManagementProperty =
        DependencyProperty.Register(nameof(AutomaticStateManagement), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.TrueBox, OnAutomaticStateManagementChanged, CoerceAutomaticStateManagement));

    private static object? CoerceAutomaticStateManagement(DependencyObject d, object? basevalue)
    {
        var ribbon = (Ribbon)d;
        if (ribbon.RibbonStateStorage.IsLoading)
        {
            return BooleanBoxes.FalseBox;
        }

        return basevalue;
    }

    private static void OnAutomaticStateManagementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ribbon = (Ribbon)d;
        if ((bool)e.NewValue)
        {
            ribbon.LoadInitialState();
        }
    }

    private void LoadInitialState()
    {
        if (this.RibbonStateStorage.IsLoaded)
        {
            return;
        }

        this.RibbonStateStorage.Load();

        if (this.SelectedTabItem is null)
        {
            this.TabControl?.SelectFirstTab();
        }
    }

    void ILogicalChildSupport.AddLogicalChild(object child)
    {
        this.AddLogicalChild(child);
    }

    /// <inheritdoc />
    void ILogicalChildSupport.RemoveLogicalChild(object child)
    {
        this.RemoveLogicalChild(child);
    }

    /// <inheritdoc />
    protected override IEnumerator LogicalChildren
    {
        get
        {
            IEnumerator baseEnumerator = base.LogicalChildren;
            while (baseEnumerator?.MoveNext() == true)
            {
                yield return baseEnumerator.Current;
            }

            if (this.layoutRoot is not null)
            {
                yield return this.layoutRoot;
            }
        }
    }

    // Occurs when customize toggle minimize command can execute handles
    private static void OnToggleMinimizeTheRibbonCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is Ribbon ribbon)
        {
            e.CanExecute = ribbon.CanMinimize;
        }
    }

    // Occurs when toggle minimize command executed
    private static void OnToggleMinimizeTheRibbonCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Ribbon ribbon)
        {
            ribbon.IsMinimized = !ribbon.IsMinimized;
        }
    }

    // Occurs when customize switch ribbon command can execute handles
    private static void OnSwitchTheRibbonCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is Ribbon ribbon)
        {
            e.CanExecute = ribbon.CanUseSimplified;
        }
    }

    // Occurs when toggle simplified ribbon command executed
    private static void OnToggleSimplifiedCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Ribbon ribbon)
        {
            ribbon.LayoutMode = ribbon.LayoutMode == RibbonLayoutMode.Simplified
                ? RibbonLayoutMode.Classic
                : RibbonLayoutMode.Simplified;
        }
    }
}