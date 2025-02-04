// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Wpf.Ui.Extensions;
using Wpf.Ui.Internal;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represent panel with ribbon group.
/// It is automatically adjusting size of controls
/// </summary>
public class RibbonGroupsContainer : Panel, IScrollInfo
{
    private readonly struct MeasureCache
    {
        public static readonly MeasureCache Empty = new MeasureCache(Size.Empty, Size.Empty);

        public MeasureCache( Size availableSize, Size desiredSize )
        {
            AvailableSize = availableSize;
            DesiredSize = desiredSize;
        }

        public Size AvailableSize { get; }

        public Size DesiredSize { get; }

        public bool IsEmpty => AvailableSize.IsEmpty;
    }

    private MeasureCache measureCache;

    /// <summary>
    /// Gets or sets reduce order of group in the ribbon panel.
    /// It must be enumerated with comma from the first to reduce to
    /// the last to reduce (use Control.Name as group name in the enum).
    /// Enclose in parentheses as (Control.Name) to reduce/enlarge
    /// scalable elements in the given group
    /// </summary>
    public string? ReduceOrder
    {
        get => (string?)GetValue(ReduceOrderProperty);
        set => SetValue(ReduceOrderProperty, value);
    }

    /// <summary>Identifies the <see cref="ReduceOrder"/> dependency property.</summary>
    public static readonly DependencyProperty ReduceOrderProperty =
        DependencyProperty.Register(nameof(ReduceOrder), typeof(string), typeof(RibbonGroupsContainer), new PropertyMetadata(OnReduceOrderChanged));

    // handles ReduseOrder property changed
    private static void OnReduceOrderChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        var ribbonPanel = (RibbonGroupsContainer)d;

        var toIncrease = ribbonPanel.reduceOrder.Skip(ribbonPanel.reduceOrderIndex).ToArray();

        foreach (var reduceOrderItem in toIncrease)
        {
            ribbonPanel.IncreaseGroupBoxSize(reduceOrderItem);
        }

        ribbonPanel.reduceOrder = (((string?)e.NewValue) ?? string.Empty).Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var newReduceOrderIndex = ribbonPanel.reduceOrder.Length - 1;
        ribbonPanel.reduceOrderIndex = newReduceOrderIndex;

