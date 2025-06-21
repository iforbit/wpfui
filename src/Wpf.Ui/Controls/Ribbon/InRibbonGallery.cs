// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Wpf.Ui.Controls.Automation.Peers;
using Wpf.Ui.Controls.Extensibility;
using Wpf.Ui.Controls.Helpers;
using Wpf.Ui.Internal;
using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents the In-Ribbon Gallery, a gallery-based control that exposes
/// a default subset of items directly in the Ribbon. Any remaining items
/// are displayed when a drop-down menu button is clicked
/// </summary>
[ContentProperty(nameof(Items))]
[TemplatePart(Name = "PART_ExpandButton", Type = typeof(ToggleButton))]
[TemplatePart(Name = "PART_DropDownButton", Type = typeof(ToggleButton))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
[TemplatePart(Name = "PART_PopupContentControl", Type = typeof(ResizeableContentControl))]
[TemplatePart(Name = "PART_FilterDropDownButton", Type = typeof(RibbonDropDownButton))]
[TemplatePart(Name = "PART_GalleryPanel", Type = typeof(GalleryPanel))]
[TemplatePart(Name = "PART_FakeImage", Type = typeof(Image))]
[TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentControl))]
[TemplatePart(Name = "PART_PopupContentPresenter", Type = typeof(ContentControl))]
[TemplatePart(Name = "PART_PopupResizeBorder", Type = typeof(FrameworkElement))]
[TemplatePart(Name = "PART_DropDownBorder", Type = typeof(Border))]
public class InRibbonGallery : Selector, IScalableRibbonControl, IDropDownControl, IRibbonControl, IRibbonSizeChangedSink, ILargeIconProvider, IMediumIconProvider, ISimplifiedRibbonControl
{
    // Freezed image (created during snapping)
    private readonly Image snappedImage = new();

    private ObservableCollection<GalleryGroupFilter>? filters;

    private RibbonToggleButton? expandButton;
    private RibbonToggleButton? dropDownButton;

    // Is visual currently snapped
    private bool isSnapped;

    private RibbonDropDownButton? groupsMenuButton;

    private GalleryPanel? galleryPanel;

    private ContentControl? controlPresenter;
    private ContentControl? popupControlPresenter;

    private bool isButtonClicked;

    private ResizeableContentControl? popupContentControl;

    internal GalleryPanelState? CurrentGalleryPanelState { get; private set; }

    /// <inheritdoc />
    public RibbonControlSize Size
    {
        get => (RibbonControlSize)this.GetValue(SizeProperty);
        set => this.SetValue(SizeProperty, value);
    }

    /// <summary>Identifies the <see cref="Size"/> dependency property.</summary>
    public static readonly DependencyProperty SizeProperty = RibbonProperties.SizeProperty.AddOwner(typeof(InRibbonGallery));

    /// <inheritdoc />
    public RibbonControlSizeDefinition SizeDefinition
    {
        get => (RibbonControlSizeDefinition)this.GetValue(SizeDefinitionProperty);
        set => this.SetValue(SizeDefinitionProperty, value);
    }

    /// <summary>Identifies the <see cref="SizeDefinition"/> dependency property.</summary>
    public static readonly DependencyProperty SizeDefinitionProperty = RibbonProperties.SizeDefinitionProperty.AddOwner(typeof(InRibbonGallery));

    /// <inheritdoc />
    public RibbonControlSizeDefinition SimplifiedSizeDefinition
    {
        get => (RibbonControlSizeDefinition)this.GetValue(SimplifiedSizeDefinitionProperty);
        set => this.SetValue(SimplifiedSizeDefinitionProperty, value);
    }

    /// <summary>Identifies the <see cref="SimplifiedSizeDefinition"/> dependency property.</summary>
    public static readonly DependencyProperty SimplifiedSizeDefinitionProperty = RibbonProperties.SimplifiedSizeDefinitionProperty.AddOwner(typeof(InRibbonGallery));

    /// <inheritdoc />
    public object? Header
    {
        get => this.GetValue(HeaderProperty);
        set => this.SetValue(HeaderProperty, value);
    }

    /// <summary>Identifies the <see cref="Header"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderProperty = RibbonControl.HeaderProperty.AddOwner(typeof(InRibbonGallery), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <inheritdoc />
    public DataTemplate? HeaderTemplate
    {
        get => (DataTemplate?)this.GetValue(HeaderTemplateProperty);
        set => this.SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>Identifies the <see cref="HeaderTemplate"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderTemplateProperty = RibbonControl.HeaderTemplateProperty.AddOwner(typeof(InRibbonGallery), new PropertyMetadata());

    /// <inheritdoc />
    public DataTemplateSelector? HeaderTemplateSelector
    {
        get => (DataTemplateSelector?)this.GetValue(HeaderTemplateSelectorProperty);
        set => this.SetValue(HeaderTemplateSelectorProperty, value);
    }

    /// <summary>Identifies the <see cref="HeaderTemplateSelector"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderTemplateSelectorProperty = RibbonControl.HeaderTemplateSelectorProperty.AddOwner(typeof(InRibbonGallery), new PropertyMetadata());

    /// <inheritdoc />
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Localizable(false)]
    public object? Icon
    {
        get => this.GetValue(IconProperty);
        set => this.SetValue(IconProperty, value);
    }

