// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using Wpf.Ui.Internal;
using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents title bar
/// </summary>
[StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(RibbonContextualTabGroup))]
[TemplatePart(Name = "PART_HeaderHolder", Type = typeof(FrameworkElement))]
[TemplatePart(Name = "PART_ItemsContainer", Type = typeof(Panel))]
public class RibbonTitleBar : HeaderedItemsControl
{
    // Header holder
    private FrameworkElement? headerHolder;

    // Items container
    private Panel? itemsContainer;

    // Header rect
    private Rect headerRect = new(0, 0, 0, 0);

    // Items rect
    private Rect itemsRect = new(0, 0, 0, 0);

    private Size lastMeasureConstraint;

    /// <summary>
    /// Gets or sets header alignment
    /// </summary>
    public HorizontalAlignment HeaderAlignment
    {
        get => (HorizontalAlignment)this.GetValue(HeaderAlignmentProperty);
        set => this.SetValue(HeaderAlignmentProperty, value);
    }

    /// <summary>Identifies the <see cref="HeaderAlignment"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderAlignmentProperty =
        DependencyProperty.Register(nameof(HeaderAlignment), typeof(HorizontalAlignment), typeof(RibbonTitleBar), new FrameworkPropertyMetadata(HorizontalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// Gets or sets a value indicating whether defines whether title bar is collapsed
    /// </summary>
    public bool IsCollapsed
    {
        get => (bool)this.GetValue(IsCollapsedProperty);
        set => this.SetValue(IsCollapsedProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsCollapsed"/> dependency property.</summary>
    public static readonly DependencyProperty IsCollapsedProperty =
        DependencyProperty.Register(nameof(IsCollapsed), typeof(bool), typeof(RibbonTitleBar), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

    private bool isAtLeastOneRequiredControlPresent;

    /// <summary>Identifies the <see cref="HideContextTabs"/> dependency property.</summary>
    public static readonly DependencyProperty HideContextTabsProperty =
        DependencyProperty.Register(nameof(HideContextTabs), typeof(bool), typeof(RibbonTitleBar), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    ///  Gets or sets a value indicating whether gets or sets whether context tabs are hidden.
    /// </summary>
    public bool HideContextTabs
    {
        get => (bool)this.GetValue(HideContextTabsProperty);
        set => this.SetValue(HideContextTabsProperty, BooleanBoxes.Box(value));
    }

    /// <summary>
    /// Initializes static members of the <see cref="RibbonTitleBar"/> class.
    /// Static constructor
    /// </summary>
    static RibbonTitleBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonTitleBar), new FrameworkPropertyMetadata(typeof(RibbonTitleBar)));

        HeaderProperty.OverrideMetadata(typeof(RibbonTitleBar), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
    }

    /// <inheritdoc />
    protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
    {
        HitTestResult? baseResult = base.HitTestCore(hitTestParameters);

        if (baseResult is null)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        return baseResult;
    }

    /// <inheritdoc />
    protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseRightButtonUp(e);

        if (e.Handled
            || this.IsMouseDirectlyOver == false)
        {
            return;
        }

        WindowSteeringHelper.ShowSystemMenu(this, e);
    }

    /// <inheritdoc />
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (e.Handled)
        {
            return;
        }

        // Contextual groups shall handle mouse events
        if (e.Source is RibbonContextualGroupsContainer or RibbonContextualTabGroup)
        {
            return;
        }

        WindowSteeringHelper.HandleMouseLeftButtonDown(e, true, true);
    }

    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new RibbonContextualTabGroup();
    }

    /// <inheritdoc />
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is RibbonContextualTabGroup;
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        this.headerHolder = this.GetTemplateChild("PART_HeaderHolder") as FrameworkElement;
        this.itemsContainer = this.GetTemplateChild("PART_ItemsContainer") as Panel;

        this.isAtLeastOneRequiredControlPresent = this.headerHolder is not null
                                                  || this.itemsContainer is not null;
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size constraint)
    {
        if (this.isAtLeastOneRequiredControlPresent == false)
        {
            return base.MeasureOverride(constraint);
        }

        this.lastMeasureConstraint = constraint;

        Size resultSize = constraint;

        if (double.IsPositiveInfinity(resultSize.Width)
            || double.IsPositiveInfinity(resultSize.Height))
        {
            resultSize = base.MeasureOverride(resultSize);
        }

        this.Update(resultSize);

        this.itemsContainer?.Measure(this.itemsRect.Size);
        this.headerHolder?.Measure(this.headerRect.Size);

        var maxHeight = Math.Max(this.itemsRect.Height, this.headerRect.Height);
        var width = this.headerRect.Width + this.itemsRect.Width;

        return new Size(width, maxHeight);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        if (this.isAtLeastOneRequiredControlPresent == false)
        {
            return base.ArrangeOverride(arrangeBounds);
        }

        // If the last measure constraint and the arrangeBounds are not equal we have to update again.
        // This can happen if the window is set to auto-size it's width.
        // As Update also does some things that are related to an arrange pass we have to update again.
        // It would be way better if Update wouldn't handle parts of the arrange pass, but that would be very difficult to implement...
        if (arrangeBounds.Equals(this.lastMeasureConstraint) == false)
        {
            this.Update(arrangeBounds);

            this.itemsContainer?.Measure(this.itemsRect.Size);
            this.headerHolder?.Measure(this.headerRect.Size);
        }

        this.itemsContainer?.Arrange(this.itemsRect);
        this.headerHolder?.Arrange(this.headerRect);

        this.EnsureCorrectLayoutAfterArrange();

        return arrangeBounds;
    }