        ribbonPanel.InvalidateMeasureAndArrange();
    }

    private string[] reduceOrder = new string[0];
    private int reduceOrderIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonGroupsContainer"/> class.
    /// Default constructor
    /// </summary>
    public RibbonGroupsContainer()
    {
        Focusable = false;
    }

    /// <inheritdoc />
    protected override UIElementCollection CreateUIElementCollection( FrameworkElement logicalParent )
    {
        return new UIElementCollection(this, /*Parent as FrameworkElement*/this);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride( Size availableSize )
    {
        // System.Diagnostics.Trace.WriteLine($"MeasureOverride {availableSize}");
        Size desiredSize = GetChildrenDesiredSizeIntermediate();

        if (reduceOrder.Length == 0

            // Check cached measure to prevent "flicker"
            || (measureCache.AvailableSize == availableSize && measureCache.DesiredSize == desiredSize))
        {
            VerifyScrollData(availableSize.Width, desiredSize.Width);
            return desiredSize;
        }

        // If we have more available space - try to expand groups
        while (desiredSize.Width <= availableSize.Width)
        {
            var hasMoreVariants = reduceOrderIndex < reduceOrder.Length - 1;
            if (hasMoreVariants == false)
            {
                break;
            }

            // Increase size of another item
            reduceOrderIndex++;
            IncreaseGroupBoxSize(reduceOrder[reduceOrderIndex]);

            desiredSize = GetChildrenDesiredSizeIntermediate();
        }

        // If not enough space - go to next variant
        while (desiredSize.Width > availableSize.Width)
        {
            var hasMoreVariants = reduceOrderIndex >= 0;
            if (hasMoreVariants == false)
            {
                break;
            }

            // Decrease size of another item
            DecreaseGroupBoxSize(reduceOrder[reduceOrderIndex]);
            reduceOrderIndex--;

            desiredSize = GetChildrenDesiredSizeIntermediate();
        }

        measureCache = new MeasureCache(availableSize, desiredSize);

        VerifyScrollData(availableSize.Width, desiredSize.Width);

        return desiredSize;
    }

    private Size GetChildrenDesiredSizeIntermediate()
    {
        double width = 0;
        double height = 0;

        foreach (UIElement? child in InternalChildren)
        {
            var groupBox = child as RibbonGroupBox;
            if (groupBox is null)
            {
                continue;
            }

            Size desiredSize = groupBox.GetDesiredSizeIntermediate();
            width += desiredSize.Width;
            height = Math.Max(height, desiredSize.Height);
        }

        return new Size(width, height);
    }

    // Increase size of the item
    private void IncreaseGroupBoxSize( string name )
    {
        RibbonGroupBox? groupBox = FindGroup(name);
        var scale = name.StartsWith("(", StringComparison.Ordinal);

        if (groupBox is null)
        {
            return;
        }

        if (scale)
        {
            groupBox.ScaleIntermediate++;
        }
        else
        {
            if (groupBox.IsSimplified)
            {
                groupBox.StateIntermediate = groupBox.SimplifiedStateDefinition.EnlargeState(groupBox.StateIntermediate);
            }
            else
            {
                groupBox.StateIntermediate = groupBox.StateDefinition.EnlargeState(groupBox.StateIntermediate);
            }
        }
    }

    // Decrease size of the item
    private void DecreaseGroupBoxSize( string name )
    {
        RibbonGroupBox? groupBox = FindGroup(name);
        var scale = name.StartsWith("(", StringComparison.OrdinalIgnoreCase);

        if (groupBox is null)
        {
            return;
        }

        if (scale)
        {
            groupBox.ScaleIntermediate--;
        }
        else
        {
            if (groupBox.IsSimplified)
            {
                groupBox.StateIntermediate = groupBox.SimplifiedStateDefinition.ReduceState(groupBox.StateIntermediate);
            }
            else
            {
                groupBox.StateIntermediate = groupBox.StateDefinition.ReduceState(groupBox.StateIntermediate);
            }
        }
    }

    private RibbonGroupBox? FindGroup( string name )
    {
        if (name.StartsWith("(", StringComparison.Ordinal))
        {
            name = name.Substring(1, name.Length - 2);
        }

        foreach (FrameworkElement? child in InternalChildren)
        {
            if (child?.Name == name)
            {
                return child as RibbonGroupBox;
            }
        }

        return null;
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride( Size finalSize )
    {
        var finalRect = new Rect(finalSize)
        {
            X = -HorizontalOffset
        };

        foreach (UIElement? item in InternalChildren)
        {
            if (item is null)
            {
                continue;
            }

            finalRect.Width = item.DesiredSize.Width;
            finalRect.Height = Math.Max(finalSize.Height, item.DesiredSize.Height);
            item.Arrange(finalRect);
            finalRect.X += item.DesiredSize.Width;
        }

        return finalSize;
    }

    /// <inheritdoc />
    public ScrollViewer? ScrollOwner
    {
        get => ScrollData.ScrollOwner;
        set => ScrollData.ScrollOwner = value;
    }

    /// <inheritdoc />
    public void SetHorizontalOffset( double offset )
    {
        var newValue = CoerceOffset(ValidateInputOffset(offset, nameof(HorizontalOffset)), ScrollData.ExtentWidth, ScrollData.ViewportWidth);

        if (DoubleUtil.AreClose(ScrollData.OffsetX, newValue) == false)
        {
            ScrollData.OffsetX = newValue;
            InvalidateArrange();
        }
    }

    /// <inheritdoc />
    public double ExtentWidth => ScrollData.ExtentWidth;

    /// <inheritdoc />
    public double HorizontalOffset => ScrollData.OffsetX;

    /// <inheritdoc />
    public double ViewportWidth => ScrollData.ViewportWidth;

    /// <inheritdoc />
    public void LineLeft()
    {
        SetHorizontalOffset(HorizontalOffset - 48.0);
    }

    /// <inheritdoc />
    public void LineRight()
    {
        SetHorizontalOffset(HorizontalOffset + 48.0);
    }

    /// <inheritdoc />
    public Rect MakeVisible( Visual visual, Rect rectangle )
    {
        // We can only work on visuals that are us or children.
        // An empty rect has no size or position.  We can't meaningfully use it.
        if (rectangle.IsEmpty
            || visual is null
            || ReferenceEquals(visual, this)
            || !IsAncestorOf(visual))
        {
            return Rect.Empty;
        }

        // Compute the child's rect relative to (0,0) in our coordinate space.
        GeneralTransform childTransform = visual.TransformToAncestor(this);

        rectangle = childTransform.TransformBounds(rectangle);

        // Initialize the viewport
        var viewport = new Rect(HorizontalOffset, rectangle.Top, ViewportWidth, rectangle.Height);
        rectangle.X += viewport.X;

        // Compute the offsets required to minimally scroll the child maximally into view.
        var minX = ComputeScrollOffsetWithMinimalScroll(viewport.Left, viewport.Right, rectangle.Left, rectangle.Right);

        // We have computed the scrolling offsets; scroll to them.
        SetHorizontalOffset(minX);

        // Compute the visible rectangle of the child relative to the viewport.
        viewport.X = minX;
        rectangle.Intersect(viewport);

        rectangle.X -= viewport.X;

        // Return the rectangle
        return rectangle;
    }

    private static double ComputeScrollOffsetWithMinimalScroll(
        double topView,
        double bottomView,
        double topChild,
        double bottomChild )
    {
        // # CHILD POSITION       CHILD SIZE      SCROLL      REMEDY
        // 1 Above viewport       <= viewport     Down        Align top edge of child & viewport
        // 2 Above viewport       > viewport      Down        Align bottom edge of child & viewport
        // 3 Below viewport       <= viewport     Up          Align bottom edge of child & viewport
        // 4 Below viewport       > viewport      Up          Align top edge of child & viewport
        // 5 Entirely within viewport             NA          No scroll.
        // 6 Spanning viewport                    NA          No scroll.
        //
        // Note: "Above viewport" = childTop above viewportTop, childBottom above viewportBottom
        //       "Below viewport" = childTop below viewportTop, childBottom below viewportBottom
        // These child thus may overlap with the viewport, but will scroll the same direction
        /*bool fAbove = DoubleUtil.LessThan(topChild, topView) && DoubleUtil.LessThan(bottomChild, bottomView);
        bool fBelow = DoubleUtil.GreaterThan(bottomChild, bottomView) && DoubleUtil.GreaterThan(topChild, topView);*/
        var fAbove = (topChild < topView) && (bottomChild < bottomView);
        var fBelow = (bottomChild > bottomView) && (topChild > topView);
        var fLarger = bottomChild - topChild > bottomView - topView;

        // Handle Cases:  1 & 4 above
        if ((fAbove && !fLarger)
            || (fBelow && fLarger))
        {
            return topChild;
        }

        // Handle Cases: 2 & 3 above
        if (fAbove || fBelow)
        {
            return bottomChild - (bottomView - topView);
        }

        // Handle cases: 5 & 6 above.
        return topView;
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public void MouseWheelDown()
    {
    }

    /// <inheritdoc />
    public void MouseWheelLeft()
    {
        SetHorizontalOffset(HorizontalOffset - 48.0);
    }

    /// <inheritdoc />
    public void MouseWheelRight()
    {
        SetHorizontalOffset(HorizontalOffset + 48.0);
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public void MouseWheelUp()
    {
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public void LineDown()
    {
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public void LineUp()
    {
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public void PageDown()
    {
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public void PageLeft()
    {
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public void PageRight()
    {
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public void PageUp()
    {
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public void SetVerticalOffset( double offset )
    {
    }

    /// <inheritdoc />
    public bool CanVerticallyScroll
    {
        get => false;
        set { }
    }

    /// <inheritdoc />
    public bool CanHorizontallyScroll
    {
        get => true;
        set { }
    }

    /// <summary>
    /// Gets not implemented
    /// </summary>
    public double ExtentHeight => 0.0;

    /// <summary>
    /// Gets not implemented
    /// </summary>
    public double VerticalOffset => 0.0;

    /// <summary>
    /// Gets not implemented
    /// </summary>
    public double ViewportHeight => 0.0;

    // Gets scroll data info
    private ScrollData ScrollData => scrollData ?? (scrollData = new ScrollData());

    // Scroll data info
    private ScrollData? scrollData;

    // Validates input offset
    private static double ValidateInputOffset( double offset, string parameterName )
    {
        if (double.IsNaN(offset))
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }

        return Math.Max(0.0, offset);
    }

    // Verifies scrolling data using the passed viewport and extent as newly computed values.
    // Checks the X/Y offset and coerces them into the range [0, Extent - ViewportSize]
    // If extent, viewport, or the newly coerced offsets are different than the existing offset,
    //   cachces are updated and InvalidateScrollInfo() is called.
    private void VerifyScrollData( double viewportWidth, double extentWidth )
    {
        var isValid = true;

        if (double.IsInfinity(viewportWidth))
        {
            viewportWidth = extentWidth;
        }

        var offsetX = CoerceOffset(ScrollData.OffsetX, extentWidth, viewportWidth);

        isValid &= DoubleUtil.AreClose(viewportWidth, ScrollData.ViewportWidth);
        isValid &= DoubleUtil.AreClose(extentWidth, ScrollData.ExtentWidth);
        isValid &= DoubleUtil.AreClose(ScrollData.OffsetX, offsetX);

        ScrollData.ViewportWidth = viewportWidth;
        ScrollData.ExtentWidth = extentWidth;
        ScrollData.OffsetX = offsetX;

        if (isValid == false)
        {
            ScrollOwner?.InvalidateScrollInfo();
        }
    }

    // Returns an offset coerced into the [0, Extent - Viewport] range.
    private static double CoerceOffset( double offset, double extent, double viewport )
    {
        if (offset > extent - viewport)
        {
            offset = extent - viewport;
        }

        if (offset < 0)
        {
            offset = 0;
        }

        return offset;
    }

    /// <inheritdoc />
    protected override void OnChildDesiredSizeChanged( UIElement child )
    {
        // Prevent invalidation for various reasons.
        // This is done to prevent excessive measuring calls.
        if (IsMeasureValid is false)
        {
            return;
        }

        base.OnChildDesiredSizeChanged(child);
        GroupBoxCacheClearedAndStateAndScaleResetted(null);
    }

    // We have to reset the reduce order to it's initial value, clear all caches we keep here and invalidate measure/arrange
    internal void GroupBoxCacheClearedAndStateAndScaleResetted( RibbonGroupBox? ribbonGroupBox )
    {
        if (measureCache.IsEmpty)
        {
            return;
        }

        var newReduceOrderIndex = reduceOrder.Length - 1;
        reduceOrderIndex = newReduceOrderIndex;

        measureCache = MeasureCache.Empty;

        foreach (var item in InternalChildren)
        {
            var groupBox = item as RibbonGroupBox;
            if (groupBox is null
                || ReferenceEquals(groupBox, ribbonGroupBox))
            {
                continue;
            }

            _ = groupBox.TryClearCacheAndResetStateAndScale();
        }

        this.InvalidateMeasureAndArrange();
    }
}