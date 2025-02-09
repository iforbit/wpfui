// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Markup;
using Wpf.Ui.Controls.Collections;
using Wpf.Ui.Controls.Data;
using Wpf.Ui.Controls.Helpers;
using Wpf.Ui.Extensions;
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
    /// Handles collection of ribbon tab items changes
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">The event data</param>
    private void OnTabItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
            {
                var isSimplified = this.IsSimplified;
                foreach (ISimplifiedStateControl item in e.NewItems.NullSafe().OfType<ISimplifiedStateControl>())
                {
                    item.UpdateSimplifiedState(isSimplified);
                }
            }

            break;

            case NotifyCollectionChangedAction.Reset:
            {
                var isSimplified = this.IsSimplified;
                foreach (ISimplifiedStateControl item in this.Tabs.OfType<ISimplifiedStateControl>())
                {
                    item.UpdateSimplifiedState(isSimplified);
                }
            }

            break;
        }
    }

    /// <summary>Identifies the <see cref="ItemsSource"/> dependency property.</summary>
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(object),
        typeof(Ribbon),
        new FrameworkPropertyMetadata(null, OnItemsSourceChanged)
    );

    private static void OnItemsSourceChanged(DependencyObject? d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Ribbon ribbon)
        {
            return;
        }

        ribbon.Tabs.Clear();

        if (e.NewValue is IEnumerable newItemsSource and not string)
        {
            foreach (var item in newItemsSource)
            {
                if (item is RibbonTabItem tabItem)
                {
                    ribbon.Tabs.Add(tabItem);
                }
                else
                {
                    // 로그 남기거나 예외 처리
                }
            }
        }
        else if (e.NewValue != null)
        {
            if (e.NewValue is RibbonTabItem tabItem)
            {
                ribbon.Tabs.Add(tabItem);
            }
            else
            {
                // 로그 남기거나 예외 처리
            }
        }

        if (e.NewValue is INotifyCollectionChanged oc)
        {
            oc.CollectionChanged += (s, e) =>
                ribbon.OnTabItemsCollectionChanged(ribbon.Tabs, e);
        }
    }

    /// <inheritdoc/>
    [Bindable(true)]
    public object? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
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
            foreach (ISimplifiedStateControl item in ribbon.Tabs.OfType<ISimplifiedStateControl>())
            {
                item.UpdateSimplifiedState(isSimplified);
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

    static Ribbon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Ribbon), new FrameworkPropertyMetadata(typeof(Ribbon)));

        // Subscribe to menu commands
        // CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(ToggleMinimizeTheRibbonCommand, OnToggleMinimizeTheRibbonCommandExecuted, OnToggleMinimizeTheRibbonCommandCanExecute));
        // CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(SwitchToTheClassicRibbonCommand, OnSwitchToTheClassicRibbonCommandExecuted, OnSwitchTheRibbonCommandCanExecute));
        // CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(SwitchToTheSimplifiedRibbonCommand, OnSwitchToTheSimplifiedRibbonCommandExecuted, OnSwitchTheRibbonCommandCanExecute));
        // CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(CustomizeTheRibbonCommand, OnCustomizeTheRibbonCommandExecuted, OnCustomizeTheRibbonCommandCanExecute));
    }

    // Default constructor
    public Ribbon()
    {
        this.VerticalAlignment = VerticalAlignment.Top;
        this.Loaded += this.OnLoaded;
        this.Unloaded += this.OnUnloaded;
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

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        this.layoutRoot = this.GetTemplateChild("PART_LayoutRoot") as Panel;

        RibbonTabItem? selectedTab = this.SelectedTabItem;
        if (this.TabControl is not null)
        {
            this.TabControl.SelectionChanged -= this.OnTabControlSelectionChanged;
            selectedTab = this.TabControl.SelectedItem as RibbonTabItem;

            this.tabsSync?.Target.Clear();
        }

        this.TabControl = this.GetTemplateChild("PART_RibbonTabControl") as RibbonTabControl;

        if (this.TabControl is not null)
        {
            this.TabControl.SelectionChanged += this.OnTabControlSelectionChanged;

            this.tabsSync = new CollectionSyncHelper<RibbonTabItem>(this.Tabs, this.TabControl.Items);

            this.TabControl.SetCurrentValue(System.Windows.Controls.Primitives.Selector.SelectedItemProperty, selectedTab);
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
        set => this.SetValue(AutomaticStateManagementProperty, value);
    }

    /// <summary>Identifies the <see cref="AutomaticStateManagement"/> dependency property.</summary>
    public static readonly DependencyProperty AutomaticStateManagementProperty =
        DependencyProperty.Register(nameof(AutomaticStateManagement), typeof(bool), typeof(Ribbon), new PropertyMetadata(true, OnAutomaticStateManagementChanged, CoerceAutomaticStateManagement));

    private static object? CoerceAutomaticStateManagement(DependencyObject d, object? basevalue)
    {
        var ribbon = (Ribbon)d;
        if (ribbon.RibbonStateStorage.IsLoading)
        {
            return false;
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
}