// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// VS2022/2026 style toolbar group container that visually groups related toolbar items
/// with a subtle border and background.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;ui:ToolBarGroup&gt;
///     &lt;ui:Button Icon="{ui:SymbolIcon Copy20}" ToolTip="Copy" /&gt;
///     &lt;ui:Button Icon="{ui:SymbolIcon Cut20}" ToolTip="Cut" /&gt;
///     &lt;ui:ToolBarGroupSeparator /&gt;
///     &lt;ui:Button Icon="{ui:SymbolIcon Paste20}" ToolTip="Paste" /&gt;
/// &lt;/ui:ToolBarGroup&gt;
/// </code>
/// </example>
public class ToolBarGroup : ItemsControl
{
    /// <summary>Identifies the <see cref="Header"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header),
        typeof(object),
        typeof(ToolBarGroup),
        new PropertyMetadata(null)
    );

    /// <summary>Identifies the <see cref="Orientation"/> dependency property.</summary>
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        nameof(Orientation),
        typeof(Orientation),
        typeof(ToolBarGroup),
        new PropertyMetadata(Orientation.Horizontal)
    );

    /// <summary>Identifies the <see cref="CornerRadius"/> dependency property.</summary>
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius),
        typeof(CornerRadius),
        typeof(ToolBarGroup),
        new PropertyMetadata(new CornerRadius(4))
    );

    /// <summary>Identifies the <see cref="ShowBorder"/> dependency property.</summary>
    public static readonly DependencyProperty ShowBorderProperty = DependencyProperty.Register(
        nameof(ShowBorder),
        typeof(bool),
        typeof(ToolBarGroup),
        new PropertyMetadata(true)
    );

    /// <summary>Identifies the <see cref="ItemSpacing"/> dependency property.</summary>
    public static readonly DependencyProperty ItemSpacingProperty = DependencyProperty.Register(
        nameof(ItemSpacing),
        typeof(double),
        typeof(ToolBarGroup),
        new PropertyMetadata(2.0)
    );

    /// <summary>Identifies the <see cref="IsDraggable"/> dependency property.</summary>
    public static readonly DependencyProperty IsDraggableProperty = DependencyProperty.Register(
        nameof(IsDraggable),
        typeof(bool),
        typeof(ToolBarGroup),
        new PropertyMetadata(false)
    );

    /// <summary>Identifies the <see cref="ShowGrip"/> dependency property.</summary>
    public static readonly DependencyProperty ShowGripProperty = DependencyProperty.Register(
        nameof(ShowGrip),
        typeof(bool),
        typeof(ToolBarGroup),
        new PropertyMetadata(false)
    );

    /// <summary>
    /// Gets or sets the header content for the group (optional, for labeled groups).
    /// </summary>
    [Bindable(true)]
    [Category("Content")]
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the orientation of items within the group.
    /// </summary>
    [Bindable(true)]
    [Category("Layout")]
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the corner radius of the group border.
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to show the group border.
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public bool ShowBorder
    {
        get => (bool)GetValue(ShowBorderProperty);
        set => SetValue(ShowBorderProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between items in the group.
    /// </summary>
    [Bindable(true)]
    [Category("Layout")]
    public double ItemSpacing
    {
        get => (double)GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the group can be dragged to reorder.
    /// </summary>
    [Bindable(true)]
    [Category("Behavior")]
    public bool IsDraggable
    {
        get => (bool)GetValue(IsDraggableProperty);
        set => SetValue(IsDraggableProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to show the drag grip handle.
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public bool ShowGrip
    {
        get => (bool)GetValue(ShowGripProperty);
        set => SetValue(ShowGripProperty, value);
    }

    private const string TemplateElementGrip = "PART_DragGrip";
    private Thumb? _dragGrip;
    private Point _dragStartPoint;
    private bool _isDragging;

    static ToolBarGroup()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToolBarGroup),
            new FrameworkPropertyMetadata(typeof(ToolBarGroup))
        );
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_dragGrip is not null)
        {
            _dragGrip.DragStarted -= OnDragGripDragStarted;
            _dragGrip.DragDelta -= OnDragGripDragDelta;
            _dragGrip.DragCompleted -= OnDragGripDragCompleted;
        }

        _dragGrip = GetTemplateChild(TemplateElementGrip) as Thumb;

        if (_dragGrip is not null)
        {
            _dragGrip.DragStarted += OnDragGripDragStarted;
            _dragGrip.DragDelta += OnDragGripDragDelta;
            _dragGrip.DragCompleted += OnDragGripDragCompleted;
        }
    }

    private void OnDragGripDragStarted(object sender, DragStartedEventArgs e)
    {
        if (!IsDraggable)
        {
            return;
        }

        _isDragging = true;
        _dragStartPoint = Mouse.GetPosition(this);

        // Start drag-drop operation
        var data = new DataObject(typeof(ToolBarGroup), this);
        DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
    }

    private void OnDragGripDragDelta(object sender, DragDeltaEventArgs e)
    {
        // Visual feedback during drag can be added here
    }

    private void OnDragGripDragCompleted(object sender, DragCompletedEventArgs e)
    {
        _isDragging = false;
    }
}