    /// <summary>Identifies the <see cref="Icon"/> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = RibbonControl.IconProperty.AddOwner(typeof(InRibbonGallery), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <summary>
    /// Gets or sets min width of the Gallery
    /// </summary>
    public int MinItemsInDropDownRow
    {
        get => (int)this.GetValue(MinItemsInDropDownRowProperty);
        set => this.SetValue(MinItemsInDropDownRowProperty, value);
    }

    /// <summary>Identifies the <see cref="MinItemsInDropDownRow"/> dependency property.</summary>
    public static readonly DependencyProperty MinItemsInDropDownRowProperty =
        DependencyProperty.Register(nameof(MinItemsInDropDownRow), typeof(int), typeof(InRibbonGallery), new PropertyMetadata(IntBoxes.One));

    /// <summary>
    /// Gets or sets max width of the Gallery
    /// </summary>
    public int MaxItemsInDropDownRow
    {
        get => (int)this.GetValue(MaxItemsInDropDownRowProperty);
        set => this.SetValue(MaxItemsInDropDownRowProperty, value);
    }

    /// <summary>Identifies the <see cref="MaxItemsInDropDownRow"/> dependency property.</summary>
    public static readonly DependencyProperty MaxItemsInDropDownRowProperty =
        DependencyProperty.Register(nameof(MaxItemsInDropDownRow), typeof(int), typeof(InRibbonGallery), new PropertyMetadata(IntBoxes.Zero));

    /// <summary>
    /// Gets or sets item width
    /// </summary>
    public double ItemWidth
    {
        get => (double)this.GetValue(ItemWidthProperty);
        set => this.SetValue(ItemWidthProperty, value);
    }

