// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// Represents an individual step in a <see cref="StepProgressBar"/> control.
/// </summary>
[StyleTypedProperty(Property = nameof(Style), StyleTargetType = typeof(StepProgressBarItem))]
public class StepProgressBarItem : System.Windows.Controls.ContentControl
{
    /// <summary>Identifies the <see cref="Description"/> dependency property.</summary>
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description),
        typeof(object),
        typeof(StepProgressBarItem),
        new PropertyMetadata(null)
    );

    /// <summary>Identifies the <see cref="Icon"/> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(IconElement),
        typeof(StepProgressBarItem),
        new PropertyMetadata(null, null, IconElement.Coerce)
    );

    /// <summary>Identifies the <see cref="Status"/> dependency property.</summary>
    public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
        nameof(Status),
        typeof(StepProgressBarItemStatus),
        typeof(StepProgressBarItem),
        new PropertyMetadata(StepProgressBarItemStatus.NotStarted)
    );

    /// <summary>Identifies the <see cref="Index"/> dependency property.</summary>
    public static readonly DependencyProperty IndexProperty = DependencyProperty.Register(
        nameof(Index),
        typeof(int),
        typeof(StepProgressBarItem),
        new PropertyMetadata(0)
    );

    /// <summary>Identifies the <see cref="Orientation"/> dependency property.</summary>
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        nameof(Orientation),
        typeof(System.Windows.Controls.Orientation),
        typeof(StepProgressBarItem),
        new PropertyMetadata(System.Windows.Controls.Orientation.Horizontal)
    );

    /// <summary>
    /// Gets or sets secondary descriptive text displayed below the main label.
    /// </summary>
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon displayed when the step is in <see cref="StepProgressBarItemStatus.Completed"/> status.
    /// When <see langword="null"/> a default checkmark icon is used.
    /// </summary>
    public IconElement? Icon
    {
        get => (IconElement?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the current progress status of this step.
    /// This is normally managed by the parent <see cref="StepProgressBar"/>.
    /// </summary>
    public StepProgressBarItemStatus Status
    {
        get => (StepProgressBarItemStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Gets or sets the zero-based index of this step within the parent <see cref="StepProgressBar"/>.
    /// This is normally managed by the parent <see cref="StepProgressBar"/>.
    /// </summary>
    public int Index
    {
        get => (int)GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }

    /// <summary>
    /// Gets or sets the orientation inherited from the parent <see cref="StepProgressBar"/>.
    /// This is normally managed by the parent <see cref="StepProgressBar"/>.
    /// </summary>
    public System.Windows.Controls.Orientation Orientation
    {
        get => (System.Windows.Controls.Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
}
