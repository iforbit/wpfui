// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Wpf.Ui.Extensions;
using Wpf.Ui.Internal;
using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls.Ribbon;

[StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(RibbonTabItem))]
[TemplatePart(Name = "PART_TabsContainer", Type = typeof(Panel))]
[TemplatePart(Name = "PART_SelectedContent", Type = typeof(FrameworkElement))]
public class RibbonTabControl : Selector, ILogicalChildSupport
{
    /// <summary>
    /// Default value for <see cref="ContentGapHeight"/>.
    /// </summary>
    public const double DefaultContentGapHeight = 3;

    /// <summary>
    /// Default value for <see cref="ContentHeight"/>.
    /// </summary>
    public const double DefaultContentHeight = 100;

    /// <summary>
    /// Gets or sets the height of the content area.
    /// </summary>
    public double ContentHeight
    {
        get => (double)this.GetValue(ContentHeightProperty);
        set => this.SetValue(ContentHeightProperty, value);
    }

    /// <summary>Identifies the <see cref="ContentHeight"/> dependency property.</summary>
    public static readonly DependencyProperty ContentHeightProperty =
        DependencyProperty.Register(nameof(ContentHeight), typeof(double), typeof(RibbonTabControl), new FrameworkPropertyMetadata(DefaultContentHeight, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// Gets or sets the height of the gap between the ribbon and the content
    /// </summary>
    public double ContentGapHeight
    {
        get => (double)this.GetValue(ContentGapHeightProperty);
        set => this.SetValue(ContentGapHeightProperty, value);
    }

    /// <summary>Identifies the <see cref="ContentGapHeight"/> dependency property.</summary>
    public static readonly DependencyProperty ContentGapHeightProperty =
        DependencyProperty.Register(nameof(ContentGapHeight), typeof(double), typeof(RibbonTabControl), new PropertyMetadata(DefaultContentGapHeight));

    /// <summary>Identifies the <see cref="AreTabHeadersVisible"/> dependency property.</summary>
    public static readonly DependencyProperty AreTabHeadersVisibleProperty = DependencyProperty.Register(nameof(AreTabHeadersVisible), typeof(bool), typeof(RibbonTabControl), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Defines whether tab headers are visible or not.
    /// </summary>
    public bool AreTabHeadersVisible
    {
        get => (bool)this.GetValue(AreTabHeadersVisibleProperty);
        set => this.SetValue(AreTabHeadersVisibleProperty, BooleanBoxes.Box(value));
    }

    /// <summary>
    /// Gets the <see cref="Panel"/> responsible for displaying the selected tabs content.
    /// </summary>
    public Panel? TabsContainer { get; private set; }

    /// <summary>
    /// Gets the <see cref="ContentControl"/> responsible for displaying the selected tabs content.
    /// </summary>
    public FrameworkElement? SelectedContentPresenter { get; private set; }

    /// <summary>
    /// Gets content of selected tab item
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public object? SelectedContent
    {
        get => this.GetValue(SelectedContentProperty);

        internal set => this.SetValue(SelectedContentPropertyKey, value);
    }

    // DependencyProperty key for SelectedContent
    private static readonly DependencyPropertyKey SelectedContentPropertyKey = DependencyProperty.RegisterReadOnly(nameof(SelectedContent), typeof(object), typeof(RibbonTabControl), new PropertyMetadata(Ribbon.OnLogicalChildPropertyChanged));

    /// <summary>Identifies the <see cref="SelectedContent"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedContentProperty = SelectedContentPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets or sets selected tab item
    /// </summary>
    internal RibbonTabItem? SelectedTab
    {
        get => (RibbonTabItem?)this.GetValue(SelectedTabProperty);
        private set => this.SetValue(SelectedTabProperty, value);
    }

    /// <summary>Identifies the <see cref="SelectedTab"/> dependency property.</summary>
    internal static readonly DependencyProperty SelectedTabProperty =
        DependencyProperty.Register(nameof(SelectedTab), typeof(RibbonTabItem), typeof(RibbonTabControl), new PropertyMetadata());

    /// <summary>
    /// Gets or sets selected tab item
    /// </summary>
    internal RibbonTabItem? SelectedTabItem
    {
        get => (RibbonTabItem?)this.GetValue(SelectedTabItemProperty);
        private set => this.SetValue(SelectedTabItemProperty, value);
    }

    /// <summary>Identifies the <see cref="SelectedTabItem"/> dependency property.</summary>
    internal static readonly DependencyProperty SelectedTabItemProperty =
        DependencyProperty.Register(nameof(SelectedTabItem), typeof(RibbonTabItem), typeof(RibbonTabControl), new PropertyMetadata());

    /// <summary>
    /// Gets or sets whether ribbon is minimized
    /// </summary>
    public bool IsMinimized
    {
        get => (bool)this.GetValue(IsMinimizedProperty);
        set => this.SetValue(IsMinimizedProperty, value);
    }

    /// <summary>Identifies the <see cref="IsMinimized"/> dependency property.</summary>
    public static readonly DependencyProperty IsMinimizedProperty = DependencyProperty.Register(nameof(IsMinimized), typeof(bool), typeof(RibbonTabControl), new PropertyMetadata(false, OnIsMinimizedChanged));

    /// <summary>
    /// Gets or sets whether ribbon can be minimized
    /// </summary>
    public bool CanMinimize
    {
        get => (bool)this.GetValue(CanMinimizeProperty);
        set => this.SetValue(CanMinimizeProperty, value);
    }

    /// <summary>Identifies the <see cref="CanMinimize"/> dependency property.</summary>
    public static readonly DependencyProperty CanMinimizeProperty = DependencyProperty.Register(nameof(CanMinimize), typeof(bool), typeof(RibbonTabControl), new PropertyMetadata(true));

    /// <summary>
    /// Gets or sets whether ribbon is simplified
    /// </summary>
    public bool IsSimplified
    {
        get => (bool)this.GetValue(IsSimplifiedProperty);
        set => this.SetValue(IsSimplifiedProperty, value);
    }

    /// <summary>Identifies the <see cref="IsSimplified"/> dependency property.</summary>
    public static readonly DependencyProperty IsSimplifiedProperty = DependencyProperty.Register(nameof(IsSimplified), typeof(bool), typeof(RibbonTabControl), new PropertyMetadata(false));

    /// <summary>
    /// Gets or sets whether ribbon can be switched simplified
    /// </summary>
    public bool CanUseSimplified
    {
        get => (bool)this.GetValue(CanUseSimplifiedProperty);
        set => this.SetValue(CanUseSimplifiedProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="CanUseSimplified"/> dependency property.</summary>
    public static readonly DependencyProperty CanUseSimplifiedProperty = DependencyProperty.Register(nameof(CanUseSimplified), typeof(bool), typeof(RibbonTabControl), new PropertyMetadata(BooleanBoxes.FalseBox));


    /// <summary>
    /// Initializes static members of the <see cref="RibbonTabControl"/> class.
    /// </summary>
    static RibbonTabControl()
    {
        var type = typeof(RibbonTabControl);

        DefaultStyleKeyProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(typeof(RibbonTabControl)));
        IsTabStopProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));

        KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonTabControl"/> class.
    /// </summary>
    public RibbonTabControl()
    {

        this.Loaded += this.OnLoaded;
        this.Unloaded += this.OnUnloaded;

        SelectorHelper.SetCanSelectMultiple(this, false);
    }

    /// <inheritdoc />
    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        this.ItemContainerGenerator.StatusChanged += this.OnGeneratorStatusChanged;
    }

    public override void OnApplyTemplate()
    {
        this.TabsContainer = this.GetTemplateChild("PART_TabsContainer") as Panel;
        this.SelectedContentPresenter = this.Template.FindName("PART_SelectedContent", this) as FrameworkElement;
    }

    // Get selected ribbon tab item
    private RibbonTabItem? GetSelectedTabItem()
    {
        var selectedItem = this.SelectedItem;
        if (selectedItem is null)
        {
            return null;
        }

        var item = selectedItem as RibbonTabItem
                   ?? this.ItemContainerGenerator.ContainerOrContainerContentFromIndex<RibbonTabItem>(this.SelectedIndex);

        return item;
    }

    // Find next tab item
    private RibbonTabItem? FindNextTabItem(int startIndex, int direction)
    {
        if (direction != 0)
        {
            var index = startIndex;
            for (var i = 0; i < this.Items.Count; i++)
            {
                index += direction;

                if (index >= this.Items.Count)
                {
                    index = 0;
                }
                else if (index < 0)
                {
                    index = this.Items.Count - 1;
                }

                if (this.ItemContainerGenerator.ContainerOrContainerContentFromIndex<RibbonTabItem>(index) is { } nextItem
                    && nextItem.IsEnabled
                    && nextItem.Visibility == Visibility.Visible)
                {
                    return nextItem;
                }
            }
        }

        return null;
    }

    // Updates selected content
    private void UpdateSelectedContent()
    {
        if (this.SelectedIndex < 0)
        {
            this.SelectedContent = null;
            this.SelectedTabItem = null;
        }
        else
        {
            var selectedTabItem = this.GetSelectedTabItem();
            if (selectedTabItem is not null)
            {
                //this.SelectedContent = selectedTabItem.GroupsContainer;
                this.SelectedTabItem = selectedTabItem;
            }
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
    }

    // Handles GeneratorStatus changed
    private void OnGeneratorStatusChanged(object? sender, EventArgs e)
    {
        if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
        {
            this.UpdateSelectedContent();
        }
    }


    /// <summary>
    /// Selects the first tab if <see cref="IsMinimized"/> is <c>false</c>.
    /// </summary>
    public void SelectFirstTab()
    {
        if (this.IsMinimized)
        {
            return;
        }

        this.SetCurrentValue(SelectedItemProperty, this.GetFirstVisibleAndEnabledItem());

        if (this.SelectedItem is null
            && this.IsEnabled == false)
        {
            this.SetCurrentValue(SelectedItemProperty, this.GetFirstVisibleItem());
        }

        if (this.IsKeyboardFocusWithin)
        {
            this.SelectedTabItem?.Focus();
        }
    }


    // Handles IsMinimized changed
    private static void OnIsMinimizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var tabControl = (RibbonTabControl)d;

        if (tabControl.IsMinimized == false)
        {
            //tabControl.IsDropDownOpen = false;
        }

        if ((bool)e.NewValue == false
            && tabControl.SelectedIndex < 0)
        {
            var item = tabControl.FindNextTabItem(-1, 1);

            if (item is not null)
            {
                item.IsSelected = true;
            }
        }
    }

    /// <summary>
    /// Gets the first visible item
    /// </summary>
    public object? GetFirstVisibleItem()
    {
        foreach (var item in this.Items)
        {
            if ((this.ItemContainerGenerator.ContainerOrContainerContentFromItem<RibbonTabItem>(item) ?? item) is RibbonTabItem ribbonTab
                && ribbonTab.Visibility == Visibility.Visible)
            {
                return ribbonTab;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the first visible and enabled item
    /// </summary>
    public object? GetFirstVisibleAndEnabledItem()
    {
        foreach (var item in this.Items)
        {
            if ((this.ItemContainerGenerator.ContainerOrContainerContentFromItem<RibbonTabItem>(item) ?? item) is RibbonTabItem ribbonTab
                && ribbonTab.Visibility == Visibility.Visible
                && ribbonTab.IsEnabled)
            {
                return ribbonTab;
            }
        }

        return null;
    }

    /// <inheritdoc />
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
            var baseEnumerator = base.LogicalChildren;
            while (baseEnumerator?.MoveNext() == true)
            {
                yield return baseEnumerator.Current;
            }

            if (this.SelectedContent is not null)
            {
                yield return this.SelectedContent;
            }
        }
    }
}