    /// <summary>
    /// Sometimes the relative position only changes after the arrange phase.
    /// To compensate such sitiations we issue a second layout pass by invalidating our measure.
    /// This situation can occur if, for example, the icon of a ribbon window has it's visibility changed.
    /// </summary>
    private void EnsureCorrectLayoutAfterArrange()
    {
        Point currentRelativePosition = this.GetCurrentRelativePosition();
        this.RunInDispatcherAsync(() => this.CheckPosition(currentRelativePosition, this.GetCurrentRelativePosition()));
    }

    private void CheckPosition(Point previousRelativePosition, Point currentRelativePositon)
    {
        if (previousRelativePosition != currentRelativePositon)
        {
            this.InvalidateMeasure();
        }
    }

    private Point GetCurrentRelativePosition()
    {
        var parentUIElement = this.Parent as UIElement;

        if (parentUIElement is null)
        {
            return default;
        }

        return this.TranslatePoint(default, parentUIElement);
    }

    // Update items size and positions
    private void Update(Size constraint)
    {
        RibbonContextualTabGroup[] visibleGroups = this.HideContextTabs
            ? Array.Empty<RibbonContextualTabGroup>()
            : this.Items.OfType<RibbonContextualTabGroup>()
                .Where(group => group.InnerVisibility == Visibility.Visible && group.Items.Count > 0)
                .ToArray();

        var canRibbonTabControlScroll = false;

        // Defensively try to find out if the RibbonTabControl can scroll
        if (visibleGroups.Length > 0)
        {
            RibbonTabItem? firstVisibleItem = visibleGroups.First().FirstVisibleItem;

            canRibbonTabControlScroll = UIHelper.GetParent<RibbonTabControl>(firstVisibleItem)?.CanScroll == true;
        }

        if (this.IsCollapsed)
        {
            // Collapse itemRect
            this.itemsRect = new Rect(0, 0, 0, 0);

            this.headerHolder?.Measure(new Size(constraint.Width, constraint.Height));

            var allTextWidth = constraint.Width;
            const int left = 0;
            var headerHolderWidth = this.headerHolder?.DesiredSize.Width ?? default;

            this.headerRect = this.GetHeaderRect(constraint, left, allTextWidth, headerHolderWidth);
        }
        else if (visibleGroups.Length is 0
                 || canRibbonTabControlScroll)
        {
            // Collapse itemRect
            this.itemsRect = new Rect(0, 0, 0, 0);

            // Set header position and size
            if (this.headerHolder is not null)
            {
                this.headerHolder.Measure(SizeConstants.Infinite);

                var left = 0;
                var allTextWidth = constraint.Width;
                var headerHolderWidth = this.headerHolder.DesiredSize.Width;

                this.headerRect = this.GetHeaderRect(constraint, left, allTextWidth, headerHolderWidth);
            }
            else
            {
                this.headerRect = new Rect(0, 0, constraint.Width, constraint.Height);
            }
        }
        else
        {
            var pointZero = default(Point);

            // get initial StartX value
            var startX = visibleGroups.First().FirstVisibleItem?.TranslatePoint(pointZero, this).X ?? 0;
            var endX = 0D;

            // Get minimum x point (workaround)
            foreach (RibbonContextualTabGroup? group in visibleGroups)
            {
                var currentStartX = group.FirstVisibleItem?.TranslatePoint(pointZero, this).X ?? 0;

                if (currentStartX < startX)
                {
                    startX = currentStartX;
                }

                RibbonTabItem? lastItem = group.LastVisibleItem;
                var currentEndX = lastItem?.TranslatePoint(new Point(lastItem.DesiredSize.Width, 0), this).X ?? 0;

                if (currentEndX > endX)
                {
                    endX = currentEndX;
                }
            }

            // Ensure that startX and endX are never negative
            startX = Math.Max(0, startX);
            endX = Math.Max(0, endX);

            // Set contextual groups position and size
            this.itemsContainer?.Measure(SizeConstants.Infinite);
            var itemsRectWidth = Math.Min(this.itemsContainer?.DesiredSize.Width ?? default, Math.Max(0, Math.Min(endX, constraint.Width) - startX));
            this.itemsRect = new Rect(startX, 0, itemsRectWidth, constraint.Height);

            // Set header
            this.headerHolder?.Measure(SizeConstants.Infinite);

            switch (this.HeaderAlignment)
            {
                case HorizontalAlignment.Left when this.headerHolder is not null:
                {
                    if (startX > 150)
                    {
                        var allTextWidth = startX;
                        this.headerRect = new Rect(0, 0, Math.Min(allTextWidth, this.headerHolder.DesiredSize.Width), constraint.Height);
                    }
                    else
                    {
                        var allTextWidth = Math.Max(0, constraint.Width - endX);
                        this.headerRect = new Rect(Math.Min(endX, constraint.Width), 0, Math.Min(allTextWidth, this.headerHolder.DesiredSize.Width), constraint.Height);
                    }
                }

                break;

                case HorizontalAlignment.Center when this.headerHolder is not null:
                {
                    var allTextWidthRight = Math.Max(0, constraint.Width - endX);
                    var allTextWidthLeft = Math.Max(0, startX);
                    var fitsRightButNotLeft = allTextWidthRight >= this.headerHolder.DesiredSize.Width && allTextWidthLeft < this.headerHolder.DesiredSize.Width;

                    if (((startX < 150 || fitsRightButNotLeft) && (startX > 0) && (startX < constraint.Width - endX)) || (endX < constraint.Width / 2))
                    {
                        this.headerRect = new Rect(Math.Min(Math.Max(endX, (constraint.Width / 2) - (this.headerHolder.DesiredSize.Width / 2)), constraint.Width), 0, Math.Min(allTextWidthRight, this.headerHolder.DesiredSize.Width), constraint.Height);
                    }
                    else
                    {
                        this.headerRect = new Rect(Math.Max(0, (allTextWidthLeft / 2) - (this.headerHolder.DesiredSize.Width / 2)), 0, Math.Min(allTextWidthLeft, this.headerHolder.DesiredSize.Width), constraint.Height);
                    }
                }

                break;

                case HorizontalAlignment.Right when this.headerHolder is not null:
                {
                    if (startX > 150)
                    {
                        var allTextWidth = Math.Max(0, startX);
                        this.headerRect = new Rect(Math.Max(0, allTextWidth - this.headerHolder.DesiredSize.Width), 0, Math.Min(allTextWidth, this.headerHolder.DesiredSize.Width), constraint.Height);
                    }
                    else
                    {
                        var allTextWidth = Math.Max(0, constraint.Width - endX);
                        this.headerRect = new Rect(Math.Min(Math.Max(endX, constraint.Width - this.headerHolder.DesiredSize.Width), constraint.Width), 0, Math.Min(allTextWidth, this.headerHolder.DesiredSize.Width), constraint.Height);
                    }
                }

                break;

                case HorizontalAlignment.Stretch:
                {
                    if (startX > 150)
                    {
                        var allTextWidth = startX;
                        this.headerRect = new Rect(0, 0, allTextWidth, constraint.Height);
                    }
                    else
                    {
                        var allTextWidth = Math.Max(0, constraint.Width - endX);
                        this.headerRect = new Rect(Math.Min(endX, constraint.Width), 0, allTextWidth, constraint.Height);
                    }
                }

                break;
            }
        }

        this.headerRect.Width += 2;
    }

