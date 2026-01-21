// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// A panel that hosts <see cref="ToolBarGroup"/> items and supports drag-and-drop reordering
/// with visual preview indicator. Supports multi-row layouts when items are dragged to a new row.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;ui:ToolBarPanel Orientation="Horizontal" AllowMultiRow="True"&gt;
///     &lt;ui:ToolBarGroup ShowGrip="True" IsDraggable="True"&gt;
///         &lt;ui:ToolBarButton Icon="{ui:SymbolIcon Copy20}" /&gt;
///     &lt;/ui:ToolBarGroup&gt;
/// &lt;/ui:ToolBarPanel&gt;
/// </code>
/// </example>
public class ToolBarPanel : Panel
{
    /// <summary>Identifies the <see cref="Orientation"/> dependency property.</summary>
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        nameof(Orientation),
        typeof(Orientation),
        typeof(ToolBarPanel),
        new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure)
    );

    /// <summary>Identifies the <see cref="DropIndicatorBrush"/> dependency property.</summary>
    public static readonly DependencyProperty DropIndicatorBrushProperty = DependencyProperty.Register(
        nameof(DropIndicatorBrush),
        typeof(Brush),
        typeof(ToolBarPanel),
        new PropertyMetadata(null)
    );

    /// <summary>Identifies the <see cref="AllowReorder"/> dependency property.</summary>
    public static readonly DependencyProperty AllowReorderProperty = DependencyProperty.Register(
        nameof(AllowReorder),
        typeof(bool),
        typeof(ToolBarPanel),
        new PropertyMetadata(true)
    );

    /// <summary>Identifies the <see cref="AllowMultiRow"/> dependency property.</summary>
    public static readonly DependencyProperty AllowMultiRowProperty = DependencyProperty.Register(
        nameof(AllowMultiRow),
        typeof(bool),
        typeof(ToolBarPanel),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure)
    );

    /// <summary>Identifies the <see cref="RowHeight"/> dependency property.</summary>
    public static readonly DependencyProperty RowHeightProperty = DependencyProperty.Register(
        nameof(RowHeight),
        typeof(double),
        typeof(ToolBarPanel),
        new FrameworkPropertyMetadata(36.0, FrameworkPropertyMetadataOptions.AffectsMeasure)
    );

    /// <summary>
    /// Gets or sets the orientation of the panel (Horizontal or Vertical).
    /// </summary>
    [Bindable(true)]
    [Category("Layout")]
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used for the drop indicator line.
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public Brush? DropIndicatorBrush
    {
        get => (Brush?)GetValue(DropIndicatorBrushProperty);
        set => SetValue(DropIndicatorBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether reordering via drag-and-drop is allowed.
    /// </summary>
    [Bindable(true)]
    [Category("Behavior")]
    public bool AllowReorder
    {
        get => (bool)GetValue(AllowReorderProperty);
        set => SetValue(AllowReorderProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether multi-row layout is enabled.
    /// When true, users can drag items to create new rows.
    /// </summary>
    [Bindable(true)]
    [Category("Layout")]
    public bool AllowMultiRow
    {
        get => (bool)GetValue(AllowMultiRowProperty);
        set => SetValue(AllowMultiRowProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of each row in multi-row mode.
    /// </summary>
    [Bindable(true)]
    [Category("Layout")]
    public double RowHeight
    {
        get => (double)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    /// <summary>
    /// Occurs when the order of toolbar groups changes via drag-and-drop.
    /// </summary>
    public event EventHandler<ToolBarGroupOrderChangedEventArgs>? GroupOrderChanged;

    /// <summary>
    /// Stores the row assignment for each child (child index -> row number).
    /// Explicitly set by user drag operations.
    /// </summary>
    private readonly Dictionary<UIElement, int> _childRowMap = new Dictionary<UIElement, int>();

    private int _dropIndex = -1;
    private int _dropRow = 0;
    private ToolBarGroup? _draggedItem;

    /// <summary>
    /// Current number of rows.
    /// </summary>
    private int _rowCount = 1;

    /// <summary>
    /// Indicates if a drag operation is in progress, used to expand panel for new row drop zone.
    /// </summary>
    private bool _isDragInProgress;

    /// <summary>
    /// The Y position where the drag started, used to determine drag direction.
    /// </summary>
    private double _dragStartY;

    /// <summary>
    /// The row of the dragged item when drag started.
    /// </summary>
    private int _dragStartRow;

    /// <summary>
    /// Direction of expansion: -1 = above, 0 = none, 1 = below
    /// </summary>
    private int _expandDirection;

    /// <summary>
    /// The group currently being dragged (VS-style real-time drag).
    /// </summary>
    private ToolBarGroup? _draggingGroup;

    /// <summary>
    /// Original index of the dragging group.
    /// </summary>
    private int _draggingOriginalIndex;

    /// <summary>
    /// Mouse X offset within the dragging group.
    /// </summary>
    private double _draggingOffsetX;

    /// <summary>
    /// Mouse Y offset within the dragging group.
    /// </summary>
    private double _draggingOffsetY;

    /// <summary>
    /// Last recorded mouse position during drag.
    /// </summary>
    private Point _lastDragPosition;

    public ToolBarPanel()
    {
        AllowDrop = true;

        DragEnter += OnDragEnter;
        DragOver += OnDragOver;
        DragLeave += OnDragLeave;
        Drop += OnDrop;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        if (!AllowMultiRow || Orientation == Orientation.Vertical)
        {
            return MeasureSingleRow(constraint);
        }

        return MeasureExplicitRows(constraint);
    }

    private Size MeasureSingleRow(Size constraint)
    {
        double totalSize = 0;
        double maxCrossSize = 0;

        foreach (UIElement child in Children)
        {
            child.Measure(constraint);
            Size desiredSize = child.DesiredSize;

            if (Orientation == Orientation.Horizontal)
            {
                totalSize += desiredSize.Width;
                maxCrossSize = Math.Max(maxCrossSize, desiredSize.Height);
            }
            else
            {
                totalSize += desiredSize.Height;
                maxCrossSize = Math.Max(maxCrossSize, desiredSize.Width);
            }
        }

        return Orientation == Orientation.Horizontal
            ? new Size(totalSize, maxCrossSize)
            : new Size(maxCrossSize, totalSize);
    }

    private Size MeasureExplicitRows(Size constraint)
    {
        // Calculate row count from explicit assignments
        _rowCount = 1;
        foreach (KeyValuePair<UIElement, int> kvp in _childRowMap)
        {
            if (Children.Contains(kvp.Key))
            {
                _rowCount = Math.Max(_rowCount, kvp.Value + 1);
            }
        }

        // Measure children and calculate row widths
        var rowWidths = new double[_rowCount];
        double maxHeight = 0;

        foreach (UIElement child in Children)
        {
            child.Measure(constraint);
            Size desiredSize = child.DesiredSize;
            int row = GetChildRow(child);

            rowWidths[row] += desiredSize.Width;
            maxHeight = Math.Max(maxHeight, desiredSize.Height);
        }

        // Total width is the max row width
        double totalWidth = 0;
        foreach (var w in rowWidths)
        {
            totalWidth = Math.Max(totalWidth, w);
        }

        // Total height: use RowHeight for consistency
        // Add extra row height during drag based on expand direction
        int displayRowCount = _isDragInProgress ? _rowCount + 1 : _rowCount;
        double totalHeight = displayRowCount * RowHeight;

        return new Size(totalWidth, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (!AllowMultiRow || Orientation == Orientation.Vertical)
        {
            return ArrangeSingleRow(finalSize);
        }

        return ArrangeExplicitRows(finalSize);
    }

    private Size ArrangeSingleRow(Size finalSize)
    {
        // Capture drag state at method start
        ToolBarGroup? draggingGroup = _draggingGroup;
        bool isDragging = _isDragInProgress && draggingGroup != null;
        Point lastDragPos = _lastDragPosition;
        double offsetX = _draggingOffsetX;
        double offsetY = _draggingOffsetY;

        // Get dragging group width for space reservation
        double draggingWidth = 0;
        double draggingHeight = 0;
        if (isDragging && draggingGroup != null && draggingGroup.IsMeasureValid)
        {
            draggingWidth = draggingGroup.DesiredSize.Width;
            draggingHeight = draggingGroup.DesiredSize.Height;
        }

        double offset = 0;
        int currentIndex = 0;

        foreach (UIElement child in Children)
        {
            // Skip dragging group - it will be positioned at mouse location
            if (isDragging && child == draggingGroup)
            {
                currentIndex++;
                continue;
            }

            // Leave space for dragging group at drop position
            if (isDragging && draggingWidth > 0 && currentIndex == _dropIndex)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    offset += draggingWidth;
                }
                else
                {
                    offset += draggingHeight;
                }
            }

            Size desiredSize = child.DesiredSize;

            if (Orientation == Orientation.Horizontal)
            {
                child.Arrange(new Rect(offset, 0, desiredSize.Width, finalSize.Height));
                offset += desiredSize.Width;
            }
            else
            {
                child.Arrange(new Rect(0, offset, finalSize.Width, desiredSize.Height));
                offset += desiredSize.Height;
            }

            currentIndex++;
        }

        // Arrange the dragging group at mouse position (VS-style real-time movement)
        if (isDragging && draggingGroup != null && draggingWidth > 0)
        {
            double dragX, dragY;

            if (Orientation == Orientation.Horizontal)
            {
                dragX = lastDragPos.X - offsetX;
                dragY = 0;

                // Clamp X to panel bounds
                dragX = Math.Max(0, Math.Min(dragX, finalSize.Width - draggingWidth));

                draggingGroup.Arrange(new Rect(dragX, dragY, draggingWidth, finalSize.Height));
            }
            else
            {
                dragX = 0;
                dragY = lastDragPos.Y - offsetY;

                // Clamp Y to panel bounds
                dragY = Math.Max(0, Math.Min(dragY, finalSize.Height - draggingHeight));

                draggingGroup.Arrange(new Rect(dragX, dragY, finalSize.Width, draggingHeight));
            }
        }

        return finalSize;
    }

    private Size ArrangeExplicitRows(Size finalSize)
    {
        // Capture state at method start to avoid re-entrancy issues
        ToolBarGroup? draggingGroup = _draggingGroup;
        bool isDragging = _isDragInProgress && draggingGroup != null;
        int dropIndex = _dropIndex;
        int dropRow = _dropRow;
        int expandDir = _expandDirection;
        Point lastDragPos = _lastDragPosition;
        double offsetX = _draggingOffsetX;

        // Group children by row (excluding dragging group for position calculation)
        var rowChildren = new List<UIElement>[_rowCount];
        for (int i = 0; i < _rowCount; i++)
        {
            rowChildren[i] = new List<UIElement>();
        }

        foreach (UIElement child in Children)
        {
            // Skip dragging group - it will be positioned separately
            if (isDragging && child == draggingGroup)
            {
                continue;
            }

            int row = GetChildRow(child);
            if (row < _rowCount)
            {
                rowChildren[row].Add(child);
            }
        }

        // Arrange each row
        double actualRowHeight = RowHeight;

        // When expanding above, shift all existing rows down by one row height
        double yShift = (isDragging && expandDir == -1) ? RowHeight : 0;

        // Pre-calculate dragging group width (if valid)
        double draggingWidth = 0;
        if (isDragging && draggingGroup != null && draggingGroup.IsMeasureValid)
        {
            draggingWidth = draggingGroup.DesiredSize.Width;
        }

        for (int row = 0; row < _rowCount; row++)
        {
            double xOffset = 0;
            double yOffset = (row * actualRowHeight) + yShift;

            // Check if this is the drop target row and we need to leave space
            bool canLeaveSpace = isDragging && draggingWidth > 0 && dropIndex >= 0;
            bool isDropTargetRow = canLeaveSpace &&
                                   ((dropRow == -1 && row == 0 && expandDir == -1) ||
                                    (dropRow >= 0 && dropRow == row));
            int positionInRow = 0;

            foreach (UIElement child in rowChildren[row])
            {
                // Leave space for dragging group at drop position
                if (isDropTargetRow && positionInRow == dropIndex)
                {
                    xOffset += draggingWidth;
                }

                Size desiredSize = child.DesiredSize;
                child.Arrange(new Rect(xOffset, yOffset, desiredSize.Width, actualRowHeight));
                xOffset += desiredSize.Width;
                positionInRow++;
            }
        }

        // Arrange the dragging group at mouse position (VS-style real-time movement)
        if (isDragging && draggingGroup != null && draggingWidth > 0)
        {
            // Calculate position based on last mouse position
            double dragX = lastDragPos.X - offsetX;
            double dragY;

            // Determine Y position based on drop row
            if (dropRow == -1)
            {
                dragY = 0; // New row at top
            }
            else if (expandDir == -1)
            {
                dragY = (dropRow + 1) * actualRowHeight;
            }
            else
            {
                dragY = dropRow * actualRowHeight;
            }

            // Clamp X to panel bounds
            dragX = Math.Max(0, Math.Min(dragX, finalSize.Width - draggingWidth));

            draggingGroup.Arrange(new Rect(
                dragX,
                dragY,
                draggingWidth,
                actualRowHeight));
        }

        return finalSize;
    }

    private int GetChildRow(UIElement child)
    {
        return _childRowMap.TryGetValue(child, out int row) ? row : 0;
    }

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        if (!AllowReorder || !e.Data.GetDataPresent(typeof(ToolBarGroup)))
        {
            e.Effects = DragDropEffects.None;
            return;
        }

        _draggedItem = e.Data.GetData(typeof(ToolBarGroup)) as ToolBarGroup;
        if (_draggedItem is null || !Children.Contains(_draggedItem))
        {
            e.Effects = DragDropEffects.None;
            return;
        }

        e.Effects = DragDropEffects.Move;
        e.Handled = true;

        // Record drag start position for direction detection
        if (AllowMultiRow)
        {
            Point position = e.GetPosition(this);
            _dragStartY = position.Y;
            _dragStartRow = GetChildRow(_draggedItem);
            _expandDirection = 0;
        }
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (!AllowReorder || _draggedItem is null)
        {
            e.Effects = DragDropEffects.None;
            return;
        }

        e.Effects = DragDropEffects.Move;
        e.Handled = true;

        Point position = e.GetPosition(this);

        // Expand when cursor approaches edge (before leaving toolbar bounds)
        if (AllowMultiRow)
        {
            const double edgeThreshold = 12.0; // Pixels from edge to trigger expansion
            int newDirection = 0;

            if (position.Y < edgeThreshold)
            {
                newDirection = -1; // Near top edge - expand up
            }
            else if (position.Y > ActualHeight - edgeThreshold)
            {
                newDirection = 1; // Near bottom edge - expand down
            }

            if (newDirection != 0 && newDirection != _expandDirection)
            {
                _expandDirection = newDirection;
                _isDragInProgress = true;
                InvalidateMeasure();
                InvalidateArrange();
            }
            else if (newDirection == 0 && _isDragInProgress)
            {
                // Back to center - collapse
                _expandDirection = 0;
                _isDragInProgress = false;
                InvalidateMeasure();
                InvalidateArrange();
            }
        }

        (int newDropIndex, int newDropRow) = CalculateDropPosition(position);

        if (newDropIndex != _dropIndex || newDropRow != _dropRow)
        {
            _dropIndex = newDropIndex;
            _dropRow = newDropRow;
            UpdateDropIndicatorPosition();
        }
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        HideDropIndicator();
        _dropIndex = -1;
        _dropRow = 0;
        _expandDirection = 0;
        _dragStartY = 0;
        _dragStartRow = 0;

        // Collapse expanded drop zone
        if (_isDragInProgress)
        {
            _isDragInProgress = false;
            InvalidateMeasure();
        }
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (!AllowReorder || _draggedItem is null || _dropIndex < 0)
        {
            HideDropIndicator();
            return;
        }

        // Get current state
        int currentIndex = Children.IndexOf(_draggedItem);
        int currentRow = GetChildRow(_draggedItem);

        // Handle row change in multi-row mode
        if (AllowMultiRow)
        {
            int actualDropRow = _dropRow;

            // Handle new row above (row -1 becomes row 0, all others shift down)
            if (_dropRow == -1)
            {
                // Shift all existing rows down by 1
                foreach (UIElement child in Children)
                {
                    if (child != _draggedItem)
                    {
                        int childRow = GetChildRow(child);
                        _childRowMap[child] = childRow + 1;
                    }
                }

                actualDropRow = 0;
                _rowCount++;
            }

            if (actualDropRow != currentRow || _dropRow == -1)
            {
                // Update row assignment for dragged item
                _childRowMap[_draggedItem] = actualDropRow;

                // If new row exceeds current row count, update
                if (actualDropRow >= _rowCount)
                {
                    _rowCount = actualDropRow + 1;
                }

                // Clean up empty rows
                CleanupEmptyRows();
            }
        }

        // Handle position change within row
        if (currentIndex >= 0)
        {
            // Calculate target index considering row grouping
            int targetIndex = CalculateTargetIndex(_dropIndex, _dropRow, currentIndex);

            if (currentIndex != targetIndex)
            {
                Children.RemoveAt(currentIndex);
                if (targetIndex > currentIndex)
                {
                    targetIndex--;
                }

                targetIndex = Math.Max(0, Math.Min(targetIndex, Children.Count));
                Children.Insert(targetIndex, _draggedItem);
            }
        }

        HideDropIndicator();
        _draggedItem = null;
        _dropIndex = -1;
        _dropRow = 0;
        _isDragInProgress = false;
        _expandDirection = 0;
        _dragStartY = 0;
        _dragStartRow = 0;
        e.Handled = true;

        InvalidateMeasure();
        RaiseGroupOrderChanged();
    }

    private int CalculateTargetIndex(int dropIndex, int dropRow, int currentIndex)
    {
        if (!AllowMultiRow)
        {
            return dropIndex;
        }

        // Count items before drop position in the target row
        int targetIndex = 0;
        int positionInRow = 0;

        foreach (UIElement child in Children)
        {
            int childRow = GetChildRow(child);

            if (childRow < dropRow)
            {
                targetIndex++;
            }
            else if (childRow == dropRow)
            {
                if (positionInRow < dropIndex)
                {
                    targetIndex++;
                    positionInRow++;
                }
            }
        }

        return targetIndex;
    }

    private void CleanupEmptyRows()
    {
        // Find which rows have children
        var usedRows = new HashSet<int>();
        foreach (UIElement child in Children)
        {
            _ = usedRows.Add(GetChildRow(child));
        }

        // If a row is empty and not row 0, remap higher rows down
        if (usedRows.Count < _rowCount)
        {
            var sortedRows = usedRows.OrderBy(r => r).ToList();
            var rowMapping = new Dictionary<int, int>();

            for (int i = 0; i < sortedRows.Count; i++)
            {
                rowMapping[sortedRows[i]] = i;
            }

            // Remap all children
            var newMap = new Dictionary<UIElement, int>();
            foreach (KeyValuePair<UIElement, int> kvp in _childRowMap)
            {
                if (Children.Contains(kvp.Key) && rowMapping.TryGetValue(kvp.Value, out int newRow))
                {
                    newMap[kvp.Key] = newRow;
                }
            }

            _childRowMap.Clear();
            foreach (KeyValuePair<UIElement, int> kvp in newMap)
            {
                _childRowMap[kvp.Key] = kvp.Value;
            }

            _rowCount = sortedRows.Count > 0 ? sortedRows.Count : 1;
        }
    }

    private (int Index, int Row) CalculateDropPosition(Point position)
    {
        if (!AllowMultiRow || Orientation == Orientation.Vertical)
        {
            return (CalculateDropIndexSingleRow(position), 0);
        }

        return CalculateDropPositionMultiRow(position);
    }

    private int CalculateDropIndexSingleRow(Point position)
    {
        int index = 0;
        double accumulatedPosition = 0;

        foreach (UIElement child in Children)
        {
            if (child is not FrameworkElement element)
            {
                index++;
                continue;
            }

            double elementSize = Orientation == Orientation.Horizontal
                ? element.ActualWidth
                : element.ActualHeight;
            double elementPosition = Orientation == Orientation.Horizontal
                ? position.X
                : position.Y;

            double midPoint = accumulatedPosition + (elementSize / 2);

            if (elementPosition < midPoint)
            {
                return index;
            }

            accumulatedPosition += elementSize;
            index++;
        }

        return Children.Count;
    }

    private (int Index, int Row) CalculateDropPositionMultiRow(Point position)
    {
        // Use RowHeight for consistent calculation
        double rowHeight = RowHeight;

        // Account for Y shift when expanding above
        double adjustedY = position.Y;
        if (_isDragInProgress && _expandDirection == -1)
        {
            adjustedY -= rowHeight; // The first row is the new empty row
        }

        // Calculate which row based on adjusted Y position
        int targetRow;

        if (_isDragInProgress)
        {
            if (_expandDirection == -1)
            {
                // Expanding above: row 0 is the new row, existing rows shift down
                if (position.Y < rowHeight)
                {
                    targetRow = -1; // New row above (will be converted to 0 when dropped)
                }
                else
                {
                    targetRow = (int)(adjustedY / rowHeight);
                    targetRow = Math.Max(0, Math.Min(targetRow, _rowCount - 1));
                }
            }
            else if (_expandDirection == 1)
            {
                // Expanding below: new row at bottom
                targetRow = (int)(position.Y / rowHeight);
                if (targetRow >= _rowCount)
                {
                    targetRow = _rowCount; // New row below
                }
                else
                {
                    targetRow = Math.Max(0, Math.Min(targetRow, _rowCount - 1));
                }
            }
            else
            {
                targetRow = (int)(position.Y / rowHeight);
                targetRow = Math.Max(0, Math.Min(targetRow, _rowCount - 1));
            }
        }
        else
        {
            targetRow = (int)(position.Y / rowHeight);
            targetRow = Math.Max(0, Math.Min(targetRow, _rowCount - 1));
        }

        // Find drop index within that row
        double accumulatedX = 0;
        int indexInRow = 0;

        foreach (UIElement child in Children)
        {
            if (GetChildRow(child) != targetRow)
            {
                continue;
            }

            if (child is FrameworkElement element)
            {
                double midPoint = accumulatedX + (element.ActualWidth / 2);

                if (position.X < midPoint)
                {
                    return (indexInRow, targetRow);
                }

                accumulatedX += element.ActualWidth;
            }

            indexInRow++;
        }

        return (indexInRow, targetRow);
    }

    private void UpdateDropIndicatorPosition()
    {
        if (_dropIndex < 0)
        {
            return;
        }

        double indicatorX = 0;
        double indicatorY = 0;
        double indicatorHeight = ActualHeight;

        if (AllowMultiRow)
        {
            double actualRowHeight = RowHeight;
            indicatorHeight = actualRowHeight;

            // Handle new row above (row -1)
            if (_dropRow == -1)
            {
                indicatorY = 0; // New row is at the top
            }
            else if (_isDragInProgress && _expandDirection == -1)
            {
                // When expanding above, existing rows are shifted down
                indicatorY = (_dropRow + 1) * actualRowHeight;
            }
            else
            {
                indicatorY = _dropRow * actualRowHeight;
            }

            // Calculate X position within the row (for new rows, it's always 0)
            if (_dropRow == -1 || _dropRow >= _rowCount)
            {
                indicatorX = 0;
            }
            else
            {
                int positionInRow = 0;
                foreach (UIElement child in Children)
                {
                    if (GetChildRow(child) == _dropRow)
                    {
                        if (positionInRow >= _dropIndex)
                        {
                            break;
                        }

                        if (child is FrameworkElement element)
                        {
                            indicatorX += element.ActualWidth;
                        }

                        positionInRow++;
                    }
                }
            }
        }
        else
        {
            // Single row mode
            int index = 0;
            foreach (UIElement child in Children)
            {
                if (index >= _dropIndex)
                {
                    break;
                }

                if (child is FrameworkElement element)
                {
                    indicatorX += Orientation == Orientation.Horizontal
                        ? element.ActualWidth
                        : element.ActualHeight;
                }

                index++;
            }
        }

        // Use adorner layer for the indicator
        var adornerLayer = AdornerLayer.GetAdornerLayer(this);
        if (adornerLayer is not null)
        {
            // Remove existing adorner
            Adorner[]? existingAdorners = adornerLayer.GetAdorners(this);
            if (existingAdorners is not null)
            {
                foreach (DropIndicatorAdorner adorner in existingAdorners.OfType<DropIndicatorAdorner>())
                {
                    adornerLayer.Remove(adorner);
                }
            }

            // Add new adorner
            var indicatorAdorner = new DropIndicatorAdorner(
                this,
                indicatorX,
                indicatorY,
                indicatorHeight,
                Orientation,
                DropIndicatorBrush ?? new SolidColorBrush(Color.FromArgb(255, 0, 120, 215))
            );
            adornerLayer.Add(indicatorAdorner);
        }
    }

    private void HideDropIndicator()
    {
        var adornerLayer = AdornerLayer.GetAdornerLayer(this);
        if (adornerLayer is null)
        {
            return;
        }

        Adorner[]? existingAdorners = adornerLayer.GetAdorners(this);
        if (existingAdorners is null)
        {
            return;
        }

        foreach (DropIndicatorAdorner adorner in existingAdorners.OfType<DropIndicatorAdorner>())
        {
            adornerLayer.Remove(adorner);
        }
    }

    /// <summary>
    /// Called by ToolBarGroup when drag starts.
    /// </summary>
    public void BeginDrag(ToolBarGroup group)
    {
        if (group == null || !AllowReorder || !Children.Contains(group))
        {
            return;
        }

        int groupIndex = Children.IndexOf(group);
        if (groupIndex < 0)
        {
            return;
        }

        // Set state
        _draggingGroup = group;
        _draggedItem = group;
        _isDragInProgress = true;
        _draggingOriginalIndex = groupIndex;
        _dragStartRow = GetChildRow(group);
        _dropIndex = groupIndex;
        _dropRow = _dragStartRow;

        // Calculate mouse offset within the group
        try
        {
            Point mousePos = Mouse.GetPosition(group);
            _draggingOffsetX = Math.Max(0, mousePos.X);
            _draggingOffsetY = Math.Max(0, mousePos.Y);
            _lastDragPosition = Mouse.GetPosition(this);
        }
        catch
        {
            _draggingOffsetX = group.ActualWidth / 2;
            _draggingOffsetY = group.ActualHeight / 2;
        }

        // Apply visual effects (these don't affect layout, so safe to apply here)
        // Opacity and ZIndex changes don't trigger measure/arrange
        group.SetCurrentValue(UIElement.OpacityProperty, 0.75);
        SetZIndex(group, 1000);
    }

    /// <summary>
    /// Called by ToolBarGroup during drag to update position.
    /// </summary>
    public void UpdateDragPosition(ToolBarGroup group, Point mousePosition)
    {
        if (_draggingGroup != group)
        {
            return;
        }

        _lastDragPosition = mousePosition;

        // Determine row expansion based on Y position
        if (AllowMultiRow)
        {
            DetermineRowExpansionFromPosition(mousePosition.Y);
        }

        // Calculate drop position
        (int newDropIndex, int newDropRow) = CalculateDropPositionFromMouse(mousePosition);

        if (newDropIndex != _dropIndex || newDropRow != _dropRow)
        {
            _dropIndex = newDropIndex;
            _dropRow = newDropRow;
            UpdateDropIndicatorPosition();
        }

        // Force re-arrange to update dragging group position
        InvalidateArrange();
    }

    /// <summary>
    /// Called by ToolBarGroup when drag ends.
    /// </summary>
    public void EndDrag(ToolBarGroup group)
    {
        if (_draggingGroup != group)
        {
            return;
        }

        // Remove visual effects
        try
        {
            group.SetCurrentValue(UIElement.OpacityProperty, 1.0);
            group.ClearValue(Panel.ZIndexProperty);
        }
        catch
        {
            // Ignore visual effect cleanup errors
        }

        // Finalize position if valid drop
        if (_dropIndex >= 0)
        {
            FinalizeGroupPosition(_dropIndex, _dropRow);
        }

        // Reset state
        _draggingGroup = null;
        _draggedItem = null;
        _isDragInProgress = false;
        _expandDirection = 0;
        _dropIndex = -1;
        _dropRow = 0;
        _dragStartY = 0;
        _dragStartRow = 0;

        HideDropIndicator();
        InvalidateMeasure();
        InvalidateArrange();
        RaiseGroupOrderChanged();
    }

    private void DetermineRowExpansionFromPosition(double mouseY)
    {
        const double edgeThreshold = 12.0;
        int newDirection = 0;

        if (mouseY < edgeThreshold)
        {
            newDirection = -1; // Near top edge - expand up
        }
        else if (mouseY > ActualHeight - edgeThreshold)
        {
            newDirection = 1; // Near bottom edge - expand down
        }

        if (newDirection != 0 && newDirection != _expandDirection)
        {
            _expandDirection = newDirection;
            InvalidateMeasure();
            InvalidateArrange();
        }
        else if (newDirection == 0 && _expandDirection != 0)
        {
            // Back to center - collapse
            _expandDirection = 0;
            InvalidateMeasure();
            InvalidateArrange();
        }
    }

    private (int Index, int Row) CalculateDropPositionFromMouse(Point position)
    {
        if (!AllowMultiRow || Orientation == Orientation.Vertical)
        {
            return (CalculateDropIndexFromMouse(position), 0);
        }

        return CalculateDropPositionMultiRowFromMouse(position);
    }

    private int CalculateDropIndexFromMouse(Point position)
    {
        int index = 0;
        double accumulatedPosition = 0;

        foreach (UIElement child in Children)
        {
            // Skip the dragging group when calculating positions
            if (child == _draggingGroup)
            {
                index++;
                continue;
            }

            if (child is not FrameworkElement element)
            {
                index++;
                continue;
            }

            double elementSize = Orientation == Orientation.Horizontal
                ? element.ActualWidth
                : element.ActualHeight;
            double elementPosition = Orientation == Orientation.Horizontal
                ? position.X
                : position.Y;

            double midPoint = accumulatedPosition + (elementSize / 2);

            if (elementPosition < midPoint)
            {
                return index;
            }

            accumulatedPosition += elementSize;
            index++;
        }

        return Children.Count;
    }

    private (int Index, int Row) CalculateDropPositionMultiRowFromMouse(Point position)
    {
        double rowHeight = RowHeight;

        // Account for Y shift when expanding above
        double adjustedY = position.Y;
        if (_expandDirection == -1)
        {
            adjustedY -= rowHeight;
        }

        // Calculate target row
        int targetRow;

        if (_expandDirection == -1)
        {
            if (position.Y < rowHeight)
            {
                targetRow = -1; // New row above
            }
            else
            {
                targetRow = (int)(adjustedY / rowHeight);
                targetRow = Math.Max(0, Math.Min(targetRow, _rowCount - 1));
            }
        }
        else if (_expandDirection == 1)
        {
            targetRow = (int)(position.Y / rowHeight);
            if (targetRow >= _rowCount)
            {
                targetRow = _rowCount; // New row below
            }
            else
            {
                targetRow = Math.Max(0, Math.Min(targetRow, _rowCount - 1));
            }
        }
        else
        {
            targetRow = (int)(position.Y / rowHeight);
            targetRow = Math.Max(0, Math.Min(targetRow, _rowCount - 1));
        }

        // Find drop index within that row
        double accumulatedX = 0;
        int indexInRow = 0;

        foreach (UIElement child in Children)
        {
            // Skip dragging group
            if (child == _draggingGroup)
            {
                continue;
            }

            if (GetChildRow(child) != targetRow)
            {
                continue;
            }

            if (child is FrameworkElement element)
            {
                double midPoint = accumulatedX + (element.ActualWidth / 2);

                if (position.X < midPoint)
                {
                    return (indexInRow, targetRow);
                }

                accumulatedX += element.ActualWidth;
            }

            indexInRow++;
        }

        return (indexInRow, targetRow);
    }

    private void FinalizeGroupPosition(int targetIndex, int targetRow)
    {
        if (_draggingGroup == null)
        {
            return;
        }

        // Handle row change in multi-row mode
        if (AllowMultiRow)
        {
            int actualDropRow = targetRow;

            // Handle new row above (row -1 becomes row 0, all others shift down)
            if (targetRow == -1)
            {
                // Shift all existing rows down by 1
                foreach (UIElement child in Children)
                {
                    if (child != _draggingGroup)
                    {
                        int childRow = GetChildRow(child);
                        _childRowMap[child] = childRow + 1;
                    }
                }

                actualDropRow = 0;
                _rowCount++;
            }

            // Update row assignment for dragged item
            if (actualDropRow != _dragStartRow || targetRow == -1)
            {
                _childRowMap[_draggingGroup] = actualDropRow;

                if (actualDropRow >= _rowCount)
                {
                    _rowCount = actualDropRow + 1;
                }

                CleanupEmptyRows();
            }
        }

        // Handle position change within panel
        int currentIndex = Children.IndexOf(_draggingGroup);
        if (currentIndex >= 0 && currentIndex != targetIndex)
        {
            // Calculate the actual target index considering we're removing from current position
            int adjustedTargetIndex = CalculateAdjustedTargetIndex(targetIndex, targetRow, currentIndex);

            if (currentIndex != adjustedTargetIndex)
            {
                Children.RemoveAt(currentIndex);
                if (adjustedTargetIndex > currentIndex)
                {
                    adjustedTargetIndex--;
                }

                adjustedTargetIndex = Math.Max(0, Math.Min(adjustedTargetIndex, Children.Count));
                Children.Insert(adjustedTargetIndex, _draggingGroup);
            }
        }
    }

    private int CalculateAdjustedTargetIndex(int dropIndex, int dropRow, int currentIndex)
    {
        if (!AllowMultiRow)
        {
            return dropIndex;
        }

        // Count items before drop position in the target row
        int targetIndex = 0;
        int positionInRow = 0;

        foreach (UIElement child in Children)
        {
            if (child == _draggingGroup)
            {
                continue;
            }

            int childRow = GetChildRow(child);

            if (childRow < dropRow)
            {
                targetIndex++;
            }
            else if (childRow == dropRow)
            {
                if (positionInRow < dropIndex)
                {
                    targetIndex++;
                    positionInRow++;
                }
            }
        }

        return targetIndex;
    }

    private void RaiseGroupOrderChanged()
    {
        if (GroupOrderChanged is null)
        {
            return;
        }

        var order = GetCurrentOrder();
        var rowAssignments = AllowMultiRow ? GetRowAssignments() : null;
        GroupOrderChanged.Invoke(this, new ToolBarGroupOrderChangedEventArgs(order, rowAssignments));
    }

    /// <summary>
    /// Gets the current order of children as an array of indices.
    /// </summary>
    public int[] GetCurrentOrder()
    {
        var result = new int[Children.Count];
        for (var i = 0; i < Children.Count; i++)
        {
            result[i] = i;
        }

        return result;
    }

    /// <summary>
    /// Gets the row assignments for each child.
    /// </summary>
    public int[] GetRowAssignments()
    {
        var result = new int[Children.Count];
        for (int i = 0; i < Children.Count; i++)
        {
            result[i] = GetChildRow(Children[i]);
        }

        return result;
    }

    /// <summary>
    /// Sets the row assignment for a child element.
    /// </summary>
    public void SetChildRow(UIElement child, int row)
    {
        if (row <= 0)
        {
            _ = _childRowMap.Remove(child);
        }
        else
        {
            _childRowMap[child] = row;
            _rowCount = Math.Max(_rowCount, row + 1);
        }

        InvalidateMeasure();
    }

    /// <summary>
    /// Sets the order of children based on the provided index array.
    /// </summary>
    public void SetOrder(int[] order, UIElement[] originalChildren)
    {
        if (order.Length != originalChildren.Length)
        {
            return;
        }

        Children.Clear();
        foreach (var idx in order)
        {
            if (idx >= 0 && idx < originalChildren.Length)
            {
                _ = Children.Add(originalChildren[idx]);
            }
        }
    }

    /// <summary>
    /// Sets row assignments for children by name.
    /// </summary>
    public void SetRowAssignments(Dictionary<string, int> rowsByName)
    {
        _childRowMap.Clear();
        _rowCount = 1;

        foreach (UIElement child in Children)
        {
            if (child is FrameworkElement fe && !string.IsNullOrEmpty(fe.Name))
            {
                if (rowsByName.TryGetValue(fe.Name, out int row) && row > 0)
                {
                    _childRowMap[child] = row;
                    _rowCount = Math.Max(_rowCount, row + 1);
                }
            }
        }

        InvalidateMeasure();
    }

    private sealed class DropIndicatorAdorner : Adorner
    {
        private readonly double _positionX;
        private readonly double _positionY;
        private readonly double _height;
        private readonly Orientation _orientation;
        private readonly Brush _brush;
        private readonly Pen _pen;

        public DropIndicatorAdorner(
            UIElement adornedElement,
            double positionX,
            double positionY,
            double height,
            Orientation orientation,
            Brush brush)
            : base(adornedElement)
        {
            _positionX = positionX;
            _positionY = positionY;
            _height = height;
            _orientation = orientation;
            _brush = brush;
            _pen = new Pen(brush, 2) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (_orientation == Orientation.Horizontal)
            {
                var startPoint = new Point(_positionX, _positionY + 2);
                var endPoint = new Point(_positionX, _positionY + _height - 2);
                drawingContext.DrawLine(_pen, startPoint, endPoint);

                DrawTriangle(drawingContext, new Point(_positionX, _positionY), true);
                DrawTriangle(drawingContext, new Point(_positionX, _positionY + _height), false);
            }
            else
            {
                Size renderSize = AdornedElement.RenderSize;
                var startPoint = new Point(2, _positionY);
                var endPoint = new Point(renderSize.Width - 2, _positionY);
                drawingContext.DrawLine(_pen, startPoint, endPoint);

                DrawTriangleHorizontal(drawingContext, new Point(0, _positionY), true);
                DrawTriangleHorizontal(drawingContext, new Point(renderSize.Width, _positionY), false);
            }
        }

        private void DrawTriangle(DrawingContext dc, Point tip, bool pointDown)
        {
            const double size = 4;
            var direction = pointDown ? 1 : -1;

            var geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(tip, true, true);
                ctx.LineTo(new Point(tip.X - size, tip.Y + (size * direction)), false, false);
                ctx.LineTo(new Point(tip.X + size, tip.Y + (size * direction)), false, false);
            }

            geometry.Freeze();
            dc.DrawGeometry(_brush, null, geometry);
        }

        private void DrawTriangleHorizontal(DrawingContext dc, Point tip, bool pointRight)
        {
            const double size = 4;
            var direction = pointRight ? 1 : -1;

            var geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(tip, true, true);
                ctx.LineTo(new Point(tip.X + (size * direction), tip.Y - size), false, false);
                ctx.LineTo(new Point(tip.X + (size * direction), tip.Y + size), false, false);
            }

            geometry.Freeze();
            dc.DrawGeometry(_brush, null, geometry);
        }
    }
}
