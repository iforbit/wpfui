// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;

using Wpf.Ui.Internal;
using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents menu item
/// </summary>
[ContentProperty(nameof(Items))]
[TemplatePart(Name = "PART_ScrollViewer", Type = typeof(ScrollViewer))]
[TemplatePart(Name = "PART_MenuPanel", Type = typeof(Panel))]
public class RibbonMenuItem : System.Windows.Controls.MenuItem, IRibbonControl, IDropDownControl, IToggleButton
{
    private Panel? menuPanel;

    private ScrollViewer? scrollViewer;

    private bool IsItemsControlMenuBase => (ItemsControlHelper.ItemsControlFromItemContainer(this) ?? VisualTreeHelper.GetParent(this)) is MenuBase;

    /// <inheritdoc />
    public RibbonControlSize Size
    {
        get => (RibbonControlSize)this.GetValue(SizeProperty);
        set => this.SetValue(SizeProperty, value);
    }

    /// <summary>Identifies the <see cref="Size"/> dependency property.</summary>
    public static readonly DependencyProperty SizeProperty = RibbonProperties.SizeProperty.AddOwner(typeof(RibbonMenuItem));

    /// <inheritdoc />
    public RibbonControlSizeDefinition SizeDefinition
    {
        get => (RibbonControlSizeDefinition)this.GetValue(SizeDefinitionProperty);
        set => this.SetValue(SizeDefinitionProperty, value);
    }

    /// <summary>Identifies the <see cref="SizeDefinition"/> dependency property.</summary>
    public static readonly DependencyProperty SizeDefinitionProperty = RibbonProperties.SizeDefinitionProperty.AddOwner(typeof(RibbonMenuItem));

    /// <inheritdoc />
    public RibbonControlSizeDefinition SimplifiedSizeDefinition
    {
        get => (RibbonControlSizeDefinition)this.GetValue(SimplifiedSizeDefinitionProperty);
        set => this.SetValue(SimplifiedSizeDefinitionProperty, value);
    }

    /// <summary>Identifies the <see cref="SimplifiedSizeDefinition"/> dependency property.</summary>
    public static readonly DependencyProperty SimplifiedSizeDefinitionProperty = RibbonProperties.SimplifiedSizeDefinitionProperty.AddOwner(typeof(RibbonMenuItem));