    private Rect GetHeaderRect(Size constraint, double left, double allTextWidth, double headerHolderWidth)
    {
        return this.HeaderAlignment switch
        {
            HorizontalAlignment.Left => new Rect(left, 0, Math.Min(allTextWidth, headerHolderWidth), constraint.Height),
            HorizontalAlignment.Center => new Rect(left + Math.Max(0, (allTextWidth / 2) - (headerHolderWidth / 2)), 0, Math.Min(allTextWidth, headerHolderWidth), constraint.Height),
            HorizontalAlignment.Right => new Rect(left + Math.Max(0, allTextWidth - headerHolderWidth), 0, Math.Min(allTextWidth, headerHolderWidth), constraint.Height),
            HorizontalAlignment.Stretch => new Rect(left, 0, allTextWidth, constraint.Height),
            _ => Rect.Empty
        };
    }

    private DispatcherOperation? forceMeasureAndArrangeOperation;

    /// <summary>
    /// Schedules a call to <see cref="FrameworkElementExtensions.ForceMeasureAndArrangeImmediate"/>.
    /// </summary>
    public void ScheduleForceMeasureAndArrange()
    {
        if (this.forceMeasureAndArrangeOperation is not null)
        {
            return;
        }

        this.forceMeasureAndArrangeOperation = this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(this.PrivateForceMeasureAndArrange));
    }

    private void PrivateForceMeasureAndArrange()
    {
        this.forceMeasureAndArrangeOperation = null;
        this.ForceMeasureAndArrangeImmediate();
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer() => new RibbonTitleBarAutomationPeer(this);
}