// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections;
using System.Diagnostics;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;

using Wpf.Ui.Controls.Helpers;
using Wpf.Ui.Extensions;
using Wpf.Ui.Internal.KnowBoxes;

using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents drop down button
/// </summary>
[ContentProperty(nameof(Items))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
[TemplatePart(Name = "PART_PopupContentControl", Type = typeof(ResizeableContentControl))]
[TemplatePart(Name = "PART_ButtonBorder", Type = typeof(UIElement))]
[DebuggerDisplay("{GetType().FullName}: Header = {Header}, Items.Count = {Items.Count}, Size = {Size}, IsSimplified = {IsSimplified}")]
public class RibbonDropDownButton : ItemsControl, IRibbonControl, IDropDownControl, ILargeIconProvider, IMediumIconProvider, ISimplifiedRibbonControl
{
    private readonly Stack<WeakReference> openMenuItems = new();

    private UIElement? buttonBorder;

    private ResizeableContentControl? popupContentControl;

    /// <summary>
    /// Gets or sets Size for the element.
    /// </summary>
    public RibbonControlSize Size
    {
        get => (RibbonControlSize)this.GetValue(SizeProperty);
        set => this.SetValue(SizeProperty, value);
    }

    /// <summary>Identifies the <see cref="Size"/> dependency property.</summary>
    public static readonly DependencyProperty SizeProperty = RibbonProperties.SizeProperty.AddOwner(typeof(RibbonDropDownButton));

    /// <inheritdoc />
    public RibbonControlSizeDefinition SizeDefinition
    {
        get => (RibbonControlSizeDefinition)this.GetValue(SizeDefinitionProperty);
        set => this.SetValue(SizeDefinitionProperty, value);
    }

    /// <summary>Identifies the <see cref="SizeDefinition"/> dependency property.</summary>
    public static readonly DependencyProperty SizeDefinitionProperty = RibbonProperties.SizeDefinitionProperty.AddOwner(typeof(RibbonDropDownButton));

    /// <inheritdoc />
    public RibbonControlSizeDefinition SimplifiedSizeDefinition
    {
        get => (RibbonControlSizeDefinition)this.GetValue(SimplifiedSizeDefinitionProperty);
        set => this.SetValue(SimplifiedSizeDefinitionProperty, value);
    }

    /// <summary>Identifies the <see cref="SimplifiedSizeDefinition"/> dependency property.</summary>
    public static readonly DependencyProperty SimplifiedSizeDefinitionProperty = RibbonProperties.SimplifiedSizeDefinitionProperty.AddOwner(typeof(RibbonDropDownButton));

    /// <inheritdoc />
    public Popup? DropDownPopup { get; private set; }

    /// <inheritdoc />
    public bool IsContextMenuOpened { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether if false: popup will not be dismissed if a mouse click occurred outside the DropDownButon's borders. <para></para>
    /// <see cref="IsDropDownOpen"/> and <see cref="ClosePopupOnMouseDown"/> will not be affected by this value.<para/>
    /// Default value is True.
    /// </summary>
    public bool DismissOnClickOutside
    {
        get => (bool)this.GetValue(DismissOnClickOutsideProperty);
        set => this.SetValue(DismissOnClickOutsideProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="DismissOnClickOutside"/> dependency property.</summary>
    public static readonly DependencyProperty DismissOnClickOutsideProperty =
        DependencyProperty.Register(nameof(DismissOnClickOutside), typeof(bool), typeof(RibbonDropDownButton), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <inheritdoc />
    public object? Header
    {
        get => this.GetValue(HeaderProperty);
        set => this.SetValue(HeaderProperty, value);
    }

    /// <summary>Identifies the <see cref="Header"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderProperty = RibbonControl.HeaderProperty.AddOwner(typeof(RibbonDropDownButton), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <inheritdoc />
    public DataTemplate? HeaderTemplate
    {
        get => (DataTemplate?)this.GetValue(HeaderTemplateProperty);
        set => this.SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>Identifies the <see cref="HeaderTemplate"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderTemplateProperty = RibbonControl.HeaderTemplateProperty.AddOwner(typeof(RibbonDropDownButton), new PropertyMetadata());

    /// <inheritdoc />
    public DataTemplateSelector? HeaderTemplateSelector
    {
        get => (DataTemplateSelector?)this.GetValue(HeaderTemplateSelectorProperty);
        set => this.SetValue(HeaderTemplateSelectorProperty, value);
    }

    /// <summary>Identifies the <see cref="HeaderTemplateSelector"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderTemplateSelectorProperty = RibbonControl.HeaderTemplateSelectorProperty.AddOwner(typeof(RibbonDropDownButton), new PropertyMetadata());

    /// <inheritdoc />
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Localizable(false)]
    public object? Icon
    {
        get => this.GetValue(IconProperty);
        set => this.SetValue(IconProperty, value);
    }

    /// <summary>Identifies the <see cref="Icon"/> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = RibbonControl.IconProperty.AddOwner(typeof(RibbonDropDownButton), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <inheritdoc />
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Localizable(false)]
    public object? LargeIcon
    {
        get => this.GetValue(LargeIconProperty);
        set => this.SetValue(LargeIconProperty, value);
    }

    /// <summary>Identifies the <see cref="LargeIcon"/> dependency property.</summary>
    public static readonly DependencyProperty LargeIconProperty = LargeIconProviderProperties.LargeIconProperty.AddOwner(typeof(RibbonDropDownButton), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <inheritdoc />
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Localizable(false)]
    public object? MediumIcon
    {
        get => this.GetValue(MediumIconProperty);
        set => this.SetValue(MediumIconProperty, value);
    }

    /// <summary>Identifies the <see cref="MediumIcon"/> dependency property.</summary>
    public static readonly DependencyProperty MediumIconProperty = MediumIconProviderProperties.MediumIconProperty.AddOwner(typeof(RibbonDropDownButton), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether button has triangle
    /// </summary>
    public bool HasTriangle
    {
        get => (bool)this.GetValue(HasTriangleProperty);
        set => this.SetValue(HasTriangleProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="HasTriangle"/> dependency property.</summary>
    public static readonly DependencyProperty HasTriangleProperty =
        DependencyProperty.Register(nameof(HasTriangle), typeof(bool), typeof(RibbonDropDownButton), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <inheritdoc />
    public bool IsDropDownOpen
    {
        get => (bool)this.GetValue(IsDropDownOpenProperty);
        set => this.SetValue(IsDropDownOpenProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsDropDownOpen"/> dependency property.</summary>
    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(RibbonDropDownButton),
            new PropertyMetadata(BooleanBoxes.FalseBox, OnIsDropDownOpenChanged));

    /// <summary>
    /// Gets or sets context menu resize mode
    /// </summary>
    public ContextMenuResizeMode ResizeMode
    {
        get => (ContextMenuResizeMode)this.GetValue(ResizeModeProperty);
        set => this.SetValue(ResizeModeProperty, value);
    }

    /// <summary>Identifies the <see cref="ResizeMode"/> dependency property.</summary>
    public static readonly DependencyProperty ResizeModeProperty =
        DependencyProperty.Register(
            nameof(ResizeMode),
            typeof(ContextMenuResizeMode),
            typeof(RibbonDropDownButton),
            new PropertyMetadata(ContextMenuResizeMode.None));

    /// <summary>
    /// Gets or sets get or sets max height of drop down popup
    /// </summary>
    public double MaxDropDownHeight
    {
        get => (double)this.GetValue(MaxDropDownHeightProperty);
        set => this.SetValue(MaxDropDownHeightProperty, value);
    }

    /// <summary>Identifies the <see cref="MaxDropDownHeight"/> dependency property.</summary>
    public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register(nameof(MaxDropDownHeight), typeof(double), typeof(RibbonDropDownButton), new FrameworkPropertyMetadata(double.NaN, null, DropDownHelper.CoerceMaxDropDownHeight));

    /// <summary>
    /// Gets or sets initial dropdown height
    /// </summary>
    public double DropDownHeight
    {
        get => (double)this.GetValue(DropDownHeightProperty);
        set => this.SetValue(DropDownHeightProperty, value);
    }

    /// <summary>Identifies the <see cref="DropDownHeight"/> dependency property.</summary>
    public static readonly DependencyProperty DropDownHeightProperty =
        DependencyProperty.Register(nameof(DropDownHeight), typeof(double), typeof(RibbonDropDownButton), new PropertyMetadata(DoubleBoxes.NaN));

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the popup of this drop down button should automatically be closed on mouse down.
    /// </summary>
    public bool ClosePopupOnMouseDown
    {
        get => (bool)this.GetValue(ClosePopupOnMouseDownProperty);
        set => this.SetValue(ClosePopupOnMouseDownProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="ClosePopupOnMouseDown"/> dependency property.</summary>
    public static readonly DependencyProperty ClosePopupOnMouseDownProperty =
        DependencyProperty.Register(nameof(ClosePopupOnMouseDown), typeof(bool), typeof(RibbonDropDownButton), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Gets or sets the delay in milliseconds to close the popup on mouse down.
    /// </summary>
    public int ClosePopupOnMouseDownDelay
    {
        get => (int)this.GetValue(ClosePopupOnMouseDownDelayProperty);
        set => this.SetValue(ClosePopupOnMouseDownDelayProperty, value);
    }

    /// <summary>Identifies the <see cref="ClosePopupOnMouseDownDelay"/> dependency property.</summary>
    public static readonly DependencyProperty ClosePopupOnMouseDownDelayProperty =
        DependencyProperty.Register(nameof(ClosePopupOnMouseDownDelay), typeof(int), typeof(RibbonDropDownButton), new PropertyMetadata(150));

    /// <summary>
    /// Gets a value indicating whether gets or sets whether or not the ribbon is in Simplified mode
    /// </summary>
    public bool IsSimplified
    {
        get => (bool)this.GetValue(IsSimplifiedProperty);
        private set => this.SetValue(IsSimplifiedPropertyKey, BooleanBoxes.Box(value));
    }

    private static readonly DependencyPropertyKey IsSimplifiedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsSimplified), typeof(bool), typeof(RibbonDropDownButton), new PropertyMetadata(BooleanBoxes.FalseBox, OnIsSimplifiedChanged));

    /// <summary>Identifies the <see cref="IsSimplified"/> dependency property.</summary>
    public static readonly DependencyProperty IsSimplifiedProperty = IsSimplifiedPropertyKey.DependencyProperty;

    private static void OnIsSimplifiedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RibbonDropDownButton dropDownButton)
        {
            dropDownButton.OnIsSimplifiedChanged((bool)e.OldValue, (bool)e.NewValue);
        }
    }

    /// <summary>
    /// Handles IsSimplified changed event for overide
    /// </summary>
    /// <param name="oldValue">old value</param>
    /// <param name="newValue">new value</param>
    protected virtual void OnIsSimplifiedChanged(bool oldValue, bool newValue)
    {
    }

    /// <inheritdoc />
    public event EventHandler? DropDownOpened;

    /// <inheritdoc />
    public event EventHandler? DropDownClosed;

    /// <summary>
    /// Initializes static members of the <see cref="RibbonDropDownButton"/> class.
    /// Static constructor
    /// </summary>
    static RibbonDropDownButton()
    {
        Type type = typeof(RibbonDropDownButton);
        DefaultStyleKeyProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(type));

        System.Windows.Controls.ToolTipService.IsEnabledProperty.OverrideMetadata(typeof(RibbonDropDownButton), new FrameworkPropertyMetadata(null, CoerceToolTipIsEnabled));

        KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
        KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));

        ToolTipService.Attach(type);
        PopupService.Attach(type);

        // ContextMenuService.Attach(type);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonDropDownButton"/> class.
    /// Default constructor
    /// </summary>
    public RibbonDropDownButton()
    {
        // ContextMenuService.Coerce(this);
        this.Loaded += this.OnLoaded;
        this.Unloaded += this.OnUnloaded;
        this.IsVisibleChanged += this.OnIsVisibleChanged;

        this.AddHandler(System.Windows.Controls.MenuItem.SubmenuOpenedEvent, new RoutedEventHandler(this.OnSubmenuOpened));
        this.AddHandler(System.Windows.Controls.MenuItem.SubmenuClosedEvent, new RoutedEventHandler(this.OnSubmenuClosed));
    }

    private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // We should better use code similar to ComboBox.OnLostMouseCapture, but most of the methods called there are internal to WPF...
        if ((bool)e.NewValue == false)
        {
            this.SetCurrentValue(IsDropDownOpenProperty, BooleanBoxes.FalseBox);
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        this.SubscribeEvents();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        this.SetCurrentValue(IsDropDownOpenProperty, false);

        this.UnSubscribeEvents();
    }

    private void SubscribeEvents()
    {
        // Always unsubscribe events to ensure we don't subscribe twice
        this.UnSubscribeEvents();

        if (this.buttonBorder is not null)
        {
            this.buttonBorder.MouseLeftButtonDown += this.HandleButtonBorderMouseLeftButtonDown;
        }

        if (this.DropDownPopup is not null)
        {
            this.DropDownPopup.KeyDown += this.OnDropDownPopupKeyDown;
            this.DropDownPopup.AddHandler(MouseDownEvent, new RoutedEventHandler(this.OnDropDownPopupMouseDown), true);
        }
    }

    private void UnSubscribeEvents()
    {
        if (this.buttonBorder is not null)
        {
            this.buttonBorder.MouseLeftButtonDown -= this.HandleButtonBorderMouseLeftButtonDown;
        }

        if (this.DropDownPopup is not null)
        {
            this.DropDownPopup.KeyDown -= this.OnDropDownPopupKeyDown;
            this.DropDownPopup.RemoveHandler(MouseDownEvent, new RoutedEventHandler(this.OnDropDownPopupMouseDown));
        }
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        this.UnSubscribeEvents();

        this.DropDownPopup = this.GetTemplateChild("PART_Popup") as Popup;

        this.popupContentControl = this.GetTemplateChild("PART_PopupContentControl") as ResizeableContentControl;

        if (this.DropDownPopup is not null)
        {
            KeyboardNavigation.SetDirectionalNavigation(this.DropDownPopup, KeyboardNavigationMode.Cycle);
            KeyboardNavigation.SetTabNavigation(this.DropDownPopup, KeyboardNavigationMode.Continue);
        }

        this.buttonBorder = this.GetTemplateChild("PART_ButtonBorder") as UIElement;

        base.OnApplyTemplate();

        this.SubscribeEvents();
    }

    /// <summary>Gets or sets the custom logic for choosing a template used to display each item.</summary>
    /// <returns>A custom object that provides logic and returns an item container.</returns>
    public ItemContainerTemplateSelector? ItemContainerTemplateSelector
    {
        get => (ItemContainerTemplateSelector?)this.GetValue(StatusBar.ItemContainerTemplateSelectorProperty);
        set => this.SetValue(StatusBar.ItemContainerTemplateSelectorProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether gets or sets a value that indicates whether the menu selects different item containers, depending on the type of the item in the underlying collection or some other heuristic.</summary>
    /// <returns>
    ///        <see langword="true" /> the menu selects different item containers; otherwise, <see langword="false" />.
    ///
    /// The registered default is <see langword="false" />. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
    public bool UsesItemContainerTemplate
    {
        get => (bool)this.GetValue(StatusBar.UsesItemContainerTemplateProperty);
        set => this.SetValue(StatusBar.UsesItemContainerTemplateProperty, value);
    }

    private object? currentItem;

    /// <inheritdoc />
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        if (base.IsItemItsOwnContainerOverride(item))
        {
            return true;
        }

        if (this.UsesItemContainerTemplate)
        {
            this.currentItem = item;
        }

        return false;
    }

    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride()
    {
        if (this.UsesItemContainerTemplate is false)
        {
            this.currentItem = null;
            return base.GetContainerForItemOverride();
        }

        var item = this.currentItem;
        this.currentItem = null;

        DataTemplate? dataTemplate = this.ItemContainerTemplateSelector?.SelectTemplate(item, this);
        if (dataTemplate is not null)
        {
            return dataTemplate.LoadContent();
        }

        return base.GetContainerForItemOverride();
    }

    private void OnDropDownPopupKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        var handled = false;

        switch (e.Key)
        {
            case Key.Escape:
                this.SetCurrentValue(IsDropDownOpenProperty, false);
                handled = true;
                break;
        }

        if (handled)
        {
            e.Handled = true;
        }
    }

    private void OnDropDownPopupMouseDown(object sender, RoutedEventArgs e)
    {
        if (this.ClosePopupOnMouseDown
            && (this.popupContentControl?.IsMouseOverResizeThumbs ?? false) == false)
        {
            // Note: get outside thread to prevent exceptions (it's a dependency property after all)
            var timespan = this.ClosePopupOnMouseDownDelay;

            if (timespan <= 0)
            {
                this.SetCurrentValue(IsDropDownOpenProperty, false);
            }
            else
            {
                // Ugly workaround, but use a timer to allow routed event to continue
                _ = Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(timespan);

                    this.RunInDispatcherAsync(() => this.SetCurrentValue(IsDropDownOpenProperty, false));
                });
            }
        }
    }

    private void HandleButtonBorderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        _ = this.Focus();
        this.SetCurrentValue(IsDropDownOpenProperty, !this.IsDropDownOpen);
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        var handled = false;

        switch (e.Key)
        {
            case Key.Down:
                // Only handle this for initial navigation. Further navigation is handled by the dropdown itself
                if (this.HasItems && this.IsDropDownOpen == false)
                {
                    this.SetCurrentValue(IsDropDownOpenProperty, true);

                    DependencyObject container = this.ItemContainerGenerator.ContainerFromIndex(0);

                    NavigateToContainer(container, FocusNavigationDirection.Down);

                    handled = true;
                }

                break;

            case Key.Up:
                // Only handle this for initial navigation. Further navigation is handled by the dropdown itself
                if (this.HasItems && this.IsDropDownOpen == false)
                {
                    this.SetCurrentValue(IsDropDownOpenProperty, true);

                    DependencyObject container = this.ItemContainerGenerator.ContainerFromIndex(this.Items.Count - 1);

                    NavigateToContainer(container, FocusNavigationDirection.Up);

                    handled = true;
                }

                break;

            case Key.Escape:
                if (this.IsDropDownOpen)
                {
                    this.SetCurrentValue(IsDropDownOpenProperty, false);
                    handled = true;
                }

                break;

            case Key.Enter:
            case Key.Space:
                this.SetCurrentValue(IsDropDownOpenProperty, !this.IsDropDownOpen);
                handled = true;
                break;
        }

        if (handled)
        {
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    internal static void NavigateToContainer(DependencyObject container, FocusNavigationDirection focusNavigationDirection = FocusNavigationDirection.Down)
    {
        var element = container as UIElement;

        if (element is null)
        {
            return;
        }

        if (element.Focusable)
        {
            _ = Keyboard.Focus(element);
        }
        else
        {
            _ = element.MoveFocus(new TraversalRequest(focusNavigationDirection));
        }
    }

    private static object CoerceToolTipIsEnabled(DependencyObject d, object basevalue)
    {
        var control = (RibbonDropDownButton)d;

        return BooleanBoxes.Box(!control.IsDropDownOpen);
    }

    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (RibbonDropDownButton)d;

        var oldValue = (bool)e.OldValue;
        var newValue = (bool)e.NewValue;

        if (newValue)
        {
            d.CoerceValue(MaxDropDownHeightProperty);
        }

        control.OnIsDropDownOpenChanged(newValue);

        (UIElementAutomationPeer.FromElement(control) as Wpf.Ui.Controls.Automation.Peers.RibbonDropDownButtonAutomationPeer)?.RaiseExpandCollapseAutomationEvent(oldValue, newValue);
    }

    private void OnIsDropDownOpenChanged(bool newValue)
    {
        // this.SetValue(System.Windows.Controls.ToolTipService.IsEnabledProperty, BooleanBoxes.Box(!newValue));
        this.SetCurrentValue(System.Windows.Controls.ToolTipService.IsEnabledProperty, BooleanBoxes.Box(!newValue));

        Debug.WriteLine($"{this.Header} IsDropDownOpen: {newValue.ToString()}");

        if (newValue)
        {
            _ = Mouse.Capture(this, CaptureMode.SubTree);

            if (this.DropDownPopup is not null)
            {
                this.RunInDispatcherAsync(
                    () =>
                    {
                        if (this.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                        {
                            this.UpdateLayout(); // 강제로 레이아웃 업데이트 시도
                        }

                        DependencyObject container = this.ItemContainerGenerator.ContainerFromIndex(0);

                        NavigateToContainer(container);

                        // Edge case: Whole dropdown content is disabled
                        if (this.IsKeyboardFocusWithin == false)
                        {
                            _ = Keyboard.Focus(this.DropDownPopup.Child);
                        }
                    });
            }

            this.OnDropDownOpened();
        }
        else
        {
            // If focus is within the subtree, make sure we have the focus so that focus isn't in the disposed hwnd
            if (this.IsKeyboardFocusWithin)
            {
                // make sure the control has focus
                _ = this.Focus();
            }

            _ = Mouse.Capture(null);

            this.OnDropDownClosed();
        }
    }

    /// <summary>
    /// Called when drop down opened.
    /// </summary>
    protected virtual void OnDropDownOpened()
    {
        this.DropDownOpened?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when drop down closed.
    /// </summary>
    protected virtual void OnDropDownClosed()
    {
        foreach (WeakReference openMenuItem in this.openMenuItems.ToArray())
        {
            if (openMenuItem.IsAlive == false)
            {
                continue;
            }

            var menuItem = (System.Windows.Controls.MenuItem?)openMenuItem.Target;
            if (menuItem?.IsSubmenuOpen == true)
            {
                menuItem.IsSubmenuOpen = false;
            }
        }

        this.openMenuItems.Clear();

        this.DropDownClosed?.Invoke(this, EventArgs.Empty);
    }

    public virtual FrameworkElement CreateQuickAccessItem()
    {
        var button = new RibbonDropDownButton
        {
            Size = RibbonControlSize.Small
        };

        this.BindQuickAccessItem(button);
        RibbonControl.Bind(this, button, nameof(this.DisplayMemberPath), DisplayMemberPathProperty, BindingMode.OneWay);
        RibbonControl.Bind(this, button, nameof(this.GroupStyleSelector), GroupStyleSelectorProperty, BindingMode.OneWay);
        RibbonControl.Bind(this, button, nameof(this.ItemContainerStyle), ItemContainerStyleProperty, BindingMode.OneWay);
        RibbonControl.Bind(this, button, nameof(this.ItemsPanel), ItemsPanelProperty, BindingMode.OneWay);
        RibbonControl.Bind(this, button, nameof(this.ItemStringFormat), ItemStringFormatProperty, BindingMode.OneWay);
        RibbonControl.Bind(this, button, nameof(this.ItemTemplate), ItemTemplateProperty, BindingMode.OneWay);

        RibbonControl.Bind(this, button, nameof(this.MaxDropDownHeight), MaxDropDownHeightProperty, BindingMode.OneWay);

        this.BindQuickAccessItemDropDownEvents(button);

        button.DropDownOpened += this.OnQuickAccessOpened;
        return button;
    }

    /// <summary>
    /// Handles quick access button drop down menu opened
    /// </summary>
    protected void OnQuickAccessOpened(object? sender, EventArgs e)
    {
        var buttonInQuickAccess = (RibbonDropDownButton?)sender;

        if (buttonInQuickAccess is null)
        {
            return;
        }

        buttonInQuickAccess.DropDownClosed += this.OnQuickAccessMenuClosedOrUnloaded;
        buttonInQuickAccess.Unloaded += this.OnQuickAccessMenuClosedOrUnloaded;

        ItemsControlHelper.MoveItemsToDifferentControl(this, buttonInQuickAccess);
    }

    /// <summary>
    /// Handles quick access button drop down menu closed
    /// </summary>
    protected void OnQuickAccessMenuClosedOrUnloaded(object? sender, EventArgs e)
    {
        var buttonInQuickAccess = (RibbonDropDownButton?)sender;

        if (buttonInQuickAccess is null)
        {
            return;
        }

        buttonInQuickAccess.DropDownClosed -= this.OnQuickAccessMenuClosedOrUnloaded;
        buttonInQuickAccess.Unloaded -= this.OnQuickAccessMenuClosedOrUnloaded;
        this.RunInDispatcherAsync(
            () =>
        {
            ItemsControlHelper.MoveItemsToDifferentControl(buttonInQuickAccess, this);
        }, DispatcherPriority.Loaded);
    }

    /// <summary>
    /// This method must be overridden to bind properties to use in quick access creating
    /// </summary>
    /// <param name="element">Toolbar item</param>
    protected virtual void BindQuickAccessItem(FrameworkElement element)
    {
        RibbonControl.BindQuickAccessItem(this, element);
        RibbonControl.Bind(this, element, nameof(this.ResizeMode), ResizeModeProperty, BindingMode.Default);
        RibbonControl.Bind(this, element, nameof(this.MaxDropDownHeight), MaxDropDownHeightProperty, BindingMode.Default);
        RibbonControl.Bind(this, element, nameof(this.HasTriangle), HasTriangleProperty, BindingMode.Default);
    }

    /// <summary>
    /// Binds the DropDownClosed and DropDownOpened events to the created quick access item
    /// </summary>
    /// <param name="button">Toolbar item</param>
    protected void BindQuickAccessItemDropDownEvents(RibbonDropDownButton button)
    {
        if (this.DropDownClosed is not null)
        {
            button.DropDownClosed += this.DropDownClosed;
        }

        if (this.DropDownOpened is not null)
        {
            button.DropDownOpened += this.DropDownOpened;
        }
    }

    public bool CanAddToQuickAccessToolBar
    {
        get => (bool)this.GetValue(CanAddToQuickAccessToolBarProperty);
        set => this.SetValue(CanAddToQuickAccessToolBarProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="CanAddToQuickAccessToolBar"/> dependency property.</summary>
    public static readonly DependencyProperty CanAddToQuickAccessToolBarProperty = RibbonControl.CanAddToQuickAccessToolBarProperty.AddOwner(typeof(RibbonDropDownButton), new PropertyMetadata(BooleanBoxes.TrueBox, RibbonControl.OnCanAddToQuickAccessToolBarChanged));

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer() => new Wpf.Ui.Controls.Automation.Peers.RibbonDropDownButtonAutomationPeer(this);

    private void OnSubmenuOpened(object sender, RoutedEventArgs e)
    {
        var menuItem = e.OriginalSource as RibbonMenuItem;
        if (menuItem is not null)
        {
            this.openMenuItems.Push(new WeakReference(menuItem));
        }
    }

    private void OnSubmenuClosed(object sender, RoutedEventArgs e)
    {
        if (this.openMenuItems.Count > 0)
        {
            _ = this.openMenuItems.Pop();
        }
    }

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

            if (this.Header is not null)
            {
                yield return this.Header;
            }
        }
    }
}