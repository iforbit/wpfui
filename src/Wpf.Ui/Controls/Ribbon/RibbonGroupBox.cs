// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections;
using System.Collections.Specialized;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Wpf.Ui.Controls.Helpers;
using Wpf.Ui.Extensions;
using Wpf.Ui.Internal;
using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls;

/// <summary>
/// RibbonGroup represents a logical group of controls as they appear on
/// a RibbonTab.  These groups can resize its content
/// </summary>
[TemplatePart(Name = "PART_HeaderContentControl", Type = typeof(ContentControl))]
[TemplatePart(Name = "PART_CollapsedHeaderContentControl", Type = typeof(ContentControl))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
[TemplatePart(Name = "PART_UpPanel", Type = typeof(Panel))]
[TemplatePart(Name = "PART_ParentPanel", Type = typeof(Panel))]
[TemplatePart(Name = "PART_SnappedImage", Type = typeof(Image))]
[System.Diagnostics.DebuggerDisplay("{GetType().FullName}: Header = {Header}, Items.Count = {Items.Count}, State = {State}, IsSimplified = {IsSimplified}")]
public class RibbonGroupBox : HeaderedItemsControl, IDropDownControl, IHeaderedControl, ILogicalChildSupport, IMediumIconProvider, ISimplifiedStateControl, ILargeIconProvider
{
    private readonly ItemContainerGeneratorAction updateChildSizesItemContainerGeneratorAction;

    // up part
    private Panel? upPanel;

    private Panel? parentPanel;

    // Freezed image (created during snapping)
    private Image? snappedImage;

    // Is visual currently snapped
    private bool isSnapped;

    /// <summary>
    /// Gets get the <see cref="ContentControl"/> responsible for rendering the header.
    /// </summary>
    public ContentControl? HeaderContentControl { get; private set; }

    /// <summary>
    /// Gets get the <see cref="ContentControl"/> responsible for rendering the header when <see cref="State"/> is equal to <see cref="RibbonGroupBoxState.Collapsed"/>.
    /// </summary>
    public ContentControl? CollapsedHeaderContentControl { get; private set; }

    /// <summary>
    /// <see cref="DependencyProperty"/> for IsCollapsedHeaderContentPresenter.
    /// </summary>
    public static readonly DependencyProperty IsCollapsedHeaderContentPresenterProperty = DependencyProperty.RegisterAttached("IsCollapsedHeaderContentPresenter", typeof(bool), typeof(RibbonGroupBox), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Sets the value of <see cref="IsCollapsedHeaderContentPresenterProperty"/>.
    /// </summary>
    public static void SetIsCollapsedHeaderContentPresenter(DependencyObject element, bool value)
    {
        element.SetValue(IsCollapsedHeaderContentPresenterProperty, BooleanBoxes.Box(value));
    }

    /// <summary>
    /// Gets the value of <see cref="IsCollapsedHeaderContentPresenterProperty"/>.
    /// </summary>
    [AttachedPropertyBrowsableForType(typeof(RibbonGroupBox))]
    public static bool GetIsCollapsedHeaderContentPresenter(DependencyObject element)
    {
        return (bool)element.GetValue(IsCollapsedHeaderContentPresenterProperty);
    }

    /// <inheritdoc />
    public Popup? DropDownPopup { get; private set; }

    /// <inheritdoc />
    public bool IsContextMenuOpened { get; set; }

    /// <summary>
    /// Gets a value indicating whether shorthand for whether the GroupBox is in a state where it can act as a button
    /// </summary>
    public bool IsInButtonState => this.State == RibbonGroupBoxState.Collapsed || this.State == RibbonGroupBoxState.QuickAccess;

    /// <summary>
    /// Gets or sets the state transition for full mode
    /// </summary>
    public RibbonGroupBoxStateDefinition StateDefinition
    {
        get => (RibbonGroupBoxStateDefinition)this.GetValue(StateDefinitionProperty);
        set => this.SetValue(StateDefinitionProperty, value);
    }

    /// <summary>Identifies the <see cref="StateDefinition"/> dependency property.</summary>
    public static readonly DependencyProperty StateDefinitionProperty =
        DependencyProperty.Register(
            nameof(StateDefinition),
            typeof(RibbonGroupBoxStateDefinition),
            typeof(RibbonGroupBox),
            new PropertyMetadata(new RibbonGroupBoxStateDefinition(null), OnStateDefinitionChanged));

    // Handles StateDefinitionProperty changes
    internal static void OnStateDefinitionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = (RibbonGroupBox)d;
        if (!box.IsSimplified)
        {
            _ = box.TryClearCacheAndResetStateAndScaleAndNotifyParentRibbonGroupsContainer();
        }
    }

    /// <summary>
    /// Gets or sets the state transition for simplified mode
    /// </summary>
    public RibbonGroupBoxStateDefinition SimplifiedStateDefinition
    {
        get => (RibbonGroupBoxStateDefinition)this.GetValue(SimplifiedStateDefinitionProperty);
        set => this.SetValue(SimplifiedStateDefinitionProperty, value);
    }

    /// <summary>Identifies the <see cref="SimplifiedStateDefinition"/> dependency property.</summary>
    public static readonly DependencyProperty SimplifiedStateDefinitionProperty =
        DependencyProperty.Register(
            nameof(SimplifiedStateDefinition),
            typeof(RibbonGroupBoxStateDefinition),
            typeof(RibbonGroupBox),
            new PropertyMetadata(new RibbonGroupBoxStateDefinition("Large,Middle,Collapsed"), OnSimplifiedStateDefinitionChanged));

    // Handles SimplifiedStateDefinitionProperty changes
    internal static void OnSimplifiedStateDefinitionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = (RibbonGroupBox)d;
        if (box.IsSimplified)
        {
            _ = box.TryClearCacheAndResetStateAndScaleAndNotifyParentRibbonGroupsContainer();
        }
    }

    /// <summary>
    /// Gets or sets the current state of the group
    /// </summary>
    public RibbonGroupBoxState State
    {
        get => (RibbonGroupBoxState)this.GetValue(StateProperty);
        set => this.SetValue(StateProperty, value);
    }

    /// <summary>Identifies the <see cref="State"/> dependency property.</summary>
    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(
            nameof(State),
            typeof(RibbonGroupBoxState),
            typeof(RibbonGroupBox),
            new PropertyMetadata(RibbonGroupBoxState.Large, OnStateChanged));

    /// <summary>
    /// On state property changed
    /// </summary>
    /// <param name="d">Object</param>
    /// <param name="e">The event data</param>
    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ribbonGroupBox = (RibbonGroupBox)d;
        ribbonGroupBox.updateChildSizesItemContainerGeneratorAction.QueueAction();
        ribbonGroupBox.Focusable = ribbonGroupBox.IsInButtonState;
    }

    private void UpdateChildSizes()
    {
        RibbonGroupBoxState groupBoxState = this.State is RibbonGroupBoxState.QuickAccess
            ? RibbonGroupBoxState.Collapsed
            : this.State;
        var isSimplified = this.IsSimplified;

        foreach (var item in this.Items)
        {
            DependencyObject element = this.ItemContainerGenerator.ContainerFromItem(item);
            this.UpdateChildSizesOfUIElement(item, element, groupBoxState, isSimplified);
        }
    }

    private void UpdateChildSizesOfUIElement(object item, DependencyObject? element, RibbonGroupBoxState groupBoxState, bool isSimplified)
    {
        if (element is null)
        {
            return;
        }

        if (element is Panel panel)
        {
            for (var i = 0; i < panel.Children.Count; i++)
            {
                this.UpdateChildSizesOfUIElement(panel.Children[i], panel.Children[i], groupBoxState, isSimplified);
            }
        }

        // Bound items
        if (element is ContentPresenter contentPresenter
            && item is not DispatcherObject)
        {
            contentPresenter.WhenLoaded(x => UpdateValues(UIHelper.GetFirstVisualChild(x) ?? x, groupBoxState, isSimplified));
            return;
        }

        UpdateValues(element, groupBoxState, isSimplified);

        return;

        static void UpdateValues(DependencyObject element, RibbonGroupBoxState groupBoxState, bool isSimplified)
        {
            UpdateIsSimplifiedOfUIElement(element, isSimplified);
            RibbonProperties.SetAppropriateSize(element, groupBoxState, isSimplified);
        }
    }

    // Current scale index
    private int scale;

    /// <summary>
    /// Gets or sets scale index (for internal IRibbonScalableControl)
    /// </summary>
    internal int Scale
    {
        get => this.scale;

        set
        {
            var difference = value - this.scale;
            this.scale = value;

            for (var i = 0; i < Math.Abs(difference); i++)
            {
                if (difference > 0)
                {
                    this.EnlargeScalableItems();
                }
                else
                {
                    this.ReduceScalableItems();
                }
            }
        }
    }

    private enum ScaleDirection
    {
        Enlarge,
        Reduce
    }

    // Finds and increases size of all scalable elements in this group box
    private void EnlargeScalableItems()
    {
        this.ScaleScaleableItems(ScaleDirection.Enlarge);
    }

    // Finds and decreases size of all scalable elements in this group box
    private void ReduceScalableItems()
    {
        this.ScaleScaleableItems(ScaleDirection.Reduce);
    }

    private void ScaleScaleableItems(ScaleDirection scaleDirection)
    {
        foreach (var item in this.Items)
        {
            IScalableRibbonControl? scalableRibbonControl = this.ItemContainerGenerator.ContainerOrContainerContentFromItem<IScalableRibbonControl>(item);

            if (scalableRibbonControl is null
                || (scalableRibbonControl is UIElement uiElement && uiElement.Visibility != Visibility.Visible))
            {
                continue;
            }

            switch (scaleDirection)
            {
                case ScaleDirection.Enlarge:
                    scalableRibbonControl.Enlarge();
                    break;

                case ScaleDirection.Reduce:
                    scalableRibbonControl.Reduce();
                    break;
            }
        }
    }

    private void ResetScaleableItems()
    {
        foreach (var item in this.Items)
        {
            IScalableRibbonControl? scalableRibbonControl = this.ItemContainerGenerator.ContainerOrContainerContentFromItem<IScalableRibbonControl>(item);

            if (scalableRibbonControl is null
                || (scalableRibbonControl is UIElement uiElement && uiElement.Visibility != Visibility.Visible))
            {
                continue;
            }

            scalableRibbonControl.ResetScale();
        }
    }

    /// <summary>
    /// Gets whether to reset cache when scalable control is scaled
    /// </summary>
    internal ScopeGuard CacheResetGuard { get; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets dialog launcher button visibility
    /// </summary>
    public bool IsLauncherVisible
    {
        get => (bool)this.GetValue(IsLauncherVisibleProperty);
        set => this.SetValue(IsLauncherVisibleProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsLauncherVisible"/> dependency property.</summary>
    public static readonly DependencyProperty IsLauncherVisibleProperty =
        DependencyProperty.Register(nameof(IsLauncherVisible), typeof(bool), typeof(RibbonGroupBox), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Gets or sets launcher button icon
    /// </summary>
    public object? LauncherIcon
    {
        get => this.GetValue(LauncherIconProperty);
        set => this.SetValue(LauncherIconProperty, value);
    }

    /// <summary>Identifies the <see cref="LauncherIcon"/> dependency property.</summary>
    public static readonly DependencyProperty LauncherIconProperty =
        DependencyProperty.Register(nameof(LauncherIcon), typeof(object), typeof(RibbonGroupBox), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <summary>
    /// Gets or sets launcher button text
    /// </summary>
    public string? LauncherText
    {
        get => (string?)this.GetValue(LauncherTextProperty);
        set => this.SetValue(LauncherTextProperty, value);
    }

    /// <summary>Identifies the <see cref="LauncherText"/> dependency property.</summary>
    public static readonly DependencyProperty LauncherTextProperty =
        DependencyProperty.Register(nameof(LauncherText), typeof(string), typeof(RibbonGroupBox), new PropertyMetadata());

    /// <summary>
    /// Gets or sets the command to invoke when this button is pressed. This is a dependency property.
    /// </summary>
    [Category("Action")]
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Bindable(true)]
    public ICommand? LauncherCommand
    {
        get => (ICommand?)this.GetValue(LauncherCommandProperty);

        set => this.SetValue(LauncherCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command property. This is a dependency property.
    /// </summary>
    [Bindable(true)]
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Category("Action")]
    public object? LauncherCommandParameter
    {
        get => this.GetValue(LauncherCommandParameterProperty);

        set => this.SetValue(LauncherCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the element on which to raise the specified command. This is a dependency property.
    /// </summary>
    [Bindable(true)]
    [Category("Action")]
    public IInputElement? LauncherCommandTarget
    {
        get => (IInputElement?)this.GetValue(LauncherCommandTargetProperty);

        set => this.SetValue(LauncherCommandTargetProperty, value);
    }

    /// <summary>Identifies the <see cref="LauncherCommandParameter"/> dependency property.</summary>
    public static readonly DependencyProperty LauncherCommandParameterProperty = DependencyProperty.Register(nameof(LauncherCommandParameter), typeof(object), typeof(RibbonGroupBox), new PropertyMetadata());

    /// <summary>Identifies the <see cref="LauncherCommand"/> dependency property.</summary>
    public static readonly DependencyProperty LauncherCommandProperty = DependencyProperty.Register(nameof(LauncherCommand), typeof(ICommand), typeof(RibbonGroupBox), new PropertyMetadata());

    /// <summary>Identifies the <see cref="LauncherCommandTarget"/> dependency property.</summary>
    public static readonly DependencyProperty LauncherCommandTargetProperty = DependencyProperty.Register(nameof(LauncherCommandTarget), typeof(IInputElement), typeof(RibbonGroupBox), new PropertyMetadata());

    /// <summary>
    /// Gets or sets launcher button tooltip
    /// </summary>
    public object? LauncherToolTip
    {
        get => this.GetValue(LauncherToolTipProperty);
        set => this.SetValue(LauncherToolTipProperty, value);
    }

    /// <summary>Identifies the <see cref="LauncherToolTip"/> dependency property.</summary>
    public static readonly DependencyProperty LauncherToolTipProperty =
        DependencyProperty.Register(nameof(LauncherToolTip), typeof(object), typeof(RibbonGroupBox), new PropertyMetadata());

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether launcher button is enabled
    /// </summary>
    public bool IsLauncherEnabled
    {
        get => (bool)this.GetValue(IsLauncherEnabledProperty);
        set => this.SetValue(IsLauncherEnabledProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsLauncherEnabled"/> dependency property.</summary>
    public static readonly DependencyProperty IsLauncherEnabledProperty =
        DependencyProperty.Register(nameof(IsLauncherEnabled), typeof(bool), typeof(RibbonGroupBox), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <inheritdoc />
    public bool IsDropDownOpen
    {
        get => (bool)this.GetValue(IsDropDownOpenProperty);
        set => this.SetValue(IsDropDownOpenProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsDropDownOpen"/> dependency property.</summary>
    public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(RibbonGroupBox), new PropertyMetadata(BooleanBoxes.FalseBox, OnIsDropDownOpenChanged, CoerceIsDropDownOpen));

    private static object? CoerceIsDropDownOpen(DependencyObject d, object? basevalue)
    {
        var box = (RibbonGroupBox)d;

        if ((box.State != RibbonGroupBoxState.Collapsed)
            && (box.State != RibbonGroupBoxState.QuickAccess))
        {
            return BooleanBoxes.Box(false);
        }

        return basevalue;
    }

    /// <summary>
    /// Gets or sets icon
    /// </summary>
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Localizable(false)]
    public object? Icon
    {
        get => this.GetValue(IconProperty);
        set => this.SetValue(IconProperty, value);
    }

    /// <summary>Identifies the <see cref="Icon"/> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = RibbonControl.IconProperty.AddOwner(typeof(RibbonGroupBox), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <inheritdoc />
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Localizable(false)]
    public object? MediumIcon
    {
        get => this.GetValue(MediumIconProperty);
        set => this.SetValue(MediumIconProperty, value);
    }

    /// <summary>Identifies the <see cref="MediumIcon"/> dependency property.</summary>
    public static readonly DependencyProperty MediumIconProperty = MediumIconProviderProperties.MediumIconProperty.AddOwner(typeof(RibbonGroupBox), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <inheritdoc />
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Localizable(false)]
    public object? LargeIcon
    {
        get => this.GetValue(LargeIconProperty);
        set => this.SetValue(LargeIconProperty, value);
    }

    /// <summary>Identifies the <see cref="LargeIcon"/> dependency property.</summary>
    public static readonly DependencyProperty LargeIconProperty = LargeIconProviderProperties.LargeIconProperty.AddOwner(typeof(RibbonGroupBox), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the groupbox shows a separator.
    /// </summary>
    public bool IsSeparatorVisible
    {
        get => (bool)this.GetValue(IsSeparatorVisibleProperty);
        set => this.SetValue(IsSeparatorVisibleProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsSeparatorVisible"/> dependency property.</summary>
    public static readonly DependencyProperty IsSeparatorVisibleProperty =
        DependencyProperty.Register(
            nameof(IsSeparatorVisible),
            typeof(bool),
            typeof(RibbonGroupBox),
            new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets a value indicating whether gets or sets whether or not the ribbon is in simplified mode.
    /// </summary>
    public bool IsSimplified
    {
        get => (bool)this.GetValue(IsSimplifiedProperty);
        private set => this.SetValue(IsSimplifiedPropertyKey, BooleanBoxes.Box(value));
    }

    // ReSharper disable once InconsistentNaming
    private static readonly DependencyPropertyKey IsSimplifiedPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsSimplified), typeof(bool), typeof(RibbonGroupBox), new PropertyMetadata(BooleanBoxes.FalseBox, OnIsSimplifiedChanged));

    /// <summary>Identifies the <see cref="IsSimplified"/> dependency property.</summary>
    public static readonly DependencyProperty IsSimplifiedProperty = IsSimplifiedPropertyKey.DependencyProperty;

    /// <summary>
    /// Called when <see cref="IsSimplified"/> changes.
    /// </summary>
    private static void OnIsSimplifiedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = (RibbonGroupBox)d;
        _ = box.TryClearCacheAndResetStateAndScaleAndNotifyParentRibbonGroupsContainer();
        box.updateChildSizesItemContainerGeneratorAction.QueueAction();
    }

    private static void UpdateIsSimplifiedOfUIElement(DependencyObject? element, bool isSimplified)
    {
        if (element is ISimplifiedStateControl simplifiedStateControl)
        {
            simplifiedStateControl.UpdateSimplifiedState(isSimplified);
        }
    }

    /// <summary>
    /// Dialog launcher btton click event
    /// </summary>
    public event RoutedEventHandler? LauncherClick;

    /// <inheritdoc />
    public event EventHandler? DropDownOpened;

    /// <inheritdoc />
    public event EventHandler? DropDownClosed;

    /// <summary>
    /// Initializes static members of the <see cref="RibbonGroupBox"/> class.
    /// </summary>
    static RibbonGroupBox()
    {
        Type type = typeof(RibbonGroupBox);

        DefaultStyleKeyProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(type));
        _ = VisibilityProperty.AddOwner(type, new PropertyMetadata(OnVisibilityChanged));
        _ = FontSizeProperty.AddOwner(type, new FrameworkPropertyMetadata(OnFontSizeChanged));
        _ = FontFamilyProperty.AddOwner(type, new FrameworkPropertyMetadata(OnFontFamilyChanged));

        PopupService.Attach(type);

        // ContextMenuService.Attach(type);
    }

    private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = (RibbonGroupBox)d;
        _ = box.TryClearCacheAndResetStateAndScaleAndNotifyParentRibbonGroupsContainer();
    }

    private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = (RibbonGroupBox)d;
        _ = box.TryClearCacheAndResetStateAndScaleAndNotifyParentRibbonGroupsContainer();
    }

    private static void OnFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = (RibbonGroupBox)d;
        _ = box.TryClearCacheAndResetStateAndScaleAndNotifyParentRibbonGroupsContainer();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonGroupBox"/> class.
    /// Default constructor
    /// </summary>
    public RibbonGroupBox()
    {
        this.CacheResetGuard = new ScopeGuard(() => { }, () => { });

        this.CoerceValue(ContextMenuProperty);
        this.Focusable = false;

        this.Loaded += this.OnLoaded;
        this.Unloaded += this.OnUnloaded;

        this.updateChildSizesItemContainerGeneratorAction = new ItemContainerGeneratorAction(this.ItemContainerGenerator, this.UpdateChildSizes);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        this.SubscribeEvents();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        this.SetCurrentValue(IsDropDownOpenProperty, false);

        if (this.IsDropDownOpen
            && this.State is RibbonGroupBoxState.QuickAccess)
        {
            this.OnPopupClosed(this, EventArgs.Empty);
        }

        this.UnSubscribeEvents();
    }

    private void SubscribeEvents()
    {
        // Always unsubscribe events to ensure we don't subscribe twice
        this.UnSubscribeEvents();

        if (this.DropDownPopup is not null)
        {
            this.DropDownPopup.Opened += this.OnPopupOpened;
            this.DropDownPopup.Closed += this.OnPopupClosed;
        }
    }

    private void UnSubscribeEvents()
    {
        if (this.DropDownPopup is not null)
        {
            this.DropDownPopup.Opened -= this.OnPopupOpened;
            this.DropDownPopup.Closed -= this.OnPopupClosed;
        }
    }

    /// <summary>
    /// Gets a panel with items
    /// </summary>
    internal Panel? GetPanel()
    {
        return this.upPanel;
    }

    /// <summary>
    /// Gets cmmon layout root for popup and groupbox
    /// </summary>
    internal Panel? GetLayoutRoot()
    {
        return this.parentPanel;
    }

    /// <summary>
    /// Gets or sets a value indicating whether snaps / Unsnaps the Visual
    /// (remove visuals and substitute with freezed image)
    /// </summary>
    public bool IsSnapped
    {
        get => this.isSnapped;

        set
        {
            if (value == this.isSnapped)
            {
                return;
            }

            if (value)
            {
                if (this.IsVisible)
                {
                    // Render the freezed image
                    var renderTargetBitmap = new RenderTargetBitmap((int)this.ActualWidth, (int)this.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                    renderTargetBitmap.Render((Visual)VisualTreeHelper.GetChild(this, 0));

                    if (this.snappedImage is not null)
                    {
                        this.snappedImage.SetCurrentValue(FlowDirectionProperty, this.FlowDirection);
                        this.snappedImage.SetCurrentValue(Image.SourceProperty, renderTargetBitmap);
                        this.snappedImage.SetCurrentValue(WidthProperty, this.ActualWidth);
                        this.snappedImage.SetCurrentValue(HeightProperty, this.ActualHeight);
                        this.snappedImage.SetCurrentValue(VisibilityProperty, Visibility.Visible);
                    }

                    this.isSnapped = true;
                }
            }
            else if (this.snappedImage is not null)
            {
                // Clean up
                this.snappedImage.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
                this.isSnapped = false;
            }

            this.InvalidateVisual();
        }
    }

    /// <summary>
    /// Gets or sets intermediate state of the group box
    /// </summary>
    internal RibbonGroupBoxState StateIntermediate { get; set; }

    /// <summary>
    /// Gets or sets intermediate scale of the group box
    /// </summary>
    internal int ScaleIntermediate { get; set; }

    /// <summary>
    /// Gets intermediate desired size
    /// </summary>
    internal Size GetDesiredSizeIntermediate()
    {
        var contentHeight = UIHelper.GetParent<RibbonTabControl>(this)?.ContentHeight ?? RibbonTabControl.DefaultContentHeight;

        using (this.CacheResetGuard.Start())
        {
            // Get desired size for these values
            this.SetCurrentValue(StateProperty, this.StateIntermediate);
            this.Scale = this.ScaleIntermediate;
            this.InvalidateLayout();
            this.Measure(new Size(double.PositiveInfinity, contentHeight));
            return this.DesiredSize;
        }
    }

    internal bool TryClearCacheAndResetStateAndScale()
    {
        if (this.CacheResetGuard.IsActive
            || this.State == RibbonGroupBoxState.QuickAccess)
        {
            return false;
        }

        this.SetCurrentValue(StateProperty, this.StateDefinition.States[0]);
        this.Scale = 0;
        this.StateIntermediate = this.StateDefinition.States[0];
        this.ScaleIntermediate = 0;

        this.ResetScaleableItems();

        return true;
    }

    /// <summary>
    /// Tries to clear the cache, reset the state and reset the scale.
    /// If that succeeds the parent <see cref="RibbonGroupsContainer"/> is notified about that.
    /// </summary>
    /// <returns><c>true</c> if the cache was reset. Otherwise <c>false</c>.</returns>
    public bool TryClearCacheAndResetStateAndScaleAndNotifyParentRibbonGroupsContainer()
    {
        // We should try to clear the entire cache.
        // The entire cache should only be cleared if we don't do regular measuring, but only if some event outside our own measuring code caused size changes (such as elements getting visible/invisible or being added/removed).
        // For reference https://github.com/fluentribbon/Fluent.Ribbon/issues/834
        if (this.TryClearCacheAndResetStateAndScale())
        {
            UIHelper.GetParent<RibbonGroupsContainer>(this)?.GroupBoxCacheClearedAndStateAndScaleResetted(this);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Invalidates layout (with children)
    /// </summary>
    internal void InvalidateLayout()
    {
        InvalidateMeasureRecursive(this);
    }

    private static void InvalidateMeasureRecursive(UIElement element)
    {
        if (element.IsMeasureValid)
        {
            element.InvalidateMeasure();
        }

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        {
            var child = VisualTreeHelper.GetChild(element, i) as UIElement;

            if (child is null)
            {
                continue;
            }

            InvalidateMeasureRecursive(child);
        }
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        this.UnSubscribeEvents();

        // Clear cache
        _ = this.TryClearCacheAndResetStateAndScaleAndNotifyParentRibbonGroupsContainer();

        this.HeaderContentControl = this.GetTemplateChild("PART_HeaderContentControl") as ContentControl;
        this.CollapsedHeaderContentControl = this.GetTemplateChild("PART_CollapsedHeaderContentControl") as ContentControl;

        this.DropDownPopup = this.GetTemplateChild("PART_Popup") as Popup;

        this.upPanel = this.GetTemplateChild("PART_UpPanel") as Panel;
        this.parentPanel = this.GetTemplateChild("PART_ParentPanel") as Panel;

        this.snappedImage = this.GetTemplateChild("PART_SnappedImage") as Image;

        this.SubscribeEvents();
    }

    /// <inheritdoc />
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);

        _ = this.TryClearCacheAndResetStateAndScaleAndNotifyParentRibbonGroupsContainer();
        this.updateChildSizesItemContainerGeneratorAction.QueueAction();
    }

    /// <inheritdoc />
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (ReferenceEquals(e.Source, this) == false
            || this.DropDownPopup is null)
        {
            return;
        }

        if (this.State == RibbonGroupBoxState.Collapsed
            || this.State == RibbonGroupBoxState.QuickAccess)
        {
            e.Handled = true;

            if (!this.IsDropDownOpen)
            {
                this.SetCurrentValue(IsDropDownOpenProperty, true);
            }
            else
            {
                PopupService.RaiseDismissPopupEventAsync(this, DismissPopupMode.MouseNotOver);
            }
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (this.IsInButtonState == false
            || e.Handled)
        {
            base.OnKeyDown(e);
            return;
        }

        switch (e.Key)
        {
            case Key.Space:
                e.Handled = true;

                this.SetCurrentValue(IsDropDownOpenProperty, true);
                break;

            case Key.System:
                if (e.SystemKey == Key.Down
                    && e.KeyboardDevice.Modifiers == ModifierKeys.Alt)
                {
                    e.Handled = true;
                    this.SetCurrentValue(IsDropDownOpenProperty, true);
                }

                break;

            case Key.Escape:
                e.Handled = true;
                this.SetCurrentValue(IsDropDownOpenProperty, false);
                break;
        }

        base.OnKeyDown(e);
    }

    private void OnPopupOpened(object? sender, EventArgs e)
    {
        this.DropDownOpened?.Invoke(this, e);
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        this.DropDownClosed?.Invoke(this, e);
    }

    /// <summary>
    /// Dialog launcher button click handler
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">the event data</param>
    private void OnDialogLauncherButtonClick(object sender, RoutedEventArgs e)
    {
        this.LauncherClick?.Invoke(this, e);
    }

    /// <summary>
    /// Handles IsOpen propertyu changes
    /// </summary>
    /// <param name="d">Object</param>
    /// <param name="e">The event data</param>
    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var groupBox = (RibbonGroupBox)d;

        var oldValue = (bool)e.OldValue;
        var newValue = (bool)e.NewValue;

        groupBox.SetValue(System.Windows.Controls.ToolTipService.IsEnabledProperty, BooleanBoxes.Box(!newValue));

        groupBox.OnIsDropDownOpenChanged();

        (UIElementAutomationPeer.FromElement(groupBox) as Wpf.Ui.Controls.Automation.Peers.RibbonGroupBoxAutomationPeer)?.RaiseExpandCollapseAutomationEvent(oldValue, newValue);

        // todo: code in DropDownButton does nearly the same. Try to unify it.
        if (newValue)
        {
            if (groupBox.DropDownPopup is not null)
            {
                groupBox.RunInDispatcherAsync(
                    () =>
                    {
                        DependencyObject container = groupBox.ItemContainerGenerator.ContainerFromIndex(0);
                        RibbonDropDownButton.NavigateToContainer(container);

                        // Edge case: Whole dropdown content is disabled
                        if (groupBox.IsKeyboardFocusWithin == false)
                        {
                            _ = Keyboard.Focus(groupBox.DropDownPopup.Child);
                        }
                    });
            }
        }
        else
        {
            _ = Keyboard.Focus(groupBox);
        }
    }

    private void OnIsDropDownOpenChanged()
    {
        if (this.IsDropDownOpen)
        {
            this.OnRibbonGroupBoxPopupOpening();
        }
        else
        {
            this.OnRibbonGroupBoxPopupClosing();
        }
    }

    // Handles popup closing
    private void OnRibbonGroupBoxPopupClosing()
    {
        // IsHitTestVisible = true;
        if (ReferenceEquals(Mouse.Captured, this))
        {
            _ = Mouse.Capture(null);
        }
    }

    // handles popup opening
    private void OnRibbonGroupBoxPopupOpening()
    {
        // IsHitTestVisible = false;
        this.RunInDispatcherAsync(() => Mouse.Capture(this, CaptureMode.SubTree), DispatcherPriority.Loaded);
    }

    public virtual FrameworkElement CreateQuickAccessItem()
    {
        var groupBox = new RibbonGroupBox();

        RibbonControl.BindQuickAccessItem(this, groupBox);

        groupBox.DropDownOpened += this.OnQuickAccessOpened;
        groupBox.DropDownClosed += this.OnQuickAccessClosed;

        groupBox.State = RibbonGroupBoxState.QuickAccess;

        RibbonControl.Bind(this, groupBox, nameof(this.ItemTemplateSelector), ItemTemplateSelectorProperty, BindingMode.OneWay);
        RibbonControl.Bind(this, groupBox, nameof(this.ItemTemplate), ItemTemplateProperty, BindingMode.OneWay);
        RibbonControl.Bind(this, groupBox, nameof(this.ItemsSource), ItemsSourceProperty, BindingMode.OneWay);
        RibbonControl.Bind(this, groupBox, nameof(this.LauncherCommandParameter), LauncherCommandParameterProperty, BindingMode.OneWay);
        RibbonControl.Bind(this, groupBox, nameof(this.LauncherCommand), LauncherCommandProperty, BindingMode.OneWay);
        RibbonControl.Bind(this, groupBox, nameof(this.LauncherCommandTarget), LauncherCommandTargetProperty, BindingMode.OneWay);
        RibbonControl.Bind(this, groupBox, nameof(this.LauncherText), LauncherTextProperty, BindingMode.OneWay);
        RibbonControl.Bind(this, groupBox, nameof(this.LauncherToolTip), LauncherToolTipProperty, BindingMode.OneWay);
        RibbonControl.Bind(this, groupBox, nameof(this.IsLauncherEnabled), IsLauncherEnabledProperty, BindingMode.OneWay);
        RibbonControl.Bind(this, groupBox, nameof(this.IsLauncherVisible), IsLauncherVisibleProperty, BindingMode.OneWay);
        groupBox.LauncherClick += this.LauncherClick;

        if (this.Icon is not null)
        {
            if (this.Icon is Visual iconVisual)
            {
                var rect = new Rectangle
                {
                    Width = 16,
                    Height = 16,
                    Fill = new VisualBrush(iconVisual)
                };
                groupBox.Icon = rect;
            }
            else
            {
                RibbonControl.Bind(this, groupBox, nameof(this.Icon), RibbonControl.IconProperty, BindingMode.OneWay);
            }
        }

        return groupBox;
    }

    private void OnQuickAccessOpened(object? sender, EventArgs e)
    {
        if (this.IsDropDownOpen == false
            && this.IsSnapped == false)
        {
            var groupBox = (RibbonGroupBox?)sender;

            // Save state
            this.IsSnapped = true;

            if (this.ItemsSource is null)
            {
                for (var i = 0; i < this.Items.Count; i++)
                {
                    var item = this.Items[0];
                    this.Items.Remove(item);
                    _ = groupBox?.Items.Add(item);
                    i--;
                }
            }
        }
    }

    private void OnQuickAccessClosed(object? sender, EventArgs e)
    {
        var groupBox = (RibbonGroupBox?)sender;

        if (this.ItemsSource is null
            && groupBox is not null)
        {
            for (var i = 0; i < groupBox.Items.Count; i++)
            {
                var item = groupBox.Items[0];
                groupBox.Items.Remove(item);
                _ = this.Items.Add(item);
                i--;
            }
        }

        this.IsSnapped = false;
    }

    public bool CanAddToQuickAccessToolBar
    {
        get => (bool)this.GetValue(CanAddToQuickAccessToolBarProperty);
        set => this.SetValue(CanAddToQuickAccessToolBarProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="CanAddToQuickAccessToolBar"/> dependency property.</summary>
    public static readonly DependencyProperty CanAddToQuickAccessToolBarProperty =
        DependencyProperty.Register(nameof(CanAddToQuickAccessToolBar), typeof(bool), typeof(RibbonGroupBox), new PropertyMetadata(BooleanBoxes.TrueBox, RibbonControl.OnCanAddToQuickAccessToolBarChanged));

    /// <inheritdoc />
    public void UpdateSimplifiedState(bool isSimplified)
    {
        this.IsSimplified = isSimplified;
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
            IEnumerator baseEnumerator = base.LogicalChildren;
            while (baseEnumerator?.MoveNext() == true)
            {
                yield return baseEnumerator.Current;
            }

            if (this.Icon is not null)
            {
                yield return this.Icon;
            }

            if (this.MediumIcon is not null)
            {
                yield return this.MediumIcon;
            }

            if (this.LargeIcon is not null)
            {
                yield return this.LargeIcon;
            }

            if (this.LauncherIcon is not null)
            {
                yield return this.LauncherIcon;
            }
        }
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer() => new Wpf.Ui.Controls.Automation.Peers.RibbonGroupBoxAutomationPeer(this);
}