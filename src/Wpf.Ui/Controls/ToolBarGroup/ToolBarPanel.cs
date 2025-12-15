// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// A panel that hosts <see cref="ToolBarGroup"/> items and supports drag-and-drop reordering
/// with visual preview indicator.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;ui:ToolBarPanel Orientation="Horizontal"&gt;
///     &lt;ui:ToolBarGroup ShowGrip="True" IsDraggable="True"&gt;
///         &lt;ui:ToolBarButton Icon="{ui:SymbolIcon Copy20}" /&gt;
///     &lt;/ui:ToolBarGroup&gt;
///     &lt;ui:ToolBarGroup ShowGrip="True" IsDraggable="True"&gt;
///         &lt;ui:ToolBarButton Icon="{ui:SymbolIcon Paste20}" /&gt;
///     &lt;/ui:ToolBarGroup&gt;
/// &lt;/ui:ToolBarPanel&gt;
/// </code>
/// </example>
public class ToolBarPanel : StackPanel
{
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
    /// Gets or sets whether reordering via drag-and-drop is allowed.
    /// </summary>
    [Bindable(true)]
    [Category("Behavior")]
    public bool AllowReorder
    {
        get => (bool)GetValue(AllowReorderProperty);
        set => SetValue(AllowReorderProperty, value);
    }

    private Rectangle? _dropIndicator;
    private int _dropIndex = -1;
    private ToolBarGroup? _draggedItem;

    public ToolBarPanel()
    {
        AllowDrop = true;

        DragEnter += OnDragEnter;
        DragOver += OnDragOver;
        DragLeave += OnDragLeave;
        Drop += OnDrop;
    }

    private void EnsureDropIndicator()
    {
        if (_dropIndicator is not null)
        {
            return;
        }

        _dropIndicator = new Rectangle
        {
            Width = Orientation == Orientation.Horizontal ? 2 : double.NaN,
            Height = Orientation == Orientation.Horizontal ? double.NaN : 2,
            Fill = DropIndicatorBrush ?? new SolidColorBrush(Color.FromArgb(255, 0, 120, 215)),
            Visibility = Visibility.Collapsed,
            IsHitTestVisible = false,
            RadiusX = 1,
            RadiusY = 1
        };
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

        EnsureDropIndicator();
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (!AllowReorder || _draggedItem is null || _dropIndicator is null)
        {
            e.Effects = DragDropEffects.None;
            return;
        }

        e.Effects = DragDropEffects.Move;
        e.Handled = true;

        var position = e.GetPosition(this);
        var newDropIndex = CalculateDropIndex(position);

        if (newDropIndex != _dropIndex)
        {
            _dropIndex = newDropIndex;
            UpdateDropIndicatorPosition();
        }
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        HideDropIndicator();
        _dropIndex = -1;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (!AllowReorder || _draggedItem is null || _dropIndex < 0)
        {
            HideDropIndicator();
            return;
        }

        var currentIndex = Children.IndexOf(_draggedItem);
        if (currentIndex >= 0 && currentIndex != _dropIndex)
        {
            // Adjust index if dropping after current position
            var targetIndex = _dropIndex;
            if (currentIndex < _dropIndex)
            {
                targetIndex--;
            }

            // Ensure target index is valid
            targetIndex = Math.Max(0, Math.Min(targetIndex, Children.Count - 1));

            if (currentIndex != targetIndex)
            {
                Children.RemoveAt(currentIndex);
                Children.Insert(targetIndex, _draggedItem);
            }
        }

        HideDropIndicator();
        _draggedItem = null;
        _dropIndex = -1;
        e.Handled = true;
    }