    /// <summary>Identifies the <see cref="ItemWidth"/> dependency property.</summary>
    public static readonly DependencyProperty ItemWidthProperty =
        DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(InRibbonGallery), new PropertyMetadata(DoubleBoxes.NaN));

    /// <summary>
    /// Gets or sets item height
    /// </summary>
    public double ItemHeight
    {
        get => (double)this.GetValue(ItemHeightProperty);
        set => this.SetValue(ItemHeightProperty, value);
    }

    /// <summary>Identifies the <see cref="ItemHeight"/> dependency property.</summary>
    public static readonly DependencyProperty ItemHeightProperty =
        DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(InRibbonGallery), new PropertyMetadata(DoubleBoxes.NaN));

    /// <summary>
    /// Gets or sets name of property which
    /// will use to group items in the Gallery.
    /// </summary>
    public string? GroupBy
    {
        get => (string?)this.GetValue(GroupByProperty);
        set => this.SetValue(GroupByProperty, value);
    }

    /// <summary>Identifies the <see cref="GroupBy"/> dependency property.</summary>
    public static readonly DependencyProperty GroupByProperty = DependencyProperty.Register(nameof(GroupBy), typeof(string), typeof(InRibbonGallery), new PropertyMetadata());

    /// <summary>
    /// Gets or sets name of property which
    /// will use to group items in the Gallery.
    /// </summary>
    public Func<object, string>? GroupByAdvanced
    {
        get => (Func<object, string>?)this.GetValue(GroupByAdvancedProperty);
        set => this.SetValue(GroupByAdvancedProperty, value);
    }

    /// <summary>Identifies the <see cref="GroupByAdvanced"/> dependency property.</summary>
    public static readonly DependencyProperty GroupByAdvancedProperty = DependencyProperty.Register(nameof(GroupByAdvanced), typeof(Func<object, string>), typeof(InRibbonGallery), new PropertyMetadata());

    /// <summary>
    /// Gets or sets orientation of gallery
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)this.GetValue(OrientationProperty);
        set => this.SetValue(OrientationProperty, value);
    }

    /// <summary>Identifies the <see cref="Orientation"/> dependency property.</summary>
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(InRibbonGallery), new PropertyMetadata(Orientation.Horizontal));

    /// <summary>
    /// Gets collection of filters
    /// </summary>
    public ObservableCollection<GalleryGroupFilter> Filters
    {
        get
        {
            if (this.filters is null)
            {
                this.filters = new ObservableCollection<GalleryGroupFilter>();
                this.filters.CollectionChanged += this.OnFilterCollectionChanged;
            }

            return this.filters;
        }
    }

    // Handle toolbar items changes
    private void OnFilterCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.HasFilter = this.Filters.Count > 0;
        this.InvalidateProperty(SelectedFilterProperty);

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:

                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (GalleryGroupFilter item in e.OldItems.NullSafe().OfType<GalleryGroupFilter>())
                {
                    this.groupsMenuButton?.Items.Remove(this.GetFilterMenuItem(item));
                }

                break;

            case NotifyCollectionChangedAction.Replace:
                foreach (GalleryGroupFilter item in e.OldItems.NullSafe().OfType<GalleryGroupFilter>())
                {
                    this.groupsMenuButton?.Items.Remove(this.GetFilterMenuItem(item));
                }

                break;
            case NotifyCollectionChangedAction.Reset:
                this.groupsMenuButton?.Items.Clear();
                break;
        }
    }

    /// <summary>
    /// Gets or sets selected filter
    /// </summary>
    public GalleryGroupFilter? SelectedFilter
    {
        get => (GalleryGroupFilter?)this.GetValue(SelectedFilterProperty);
        set => this.SetValue(SelectedFilterProperty, value);
    }

    /// <summary>Identifies the <see cref="SelectedFilter"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedFilterProperty =
        DependencyProperty.Register(nameof(SelectedFilter), typeof(GalleryGroupFilter), typeof(InRibbonGallery), new PropertyMetadata(null, OnSelectedFilterChanged, CoerceSelectedFilter));

    // Coerce selected filter
    private static object? CoerceSelectedFilter(DependencyObject d, object? basevalue)
    {
        var gallery = (InRibbonGallery)d;
        if (basevalue is null
            && gallery.Filters.Count > 0)
        {
            return gallery.Filters[0];
        }

        return basevalue;
    }

    // Handles filter property changed
    private static void OnSelectedFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var gallery = (InRibbonGallery)d;

        if (e.OldValue is GalleryGroupFilter oldFilter)
        {
            MenuItem? menuItem = gallery.GetFilterMenuItem(oldFilter);

            if (menuItem is not null)
            {
                menuItem.IsChecked = false;
            }
        }

        if (e.NewValue is GalleryGroupFilter newFilter)
        {
            gallery.SelectedFilterTitle = newFilter.Title;
            gallery.SelectedFilterGroups = newFilter.Groups;
            MenuItem? menuItem = gallery.GetFilterMenuItem(newFilter);

            if (menuItem is not null)
            {
                menuItem.IsChecked = true;
            }
        }
        else
        {
            gallery.SelectedFilterTitle = string.Empty;
            gallery.SelectedFilterGroups = null;
        }

        gallery.UpdateLayout();
    }

    /// <summary>
    /// Gets selected filter title
    /// </summary>
    public string? SelectedFilterTitle
    {
        get => (string?)this.GetValue(SelectedFilterTitleProperty);
        private set => this.SetValue(SelectedFilterTitlePropertyKey, value);
    }

    private static readonly DependencyPropertyKey SelectedFilterTitlePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SelectedFilterTitle), typeof(string), typeof(InRibbonGallery), new PropertyMetadata());

    /// <summary>Identifies the <see cref="SelectedFilterTitle"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedFilterTitleProperty = SelectedFilterTitlePropertyKey.DependencyProperty;

    /// <summary>
    /// Gets selected filter groups
    /// </summary>
    public string? SelectedFilterGroups
    {
        get => (string?)this.GetValue(SelectedFilterGroupsProperty);
        private set => this.SetValue(SelectedFilterGroupsPropertyKey, value);
    }

    private static readonly DependencyPropertyKey SelectedFilterGroupsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SelectedFilterGroups), typeof(string), typeof(InRibbonGallery), new PropertyMetadata());

    /// <summary>Identifies the <see cref="SelectedFilterGroups"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedFilterGroupsProperty = SelectedFilterGroupsPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets a value indicating whether gets whether gallery has selected filter
    /// </summary>
    public bool HasFilter
    {
        get => (bool)this.GetValue(HasFilterProperty);
        private set => this.SetValue(HasFilterPropertyKey, BooleanBoxes.Box(value));
    }

    private static readonly DependencyPropertyKey HasFilterPropertyKey = DependencyProperty.RegisterReadOnly(nameof(HasFilter), typeof(bool), typeof(InRibbonGallery), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>Identifies the <see cref="HasFilter"/> dependency property.</summary>
    public static readonly DependencyProperty HasFilterProperty = HasFilterPropertyKey.DependencyProperty;

    private void OnFilterMenuItemClick(object sender, RoutedEventArgs e)
    {
        var senderItem = (MenuItem)sender;
        MenuItem? item = this.GetFilterMenuItem(this.SelectedFilter);

        if (item is not null)
        {
            item.IsChecked = false;
        }

        senderItem.IsChecked = true;
        this.SetCurrentValue(SelectedFilterProperty, senderItem.Tag as GalleryGroupFilter);
        if (this.groupsMenuButton is not null)
        {
            this.groupsMenuButton.SetCurrentValue(RibbonDropDownButton.IsDropDownOpenProperty, false);
        }

        e.Handled = true;
    }

    private MenuItem? GetFilterMenuItem(GalleryGroupFilter? filter)
    {
        if (filter is null)
        {
            return null;
        }

        return this.groupsMenuButton?.Items.Cast<MenuItem>()
            .FirstOrDefault(item => item is not null && item.Header.ToString() == filter.Title);
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether gallery items can be selected
    /// </summary>
    public bool Selectable
    {
        get => (bool)this.GetValue(SelectableProperty);
        set => this.SetValue(SelectableProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="Selectable"/> dependency property.</summary>
    public static readonly DependencyProperty SelectableProperty =
        DependencyProperty.Register(
            nameof(Selectable),
            typeof(bool),
            typeof(InRibbonGallery),
            new PropertyMetadata(BooleanBoxes.TrueBox, OnSelectableChanged));

    private static void OnSelectableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.CoerceValue(SelectedItemProperty);
    }

    /// <inheritdoc />
    public Popup? DropDownPopup { get; private set; }

    /// <inheritdoc />
    public bool IsContextMenuOpened { get; set; }

    /// <inheritdoc />
    public bool IsDropDownOpen
    {
        get => (bool)this.GetValue(IsDropDownOpenProperty);
        set => this.SetValue(IsDropDownOpenProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsDropDownOpen"/> dependency property.</summary>
    public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(InRibbonGallery), new PropertyMetadata(BooleanBoxes.FalseBox, OnIsDropDownOpenChanged));

    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var inRibbonGallery = (InRibbonGallery)d;

        var newValue = (bool)e.NewValue;
        var oldValue = !newValue;

        if (newValue)
        {
            d.CoerceValue(MaxDropDownHeightProperty);
        }

        // Fire accessibility event
        if (UIElementAutomationPeer.FromElement(inRibbonGallery) is RibbonInRibbonGalleryAutomationPeer peer)
        {
            peer.RaiseExpandCollapseAutomationEvent(oldValue, newValue);
        }

        if (newValue)
        {
            inRibbonGallery.IsSnapped = true;

            if (inRibbonGallery.controlPresenter is not null)
            {
                inRibbonGallery.controlPresenter.SetCurrentValue(ContentControl.ContentProperty, inRibbonGallery.snappedImage);
            }

            if (inRibbonGallery.galleryPanel is not null)
            {
                using (new ScopeGuard(inRibbonGallery.galleryPanel.SuspendUpdates, inRibbonGallery.galleryPanel.ResumeUpdatesRefresh).Start())
                {
                    inRibbonGallery.CurrentGalleryPanelState?.Save();

                    inRibbonGallery.galleryPanel.SetCurrentValue(GalleryPanel.MinItemsInRowProperty, inRibbonGallery.MinItemsInDropDownRow);
                    inRibbonGallery.galleryPanel.SetCurrentValue(GalleryPanel.MaxItemsInRowProperty, inRibbonGallery.MaxItemsInDropDownRow);
                    inRibbonGallery.galleryPanel.SetCurrentValue(GalleryPanel.IsGroupedProperty, true);
                }
            }

            if (inRibbonGallery.popupControlPresenter is not null)
            {
                inRibbonGallery.popupControlPresenter.SetCurrentValue(ContentControl.ContentProperty, inRibbonGallery.galleryPanel);
            }

            inRibbonGallery.DropDownOpened?.Invoke(inRibbonGallery, EventArgs.Empty);

            _ = Mouse.Capture(inRibbonGallery, CaptureMode.SubTree);

            if (inRibbonGallery.DropDownPopup?.Child is not null)
            {
                inRibbonGallery.RunInDispatcherAsync(() =>
                {
                    _ = Keyboard.Focus(inRibbonGallery.DropDownPopup.Child);
                    _ = inRibbonGallery.DropDownPopup.Child.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                });
            }
        }
        else
        {
            if (inRibbonGallery.popupControlPresenter is not null)
            {
                inRibbonGallery.popupControlPresenter.SetCurrentValue(ContentControl.ContentProperty, null);
            }

            if (inRibbonGallery.galleryPanel is not null)
            {
                using (new ScopeGuard(inRibbonGallery.galleryPanel.SuspendUpdates, inRibbonGallery.galleryPanel.ResumeUpdatesRefresh).Start())
                {
                    inRibbonGallery.CurrentGalleryPanelState?.Restore();

                    inRibbonGallery.galleryPanel.SetCurrentValue(GalleryPanel.IsGroupedProperty, false);

                    inRibbonGallery.galleryPanel.ClearValue(WidthProperty);
                }
            }

            if (inRibbonGallery.IsSnapped
                && inRibbonGallery.IsFrozen == false)
            {
                inRibbonGallery.IsSnapped = false;
            }

            if (inRibbonGallery.controlPresenter is not null)
            {
                inRibbonGallery.controlPresenter.SetCurrentValue(ContentControl.ContentProperty, inRibbonGallery.galleryPanel);
            }

            inRibbonGallery.DropDownClosed?.Invoke(inRibbonGallery, EventArgs.Empty);

            inRibbonGallery.RunInDispatcherAsync(
                () =>
            {
                GalleryItem? selectedContainer = inRibbonGallery.ItemContainerGenerator.ContainerOrContainerContentFromItem<GalleryItem>(inRibbonGallery.SelectedItem);
                selectedContainer?.BringIntoView();
            }, DispatcherPriority.SystemIdle);

            // If focus is within the subtree, make sure we have the focus so that focus isn't in the disposed hwnd
            if (inRibbonGallery.IsKeyboardFocusWithin)
            {
                // make sure the inRibbonGallery has focus
                _ = inRibbonGallery.Focus();

                inRibbonGallery.RunInDispatcherAsync(
                    () =>
                {
                    GalleryItem? selectedContainer = inRibbonGallery.ItemContainerGenerator.ContainerOrContainerContentFromItem<GalleryItem>(inRibbonGallery.SelectedItem);
                    if (selectedContainer is not null)
                    {
                        _ = selectedContainer.Focus();
                    }
                    else
                    {
                        _ = inRibbonGallery.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                    }
                }, DispatcherPriority.SystemIdle);
            }

            if (Mouse.Captured == inRibbonGallery)
            {
                _ = Mouse.Capture(null);
            }
        }
    }

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
        DependencyProperty.Register(nameof(ResizeMode), typeof(ContextMenuResizeMode), typeof(InRibbonGallery), new PropertyMetadata(ContextMenuResizeMode.None));

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether InRibbonGallery
    /// </summary>
    public bool CanCollapseToButton
    {
        get => (bool)this.GetValue(CanCollapseToButtonProperty);
        set => this.SetValue(CanCollapseToButtonProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="CanCollapseToButton"/> dependency property.</summary>
    public static readonly DependencyProperty CanCollapseToButtonProperty =
        DependencyProperty.Register(nameof(CanCollapseToButton), typeof(bool), typeof(InRibbonGallery), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets a value indicating whether gets whether InRibbonGallery is collapsed to button
    /// </summary>
    public bool IsCollapsed
    {
        get => (bool)this.GetValue(IsCollapsedProperty);
        set => this.SetValue(IsCollapsedProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsCollapsed"/> dependency property.</summary>
    public static readonly DependencyProperty IsCollapsedProperty =
        DependencyProperty.Register(nameof(IsCollapsed), typeof(bool), typeof(InRibbonGallery), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <inheritdoc />
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Localizable(false)]
    public object? LargeIcon
    {
        get => this.GetValue(LargeIconProperty);
        set => this.SetValue(LargeIconProperty, value);
    }

    /// <summary>Identifies the <see cref="LargeIcon"/> dependency property.</summary>
    public static readonly DependencyProperty LargeIconProperty = LargeIconProviderProperties.LargeIconProperty.AddOwner(typeof(InRibbonGallery), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <inheritdoc />
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Localizable(false)]
    public object? MediumIcon
    {
        get => this.GetValue(MediumIconProperty);
        set => this.SetValue(MediumIconProperty, value);
    }

    /// <summary>Identifies the <see cref="MediumIcon"/> dependency property.</summary>
    public static readonly DependencyProperty MediumIconProperty = MediumIconProviderProperties.MediumIconProperty.AddOwner(typeof(InRibbonGallery), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <summary>
    /// Gets a value indicating whether snaps / Unsnaps the Visual
    /// (remove visuals and substitute with freezed image)
    /// </summary>
    public bool IsSnapped
    {
        get => this.isSnapped;

        private set
        {
            if (value == this.isSnapped)
            {
                return;
            }

            if (this.IsCollapsed)
            {
                return;
            }

            if (this.IsVisible == false)
            {
                return;
            }

            if (value
                && (int)this.ActualWidth > 0
                && (int)this.ActualHeight > 0
                && this.galleryPanel is not null
                && (int)this.galleryPanel.ActualWidth > 0
                && (int)this.galleryPanel.ActualHeight > 0)
            {
                // Render the freezed image
                RenderOptions.SetBitmapScalingMode(this.snappedImage, BitmapScalingMode.NearestNeighbor);

                var renderTargetBitmap = new RenderTargetBitmap(
                    (int)this.galleryPanel.ActualWidth,
                    (int)this.galleryPanel.ActualHeight,
                    96,
                    96,
                    PixelFormats.Pbgra32);

                renderTargetBitmap.Render(this.galleryPanel);

                this.snappedImage.SetCurrentValue(Image.SourceProperty, renderTargetBitmap);
                this.snappedImage.SetCurrentValue(FlowDirectionProperty, this.FlowDirection);
                this.snappedImage.SetCurrentValue(WidthProperty, this.galleryPanel.ActualWidth);
                this.snappedImage.SetCurrentValue(HeightProperty, this.galleryPanel.ActualHeight);
            }
            else
            {
                this.snappedImage.SetCurrentValue(Image.SourceProperty, null);
                this.snappedImage.SetCurrentValue(WidthProperty, 0D);
                this.snappedImage.SetCurrentValue(HeightProperty, 0D);
            }

            this.isSnapped = value;
        }
    }

    public bool IsFrozen { get; private set; }

    /// <summary>
    /// Gets or sets max count of items in row
    /// </summary>
    public int MaxItemsInRow
    {
        get => (int)this.GetValue(MaxItemsInRowProperty);
        set => this.SetValue(MaxItemsInRowProperty, value);
    }

    /// <summary>Identifies the <see cref="MaxItemsInRow"/> dependency property.</summary>
    public static readonly DependencyProperty MaxItemsInRowProperty =
        DependencyProperty.Register(nameof(MaxItemsInRow), typeof(int), typeof(InRibbonGallery), new PropertyMetadata(8, OnMaxItemsInRowChanged));

    private static void OnMaxItemsInRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var gal = (InRibbonGallery)d;
        var maxItemsInRow = (int)e.NewValue;

        if (gal.IsDropDownOpen == false
            && gal.galleryPanel is not null)
        {
            gal.galleryPanel.SetCurrentValue(GalleryPanel.MaxItemsInRowProperty, maxItemsInRow);
        }
    }

    /// <summary>
    /// Gets or sets min count of items in row
    /// </summary>
    public int MinItemsInRow
    {
        get => (int)this.GetValue(MinItemsInRowProperty);
        set => this.SetValue(MinItemsInRowProperty, value);
    }

    /// <summary>Identifies the <see cref="MinItemsInRow"/> dependency property.</summary>
    public static readonly DependencyProperty MinItemsInRowProperty =
        DependencyProperty.Register(nameof(MinItemsInRow), typeof(int), typeof(InRibbonGallery), new PropertyMetadata(IntBoxes.One, OnMinItemsInRowChanged));

    private static void OnMinItemsInRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var gal = (InRibbonGallery)d;
        var minItemsInRow = (int)e.NewValue;

        if (gal.IsDropDownOpen == false
            && gal.galleryPanel is not null)
        {
            gal.galleryPanel.SetCurrentValue(GalleryPanel.MinItemsInRowProperty, minItemsInRow);
        }
    }

    /// <summary>
    /// Gets or sets get or sets max height of drop down popup
    /// </summary>
    public double MaxDropDownHeight
    {
        get => (double)this.GetValue(MaxDropDownHeightProperty);
        set => this.SetValue(MaxDropDownHeightProperty, value);
    }

    /// <summary>Identifies the <see cref="MaxDropDownHeight"/> dependency property.</summary>
    public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register(nameof(MaxDropDownHeight), typeof(double), typeof(InRibbonGallery), new FrameworkPropertyMetadata(double.NaN, null, DropDownHelper.CoerceMaxDropDownHeight));

    /// <summary>
    /// Gets or sets get or sets max width of drop down popup
    /// </summary>
    public double MaxDropDownWidth
    {
        get => (double)this.GetValue(MaxDropDownWidthProperty);
        set => this.SetValue(MaxDropDownWidthProperty, value);
    }

    /// <summary>Identifies the <see cref="MaxDropDownWidth"/> dependency property.</summary>
    public static readonly DependencyProperty MaxDropDownWidthProperty =
        DependencyProperty.Register(nameof(MaxDropDownWidth), typeof(double), typeof(InRibbonGallery), new PropertyMetadata(SystemParameters.PrimaryScreenWidth / 3.0));

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
        DependencyProperty.Register(nameof(DropDownHeight), typeof(double), typeof(InRibbonGallery), new PropertyMetadata(DoubleBoxes.NaN));

    /// <summary>
    /// Gets or sets initial dropdown width
    /// </summary>
    public double DropDownWidth
    {
        get => (double)this.GetValue(DropDownWidthProperty);
        set => this.SetValue(DropDownWidthProperty, value);
    }

    /// <summary>Identifies the <see cref="DropDownWidth"/> dependency property.</summary>
    public static readonly DependencyProperty DropDownWidthProperty =
        DependencyProperty.Register(nameof(DropDownWidth), typeof(double), typeof(InRibbonGallery), new PropertyMetadata(DoubleBoxes.NaN));

    /// <summary>Identifies the <see cref="GalleryPanelContainerHeight"/> dependency property.</summary>
    public static readonly DependencyProperty GalleryPanelContainerHeightProperty = DependencyProperty.Register(nameof(GalleryPanelContainerHeight), typeof(double), typeof(InRibbonGallery), new PropertyMetadata(68D));

    /// <summary>
    /// Gets or sets the height of the container which hosts the <see cref="GalleryPanel"/>.
    /// </summary>
    public double GalleryPanelContainerHeight
    {
        get => (double)this.GetValue(GalleryPanelContainerHeightProperty);
        set => this.SetValue(GalleryPanelContainerHeightProperty, value);
    }

    /// <summary>
    /// Gets a value indicating whether gets or sets whether or not the ribbon is in Simplified mode
    /// </summary>
    public bool IsSimplified
    {
        get => (bool)this.GetValue(IsSimplifiedProperty);
        private set => this.SetValue(IsSimplifiedPropertyKey, BooleanBoxes.Box(value));
    }

    private static readonly DependencyPropertyKey IsSimplifiedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsSimplified), typeof(bool), typeof(InRibbonGallery), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>Identifies the <see cref="IsSimplified"/> dependency property.</summary>
    public static readonly DependencyProperty IsSimplifiedProperty = IsSimplifiedPropertyKey.DependencyProperty;

    /// <summary>Identifies the <see cref="ExpandButtonContent"/> dependency property.</summary>
    public static readonly DependencyProperty ExpandButtonContentProperty = DependencyProperty.Register(nameof(ExpandButtonContent), typeof(object), typeof(InRibbonGallery), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <summary>
    /// Gets or sets the content for the expand button.
    /// </summary>
    public object? ExpandButtonContent
    {
        get => (object?)this.GetValue(ExpandButtonContentProperty);
        set => this.SetValue(ExpandButtonContentProperty, value);
    }

    /// <summary>Identifies the <see cref="ExpandButtonContentTemplate"/> dependency property.</summary>
    public static readonly DependencyProperty ExpandButtonContentTemplateProperty = DependencyProperty.Register(nameof(ExpandButtonContentTemplate), typeof(DataTemplate), typeof(InRibbonGallery), new PropertyMetadata(default(DataTemplate)));

    /// <summary>
    /// Gets or sets the content template for the expand button.
    /// </summary>
    public DataTemplate? ExpandButtonContentTemplate
    {
        get => (DataTemplate?)this.GetValue(ExpandButtonContentTemplateProperty);
        set => this.SetValue(ExpandButtonContentTemplateProperty, value);
    }

    /// <inheritdoc />
    public event EventHandler? Scaled;

    /// <inheritdoc />
    public event EventHandler? DropDownOpened;

    /// <inheritdoc />
    public event EventHandler? DropDownClosed;

    /// <summary>
    /// Initializes static members of the <see cref="InRibbonGallery"/> class.
    /// </summary>
    static InRibbonGallery()
    {
        Type type = typeof(InRibbonGallery);

        DefaultStyleKeyProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(type));
        SelectedItemProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(null, CoerceSelectedItem));

        // ToolTipService.Attach(type);
        PopupService.Attach(type);

        // ContextMenuService.Attach(type);
    }

    // Coerce selected item
    private static object? CoerceSelectedItem(DependencyObject d, object? basevalue)
    {
        var gallery = (InRibbonGallery)d;

        if (gallery.Selectable == false)
        {
            GalleryItem? galleryItem = gallery.ItemContainerGenerator.ContainerOrContainerContentFromItem<GalleryItem>(basevalue);
            if (basevalue is not null
                && galleryItem is not null)
            {
                galleryItem.IsSelected = false;
            }

            return null;
        }

        return basevalue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InRibbonGallery"/> class.
    /// Default constructor
    /// </summary>
    public InRibbonGallery()
    {
        // ContextMenuService.Coerce(this);
        this.Unloaded += this.OnUnloaded;

        SelectorHelper.SetCanSelectMultiple(this, false);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        this.SetCurrentValue(IsDropDownOpenProperty, false);
    }

    /// <inheritdoc />
    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (e.Handled)
        {
            return;
        }

        if (e.Key == Key.F4
            && (e.KeyboardDevice.Modifiers & ModifierKeys.Alt) == 0)
        {
            this.SetCurrentValue(IsDropDownOpenProperty, !this.IsDropDownOpen);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape
                 && this.IsDropDownOpen)
        {
            this.SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        if ((!AutomationPeer.ListenerExists(AutomationEvents.SelectionPatternOnInvalidated)
             && !AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected)
             && (!AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementAddedToSelection)
                 && !AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection)))
            || UIElementAutomationPeer.CreatePeerForElement(this) is not RibbonInRibbonGalleryAutomationPeer peerForElement)
        {
            return;
        }

        peerForElement.RaiseSelectionEvents(e);
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        if (this.expandButton is not null)
        {
            this.expandButton.Click -= this.OnExpandClick;
        }

        this.expandButton = (RibbonToggleButton?)(this.GetTemplateChild("PART_ExpandButton") as ToggleButton);

        if (this.expandButton is not null)
        {
            this.expandButton.Click += this.OnExpandClick;
        }

        this.dropDownButton = (RibbonToggleButton?)(this.GetTemplateChild("PART_DropDownButton") as ToggleButton);
        if (this.dropDownButton is ISimplifiedStateControl control)
        {
            control.UpdateSimplifiedState(this.IsSimplified);
        }

        if (this.DropDownPopup is not null)
        {
            this.DropDownPopup.PreviewMouseLeftButtonUp -= this.OnPopupPreviewMouseUp;
            this.DropDownPopup.PreviewMouseLeftButtonDown -= this.OnPopupPreviewMouseDown;
        }

        this.DropDownPopup = this.GetTemplateChild("PART_Popup") as Popup;

        if (this.DropDownPopup is not null)
        {
            this.DropDownPopup.PreviewMouseLeftButtonUp += this.OnPopupPreviewMouseUp;
            this.DropDownPopup.PreviewMouseLeftButtonDown += this.OnPopupPreviewMouseDown;

            KeyboardNavigation.SetControlTabNavigation(this.DropDownPopup, KeyboardNavigationMode.Cycle);
            KeyboardNavigation.SetDirectionalNavigation(this.DropDownPopup, KeyboardNavigationMode.Cycle);
            KeyboardNavigation.SetTabNavigation(this.DropDownPopup, KeyboardNavigationMode.Cycle);
        }

        this.popupContentControl = this.GetTemplateChild("PART_PopupContentControl") as ResizeableContentControl;

        this.groupsMenuButton?.Items.Clear();

        this.groupsMenuButton = this.GetTemplateChild("PART_FilterDropDownButton") as RibbonDropDownButton;

        this.galleryPanel = this.GetTemplateChild("PART_GalleryPanel") as GalleryPanel;

        if (this.galleryPanel is not null)
        {
            using (new ScopeGuard(this.galleryPanel.SuspendUpdates, this.galleryPanel.ResumeUpdates).Start())
            {
                this.galleryPanel.SetCurrentValue(GalleryPanel.MinItemsInRowProperty, this.MinItemsInRow);
                this.galleryPanel.SetCurrentValue(GalleryPanel.MaxItemsInRowProperty, this.MaxItemsInRow);
            }

            this.CurrentGalleryPanelState = new GalleryPanelState(this.galleryPanel);
        }
        else
        {
            this.CurrentGalleryPanelState = null;
        }

        this.controlPresenter = this.GetTemplateChild("PART_ContentPresenter") as ContentControl;
        this.popupControlPresenter = this.GetTemplateChild("PART_PopupContentPresenter") as ContentControl;
    }

    private void OnPopupPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        // Ignore mouse up when mouse donw is on expand button
        if (this.isButtonClicked)
        {
            this.isButtonClicked = false;
            e.Handled = true;
        }
    }

    private void OnPopupPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        this.isButtonClicked = false;
    }

    private void OnExpandClick(object sender, RoutedEventArgs e)
    {
        this.isButtonClicked = true;
    }

    /// <inheritdoc />
    public void OnSizePropertyChanged(RibbonControlSize previous, RibbonControlSize current)
    {
        if (this.CanAutomaticallyChangeIsCollapsed())
        {
            if (current == RibbonControlSize.Large
                && this.galleryPanel?.MinItemsInRow > this.MinItemsInRow)
            {
                this.SetCurrentValue(IsCollapsedProperty, BooleanBoxes.FalseBox);
            }
            else
            {
                this.SetCurrentValue(IsCollapsedProperty, BooleanBoxes.TrueBox);
            }
        }
    }

    private bool CanAutomaticallyChangeIsCollapsed()
    {
        if (this.CanCollapseToButton is false)
        {
            return false;
        }

        ValueSource valueSource = DependencyPropertyHelper.GetValueSource(this, IsCollapsedProperty);
        return valueSource.BaseValueSource is BaseValueSource.Default
            || (valueSource.BaseValueSource is BaseValueSource.Local && valueSource.IsCurrent);
    }

    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new GalleryItem();
    }

    /// <inheritdoc />
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is GalleryItem;
    }

    /// <inheritdoc />
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);

        // We don't want to notify scaling when items are moved to a different control.
        // This prevents excessive cache invalidation.
        if (ItemsControlHelper.GetIsMovingItemsToDifferentControl(this) == false)
        {
            this.Scaled?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            this.SetCurrentValue(IsDropDownOpenProperty, false);
        }

        base.OnKeyDown(e);
    }

    private void Freeze()
    {
        this.IsSnapped = true;
        this.IsFrozen = true;

        if (this.controlPresenter is not null)
        {
            this.controlPresenter.SetCurrentValue(ContentControl.ContentProperty, this.snappedImage);
        }

        // Move items and selected item
        _ = this.SelectedItem;
        this.SetCurrentValue(SelectedItemProperty, null);
    }

    /// <inheritdoc />
    public void ResetScale()
    {
        if (this.CanAutomaticallyChangeIsCollapsed()
            && RibbonProperties.GetSize(this) == RibbonControlSize.Large)
        {
            this.ClearValue(IsCollapsedProperty);
        }

        if (this.galleryPanel is not null
            && this.galleryPanel.MaxItemsInRow < this.MaxItemsInRow)
        {
            this.galleryPanel.SetCurrentValue(GalleryPanel.MaxItemsInRowProperty, this.MaxItemsInRow);
        }

        this.InvalidateMeasure();
    }

    /// <inheritdoc />
    public void Enlarge()
    {
        if (this.CanAutomaticallyChangeIsCollapsed()
            && this.IsCollapsed
            && RibbonProperties.GetSize(this) == RibbonControlSize.Large)
        {
            this.SetCurrentValue(IsCollapsedProperty, BooleanBoxes.FalseBox);
        }
        else if (this.galleryPanel is not null
                 && this.galleryPanel.MaxItemsInRow < this.MaxItemsInRow)
        {
            this.galleryPanel.SetCurrentValue(GalleryPanel.MaxItemsInRowProperty, Math.Min(this.galleryPanel.MaxItemsInRow + 1, this.MaxItemsInRow));
        }
        else
        {
            return;
        }

        this.InvalidateMeasure();

        this.Scaled?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void Reduce()
    {
        if (this.galleryPanel is not null
            && this.galleryPanel.MaxItemsInRow > this.MinItemsInRow)
        {
            this.galleryPanel.SetCurrentValue(GalleryPanel.MaxItemsInRowProperty, Math.Max(this.galleryPanel.MaxItemsInRow - 1, 0));
        }
        else if (this.CanAutomaticallyChangeIsCollapsed()
                 && this.IsCollapsed == false)
        {
            this.SetCurrentValue(IsCollapsedProperty, BooleanBoxes.TrueBox);
        }
        else
        {
            return;
        }

        this.InvalidateMeasure();

        this.Scaled?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void UpdateSimplifiedState(bool isSimplified)
    {
        this.IsSimplified = isSimplified;
        if (this.dropDownButton is ISimplifiedStateControl control)
        {
            control.UpdateSimplifiedState(isSimplified);
        }
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

            if (this.ExpandButtonContent is not null)
            {
                yield return this.ExpandButtonContent;
            }
        }
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer() => new RibbonInRibbonGalleryAutomationPeer(this);

    internal class GalleryPanelState
    {
        public GalleryPanelState(GalleryPanel galleryPanel)
        {
            this.GalleryPanel = galleryPanel;
            this.Save();
        }

        public GalleryPanel GalleryPanel { get; }

        public int MinItemsInRow { get; private set; }

        public int MaxItemsInRow { get; private set; }

        public void Save()
        {
            this.MinItemsInRow = this.GalleryPanel.MinItemsInRow;
            this.MaxItemsInRow = this.GalleryPanel.MaxItemsInRow;
        }

        public void Restore()
        {
            this.GalleryPanel.SetCurrentValue(GalleryPanel.MinItemsInRowProperty, this.MinItemsInRow);
            this.GalleryPanel.SetCurrentValue(GalleryPanel.MaxItemsInRowProperty, this.MaxItemsInRow);
        }
    }

    /// <summary>
    /// Causes the object to scroll into view.  If it is not visible, it is aligned either at the top or bottom of the viewport.
    /// </summary>
    public void ScrollIntoView(object item)
    {
        if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
        {
            _ = this.OnBringItemIntoView(item);
        }
        else
        {
            // The items aren't generated, try at a later time
            _ = this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(this.OnBringItemIntoView), item);
        }
    }

    private object? OnBringItemIntoView(object item)
    {
        GalleryItem? selectedContainer = this.ItemContainerGenerator.ContainerOrContainerContentFromItem<GalleryItem>(item);
        selectedContainer?.BringIntoView();
        return null;
    }
}