    /// <inheritdoc />
    public bool ShowInSimplified
    {
        get => (bool)this.GetValue(ShowInSimplifiedProperty);
        set => this.SetValue(ShowInSimplifiedProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="ShowInSimplified"/> dependency property.</summary>
    public static readonly DependencyProperty ShowInSimplifiedProperty =
        DependencyProperty.Register(nameof(ShowInSimplified), typeof(bool), typeof(RibbonMenuItem), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <inheritdoc />
    public bool IsSimplified => false; // MenuItem always returns false for simplified state

    /// <inheritdoc />
    public Popup? DropDownPopup { get; private set; }

    /// <inheritdoc />
    public bool IsContextMenuOpened { get; set; }

    /// <summary>
    /// Gets or sets useless property only used in secon level application menu items
    /// </summary>
    public string? Description
    {
        get => (string?)this.GetValue(DescriptionProperty);
        set => this.SetValue(DescriptionProperty, value);
    }

    /// <summary>Identifies the <see cref="Description"/> dependency property.</summary>
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description), typeof(string), typeof(RibbonMenuItem), new PropertyMetadata(default(string)));

    /// <inheritdoc />
    public bool IsDropDownOpen
    {
        get => this.IsSubmenuOpen;
        set => this.SetValue(IsSubmenuOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether ribbon control click must close backstage
    /// </summary>
    public bool IsDefinitive
    {
        get => (bool)this.GetValue(IsDefinitiveProperty);
        set => this.SetValue(IsDefinitiveProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsDefinitive"/> dependency property.</summary>
    public static readonly DependencyProperty IsDefinitiveProperty = DependencyProperty.Register(nameof(IsDefinitive), typeof(bool), typeof(RibbonMenuItem), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets context menu resize mode
    /// </summary>
    public ContextMenuResizeMode ResizeMode
    {
        get => (ContextMenuResizeMode)this.GetValue(ResizeModeProperty);
        set => this.SetValue(ResizeModeProperty, value);
    }

    /// <summary>Identifies the <see cref="ResizeMode"/> dependency property.</summary>
    public static readonly DependencyProperty ResizeModeProperty = DependencyProperty.Register(nameof(ResizeMode), typeof(ContextMenuResizeMode), typeof(RibbonMenuItem), new PropertyMetadata(ContextMenuResizeMode.None));

    /// <summary>
    /// Gets or sets get or sets max height of drop down popup
    /// </summary>
    public double MaxDropDownHeight
    {
        get => (double)this.GetValue(MaxDropDownHeightProperty);
        set => this.SetValue(MaxDropDownHeightProperty, value);
    }

    /// <summary>Identifies the <see cref="MaxDropDownHeight"/> dependency property.</summary>
    public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register(nameof(MaxDropDownHeight), typeof(double), typeof(RibbonMenuItem), new FrameworkPropertyMetadata(double.NaN, null, DropDownHelper.CoerceMaxDropDownHeight));

    /// <summary>
    /// Gets or sets a value indicating whether menu item is split.
    /// </summary>
    public bool IsSplit
    {
        get => (bool)this.GetValue(IsSplitProperty);
        set => this.SetValue(IsSplitProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsSplit"/> dependency property.</summary>
    public static readonly DependencyProperty IsSplitProperty = DependencyProperty.Register(nameof(IsSplit), typeof(bool), typeof(RibbonMenuItem), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <inheritdoc />
    public string? GroupName
    {
        get => (string?)this.GetValue(GroupNameProperty);
        set => this.SetValue(GroupNameProperty, value);
    }

    /// <inheritdoc />
    bool? IToggleButton.IsChecked
    {
        get => this.IsChecked;
        set => this.SetValue(IsCheckedProperty, value == true);
    }

    /// <summary>Identifies the <see cref="GroupName"/> dependency property.</summary>
    public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register(nameof(GroupName), typeof(string), typeof(RibbonMenuItem), new PropertyMetadata(ToggleButtonHelper.OnGroupNameChanged));

    /// <inheritdoc />
    public event EventHandler? DropDownOpened;

    /// <inheritdoc />
    public event EventHandler? DropDownClosed;

    /// <summary>
    /// Initializes static members of the <see cref="RibbonMenuItem"/> class.
    /// </summary>
    static RibbonMenuItem()
    {
        Type type = typeof(RibbonMenuItem);
        ToolTipService.Attach(type);

        // PopupService.Attach(type);
        // ContextMenuService.Attach(type);
        DefaultStyleKeyProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(type));
        IsCheckedProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, ToggleButtonHelper.OnIsCheckedChanged));

        IconProperty.OverrideMetadata(typeof(RibbonMenuItem), new FrameworkPropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonMenuItem"/> class.
    /// </summary>
    public RibbonMenuItem()
    {
        // ContextMenuService.Coerce(this);
    }

    /// <inheritdoc />
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        // Fix to raise MouseWhele event
        (this.Parent as ListBox)?.RaiseEvent(e);
    }

    /// <summary>Identifies the <see cref="RecognizesAccessKey"/> dependency property.</summary>
    public static readonly DependencyProperty RecognizesAccessKeyProperty = DependencyProperty.RegisterAttached(nameof(RecognizesAccessKey), typeof(bool), typeof(RibbonMenuItem), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>Helper for setting <see cref="RecognizesAccessKeyProperty"/> on <paramref name="element"/>.</summary>
    /// <param name="element"><see cref="DependencyObject"/> to set <see cref="RecognizesAccessKeyProperty"/> on.</param>
    /// <param name="value">RecognizesAccessKey property value.</param>
    public static void SetRecognizesAccessKey(DependencyObject element, bool value)
    {
        element.SetValue(RecognizesAccessKeyProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Helper for getting <see cref="RecognizesAccessKeyProperty"/> from <paramref name="element"/>.</summary>
    /// <param name="element"><see cref="DependencyObject"/> to read <see cref="RecognizesAccessKeyProperty"/> from.</param>
    /// <returns>RecognizesAccessKey property value.</returns>
    public static bool GetRecognizesAccessKey(DependencyObject element)
    {
        return (bool)element.GetValue(RecognizesAccessKeyProperty);
    }

    /// <summary>
    /// Gets or sets a value indicating whether defines if access keys should be recognized.
    /// </summary>
    public bool RecognizesAccessKey
    {
        get => (bool)this.GetValue(RecognizesAccessKeyProperty);
        set => this.SetValue(RecognizesAccessKeyProperty, BooleanBoxes.Box(value));
    }

    private bool isContextMenuOpening;
    private object? currentItem;

    /// <inheritdoc />
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        if (item is RibbonMenuItem or Separator)
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
            return new RibbonMenuItem();
        }

        var item = this.currentItem;
        this.currentItem = null;

        DataTemplate? dataTemplate = this.ItemContainerTemplateSelector.SelectTemplate(item, this);
        if (dataTemplate is not null)
        {
            return dataTemplate.LoadContent();
        }

        return new MenuItem();
    }

    /// <summary>
    /// Gets returns logical parent; either Parent or ItemsControlFromItemContainer(this).
    /// </summary>
    /// <remarks>
    /// Copied from <see cref="System.Windows.Controls.MenuItem"/>.
    /// </remarks>
    public object LogicalParent
    {
        get
        {
            if (this.Parent is not null)
            {
                return this.Parent;
            }

            return ItemsControlFromItemContainer(this);
        }
    }

    /// <inheritdoc />
    protected override void OnIsKeyboardFocusedChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnIsKeyboardFocusedChanged(e);

        // Note: IsHighlighted is a read-only property in MenuItem and is automatically managed by WPF
        // We don't need to set it manually
    }

    /// <inheritdoc />
    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);

        if (this.IsItemsControlMenuBase == false
            && this.isContextMenuOpening == false)
        {
            if (this.HasItems
                && this.LogicalParent is RibbonDropDownButton)
            {
                this.SetCurrentValue(IsSubmenuOpenProperty, true);
            }
        }
    }

    /// <inheritdoc />
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);

        if (this.IsItemsControlMenuBase == false
            && this.isContextMenuOpening == false)
        {
            // prevent too slow close on regular DropDown
            // && this.LogicalParent is ApplicationMenu == false) // prevent eager close on ApplicationMenu
            if (this.HasItems && this.LogicalParent is RibbonDropDownButton)
            {
                this.SetCurrentValue(IsSubmenuOpenProperty, false);
            }
        }
    }

    /// <inheritdoc />
    protected override void OnContextMenuOpening(ContextMenuEventArgs e)
    {
        this.isContextMenuOpening = true;

        // We have to close the sub menu as soon as the context menu gets opened
        // but only if it should be opened on ourself
        if (ReferenceEquals(this, e.Source))
        {
            this.SetCurrentValue(IsSubmenuOpenProperty, false);
        }

        base.OnContextMenuOpening(e);
    }

    /// <inheritdoc />
    protected override void OnContextMenuClosing(ContextMenuEventArgs e)
    {
        this.isContextMenuOpening = false;

        base.OnContextMenuClosing(e);
    }

    /// <inheritdoc />
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            if (this.IsSplit)
            {
                if (this.GetTemplateChild("PART_ButtonBorder") is Border buttonBorder
                    && PopupService.IsMousePhysicallyOver(buttonBorder))
                {
                    this.OnClick();
                }
            }
        }

        base.OnMouseLeftButtonUp(e);
    }

    /// <inheritdoc />
    protected override void OnClick()
    {
        // Close popup on click
        if (this.IsDefinitive
            && (!this.HasItems || this.IsSplit))
        {
            PopupService.RaiseDismissPopupEventAsync(this, DismissPopupMode.Always);
        }

        var revertIsChecked = false;

        // Rewriting everthing contained in base.OnClick causes a lot of trouble.
        // In case IsCheckable is true and GroupName is not empty we revert the value for IsChecked back to true to prevent unchecking all items in the group
        if (this.IsCheckable
            && string.IsNullOrEmpty(this.GroupName) == false)
        {
            // If checked revert the IsChecked value back to true after forwarding the click to base
            if (this.IsChecked)
            {
                revertIsChecked = true;
            }
        }

        base.OnClick();

        if (revertIsChecked)
        {
            this.RunInDispatcherAsync(() => this.SetCurrentValue(IsCheckedProperty, BooleanBoxes.TrueBox), DispatcherPriority.Background);
        }
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        if (this.DropDownPopup is not null)
        {
            this.DropDownPopup.Opened -= this.OnDropDownOpened;
            this.DropDownPopup.Closed -= this.OnDropDownClosed;
        }

        this.DropDownPopup = this.GetTemplateChild("PART_Popup") as Popup;

        if (this.DropDownPopup is not null)
        {
            this.DropDownPopup.Opened += this.OnDropDownOpened;
            this.DropDownPopup.Closed += this.OnDropDownClosed;

            KeyboardNavigation.SetControlTabNavigation(this.DropDownPopup, KeyboardNavigationMode.Cycle);
            KeyboardNavigation.SetDirectionalNavigation(this.DropDownPopup, KeyboardNavigationMode.Cycle);
            KeyboardNavigation.SetTabNavigation(this.DropDownPopup, KeyboardNavigationMode.Cycle);
        }

        this.scrollViewer = this.GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
        this.menuPanel = this.GetTemplateChild("PART_MenuPanel") as Panel;
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (this.IsSubmenuOpen)
            {
                this.SetCurrentValue(IsSubmenuOpenProperty, false);
            }
            else
            {
                this.CloseParentDropDownOrMenuItem();
            }

            e.Handled = true;
        }
        else
        {
            if (this.IsItemsControlMenuBase == false)
            {
                Key key = e.Key;

                if (this.FlowDirection == FlowDirection.RightToLeft)
                {
                    if (key == Key.Right)
                    {
                        key = Key.Left;
                    }
                    else if (key == Key.Left)
                    {
                        key = Key.Right;
                    }
                }

                if (key == Key.Right
                    && this.menuPanel is not null)
                {
                    this.SetCurrentValue(IsSubmenuOpenProperty, true);
                    _ = this.menuPanel.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                    e.Handled = true;
                }
                else if (key == Key.Left)
                {
                    if (this.IsSubmenuOpen)
                    {
                        this.SetCurrentValue(IsSubmenuOpenProperty, false);
                    }
                    else
                    {
                        System.Windows.Controls.MenuItem? parentMenuItem = UIHelper.GetParent<System.Windows.Controls.MenuItem>(this);
                        if (parentMenuItem is not null)
                        {
                            parentMenuItem.IsSubmenuOpen = false;
                        }
                    }

                    e.Handled = true;
                }

                if (e.Handled)
                {
                    return;
                }
            }

            base.OnKeyDown(e);
        }
    }

    private void CloseParentDropDownOrMenuItem()
    {
        DependencyObject? parent = UIHelper.GetParent<DependencyObject>(this, x => x is IDropDownControl || x is System.Windows.Controls.MenuItem);

        if (parent is null)
        {
            return;
        }

        if (parent is IDropDownControl dropDown)
        {
            dropDown.IsDropDownOpen = false;
        }
        else
        {
            ((System.Windows.Controls.MenuItem)parent).SetCurrentValue(IsSubmenuOpenProperty, false);
        }
    }

    // Handles drop down opened
    private void OnDropDownClosed(object? sender, EventArgs e)
    {
        this.DropDownClosed?.Invoke(this, e);
    }

    // Handles drop down closed
    private void OnDropDownOpened(object? sender, EventArgs e)
    {
        if (this.scrollViewer is not null
            && this.ResizeMode != ContextMenuResizeMode.None)
        {
            this.scrollViewer.SetCurrentValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
        }

        if (this.menuPanel is not null)
        {
            this.menuPanel.SetCurrentValue(WidthProperty, double.NaN);
            this.menuPanel.SetCurrentValue(HeightProperty, double.NaN);
        }

        this.DropDownOpened?.Invoke(this, e);
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
        }
    }
}