    private int CalculateDropIndex(Point position)
    {
        var index = 0;
        double accumulatedPosition = 0;

        foreach (UIElement child in Children)
        {
            if (child is not FrameworkElement element)
            {
                index++;
                continue;
            }

            double elementSize;
            double elementPosition;

            if (Orientation == Orientation.Horizontal)
            {
                elementSize = element.ActualWidth + element.Margin.Left + element.Margin.Right;
                elementPosition = position.X;
            }
            else
            {
                elementSize = element.ActualHeight + element.Margin.Top + element.Margin.Bottom;
                elementPosition = position.Y;
            }

            var midPoint = accumulatedPosition + (elementSize / 2);

            if (elementPosition < midPoint)
            {
                return index;
            }

            accumulatedPosition += elementSize;
            index++;
        }

        return Children.Count;
    }

    private void UpdateDropIndicatorPosition()
    {
        if (_dropIndicator is null || _dropIndex < 0)
        {
            return;
        }

        // Remove from current position if already added
        if (_dropIndicator.Parent is Panel parent)
        {
            parent.Children.Remove(_dropIndicator);
        }

        // Calculate position based on drop index
        double indicatorPosition = 0;
        var childIndex = 0;

        foreach (UIElement child in Children)
        {
            if (childIndex >= _dropIndex)
            {
                break;
            }

            if (child is FrameworkElement element)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    indicatorPosition += element.ActualWidth + element.Margin.Left + element.Margin.Right;
                }
                else
                {
                    indicatorPosition += element.ActualHeight + element.Margin.Top + element.Margin.Bottom;
                }
            }

            childIndex++;
        }

        // Use adorner layer for the indicator
        var adornerLayer = AdornerLayer.GetAdornerLayer(this);
        if (adornerLayer is not null)
        {
            // Remove existing adorner
            var existingAdorners = adornerLayer.GetAdorners(this);
            if (existingAdorners is not null)
            {
                foreach (var adorner in existingAdorners.OfType<DropIndicatorAdorner>())
                {
                    adornerLayer.Remove(adorner);
                }
            }

            // Add new adorner
            var indicatorAdorner = new DropIndicatorAdorner(
                this,
                indicatorPosition,
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

        var existingAdorners = adornerLayer.GetAdorners(this);
        if (existingAdorners is null)
        {
            return;
        }

        foreach (var adorner in existingAdorners.OfType<DropIndicatorAdorner>())
        {
            adornerLayer.Remove(adorner);
        }
    }

    /// <summary>
    /// Adorner that displays the drop indicator line.
    /// </summary>
    private sealed class DropIndicatorAdorner : Adorner
    {
        private readonly double _position;
        private readonly Orientation _orientation;
        private readonly Brush _brush;
        private readonly Pen _pen;

        public DropIndicatorAdorner(
            UIElement adornedElement,
            double position,
            Orientation orientation,
            Brush brush)
            : base(adornedElement)
        {
            _position = position;
            _orientation = orientation;
            _brush = brush;
            _pen = new Pen(brush, 2) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var renderSize = AdornedElement.RenderSize;

            if (_orientation == Orientation.Horizontal)
            {
                // Vertical line for horizontal orientation
                var startPoint = new Point(_position, 2);
                var endPoint = new Point(_position, renderSize.Height - 2);
                drawingContext.DrawLine(_pen, startPoint, endPoint);

                // Draw small triangles at top and bottom
                DrawTriangle(drawingContext, new Point(_position, 0), true);
                DrawTriangle(drawingContext, new Point(_position, renderSize.Height), false);
            }
            else
            {
                // Horizontal line for vertical orientation
                var startPoint = new Point(2, _position);
                var endPoint = new Point(renderSize.Width - 2, _position);
                drawingContext.DrawLine(_pen, startPoint, endPoint);

                // Draw small triangles at left and right
                DrawTriangleHorizontal(drawingContext, new Point(0, _position), true);
                DrawTriangleHorizontal(drawingContext, new Point(renderSize.Width, _position), false);
            }
        }

        private void DrawTriangle(DrawingContext dc, Point tip, bool pointDown)
        {
            const double size = 4;
            var direction = pointDown ? 1 : -1;

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
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
            using (var ctx = geometry.Open())
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
