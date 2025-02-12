// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.Controls.Extensibility;
using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls;

/// <summary>
/// Attached Properties for the Fluent Ribbon library
/// </summary>
public class RibbonProperties : DependencyObject
{
    /// <summary>
    /// Using a DependencyProperty as the backing store for Size.
    /// This enables animation, styling, binding, etc...
    /// </summary>
    public static readonly DependencyProperty SizeProperty =
    DependencyProperty.RegisterAttached(
        "Size",
        typeof(RibbonControlSize),
        typeof(RibbonProperties),
        new FrameworkPropertyMetadata(
            RibbonControlSize.Large,
            FrameworkPropertyMetadataOptions.AffectsArrange |
            FrameworkPropertyMetadataOptions.AffectsMeasure |
            FrameworkPropertyMetadataOptions.AffectsRender |
            FrameworkPropertyMetadataOptions.AffectsParentArrange |
            FrameworkPropertyMetadataOptions.AffectsParentMeasure,
            OnSizeChanged));

    /// <summary>
    /// Sets <see cref="SizeProperty"/> for <paramref name="element"/>.
    /// </summary>
    public static void SetSize(DependencyObject element, RibbonControlSize value)
    {
        element.SetValue(SizeProperty, value);
    }

    /// <summary>
    /// Gets <see cref="SizeProperty"/> for <paramref name="element"/>.
    /// </summary>
    // [AttachedPropertyBrowsableForType(typeof(IRibbonControl))]
    public static RibbonControlSize GetSize(DependencyObject element)
    {
        return (RibbonControlSize)element.GetValue(SizeProperty);
    }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var sink = d as IRibbonSizeChangedSink;

        sink?.OnSizePropertyChanged((RibbonControlSize)e.OldValue, (RibbonControlSize)e.NewValue);
    }

    /// <summary>
    /// Using a DependencyProperty as the backing store for SizeDefinition.
    /// This enables animation, styling, binding, etc...
    /// </summary>
    public static readonly DependencyProperty SizeDefinitionProperty =
        DependencyProperty.RegisterAttached(
            "SizeDefinition",
            typeof(RibbonControlSizeDefinition),
            typeof(RibbonProperties),
            new FrameworkPropertyMetadata(
                new RibbonControlSizeDefinition(RibbonControlSize.Large, RibbonControlSize.Middle, RibbonControlSize.Small),
                FrameworkPropertyMetadataOptions.AffectsArrange |
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsParentArrange |
                FrameworkPropertyMetadataOptions.AffectsParentMeasure,
                OnSizeDefinitionChanged));

    /// <summary>
    /// Sets <see cref="SizeDefinitionProperty"/> for <paramref name="element"/>.
    /// </summary>
    public static void SetSizeDefinition(DependencyObject element, RibbonControlSizeDefinition value)
    {
        element.SetValue(SizeDefinitionProperty, value);
    }

    /// <summary>
    /// Gets <see cref="SizeDefinitionProperty"/> for <paramref name="element"/>.
    /// </summary>
    // [AttachedPropertyBrowsableForType(typeof(IRibbonControl))]
    public static RibbonControlSizeDefinition GetSizeDefinition(DependencyObject element)
    {
        return (RibbonControlSizeDefinition)element.GetValue(SizeDefinitionProperty);
    }

    // Handles RibbonSizeDefinitionProperty changes
    internal static void OnSizeDefinitionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Find parent group box
        RibbonGroupBox? groupBox = FindParentRibbonGroupBox(d);
        var element = (UIElement)d;
        var isSimplified = groupBox?.IsSimplified ?? false;

        if (!isSimplified)
        {
            SetAppropriateSize(element, groupBox?.State ?? RibbonGroupBoxState.Large, isSimplified);
        }
    }

    // Finds parent group box
    internal static RibbonGroupBox? FindParentRibbonGroupBox(DependencyObject element)
    {
        DependencyObject? currentElement = element;
        RibbonGroupBox? groupBox;

        while ((groupBox = currentElement as RibbonGroupBox) is null)
        {
            currentElement = VisualTreeHelper.GetParent(currentElement)
                             ?? LogicalTreeHelper.GetParent(currentElement);

            if (currentElement is null)
            {
                break;
            }
        }

        return groupBox;
    }

    /// <summary>
    /// Sets appropriate size of the control according to the
    /// given group box state and control's size definition
    /// </summary>
    /// <param name="element">UI Element</param>
    /// <param name="state">Group box state</param>
    /// <param name="isSimplified">Group box isSimplified state</param>
    public static void SetAppropriateSize(DependencyObject element, RibbonGroupBoxState state, bool isSimplified)
    {
        RibbonControlSizeDefinition sizeDefinition = isSimplified ? GetSimplifiedSizeDefinition(element) : GetSizeDefinition(element);
        SetSize(element, sizeDefinition.GetSize(state));
    }

    /// <summary>
    /// Using a DependencyProperty as the backing store for SimplifiedSizeDefinition.
    /// This enables animation, styling, binding, etc...
    /// </summary>
    public static readonly DependencyProperty SimplifiedSizeDefinitionProperty =
        DependencyProperty.RegisterAttached(
            nameof(ISimplifiedRibbonControl.SimplifiedSizeDefinition),
            typeof(RibbonControlSizeDefinition),
            typeof(RibbonProperties),
            new FrameworkPropertyMetadata(
                new RibbonControlSizeDefinition(
                    RibbonControlSize.Large,
                    RibbonControlSize.Middle,
                    RibbonControlSize.Small),
                FrameworkPropertyMetadataOptions.AffectsArrange |
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsParentArrange |
                FrameworkPropertyMetadataOptions.AffectsParentMeasure,
                OnSimplifiedSizeDefinitionChanged));

    /// <summary>
    /// Sets <see cref="SimplifiedSizeDefinitionProperty"/> for <paramref name="element"/>.
    /// </summary>
    public static void SetSimplifiedSizeDefinition(DependencyObject element, RibbonControlSizeDefinition value)
    {
        element.SetValue(SimplifiedSizeDefinitionProperty, value);
    }

    /// <summary>
    /// Gets <see cref="SimplifiedSizeDefinitionProperty"/> for <paramref name="element"/>.
    /// </summary>
    // [AttachedPropertyBrowsableForType(typeof(ISimplifiedRibbonControl))]
    public static RibbonControlSizeDefinition GetSimplifiedSizeDefinition(DependencyObject element)
    {
        return (RibbonControlSizeDefinition)element.GetValue(SimplifiedSizeDefinitionProperty);
    }

    // Handles RibbonSizeDefinitionProperty changes
    internal static void OnSimplifiedSizeDefinitionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Find parent group box
        RibbonGroupBox? groupBox = FindParentRibbonGroupBox(d);
        var element = (UIElement)d;
        var isSimplified = groupBox?.IsSimplified ?? false;

        if (isSimplified)
        {
            SetAppropriateSize(element, groupBox?.State ?? RibbonGroupBoxState.Large, isSimplified);
        }
    }

    /// <summary>
    /// Sets appropriate size of the control according to the
    /// given ribbon control size and control's size definition
    /// </summary>
    /// <param name="element">UI Element</param>
    /// <param name="size">Ribbon control size before applying SizeDefinition</param>
    public static void SetAppropriateSize(DependencyObject element, RibbonControlSize size)
    {
        SetSize(element, GetSizeDefinition(element).GetSize(size));
    }

    /// <summary>
    /// <see cref="DependencyProperty"/> for specifying MouseOverBackground.
    /// </summary>
    public static readonly DependencyProperty MouseOverBackgroundProperty = DependencyProperty.RegisterAttached("MouseOverBackground", typeof(Brush), typeof(RibbonProperties), new PropertyMetadata(default(Brush)));

    /// <summary>
    /// Sets <see cref="MouseOverBackgroundProperty"/> for <paramref name="element"/>.
    /// </summary>
    public static void SetMouseOverBackground(DependencyObject element, Brush? value)
    {
        element.SetValue(MouseOverBackgroundProperty, value);
    }

    /// <summary>
    /// Gets <see cref="MouseOverBackgroundProperty"/> for <paramref name="element"/>.
    /// </summary>
    // [AttachedPropertyBrowsableForType(typeof(IRibbonControl))]
    public static Brush? GetMouseOverBackground(DependencyObject element)
    {
        return (Brush)element.GetValue(MouseOverBackgroundProperty);
    }

    /// <summary>
    /// <see cref="DependencyProperty"/> for specifying PressedBackground.
    /// </summary>
    public static readonly DependencyProperty PressedBackgroundProperty = DependencyProperty.RegisterAttached("PressedBackground", typeof(Brush), typeof(RibbonProperties), new PropertyMetadata(default(Brush)));

    /// <summary>
    /// Sets <see cref="PressedBackgroundProperty"/> for <paramref name="element"/>.
    /// </summary>
    public static void SetPressedBackground(DependencyObject element, Brush? value)
    {
        element.SetValue(PressedBackgroundProperty, value);
    }

    /// <summary>
    /// Gets <see cref="PressedBackgroundProperty"/> for <paramref name="element"/>.
    /// </summary>
    // [AttachedPropertyBrowsableForType(typeof(IRibbonControl))]
    public static Brush? GetPressedBackground(DependencyObject element)
    {
        return (Brush)element.GetValue(PressedBackgroundProperty);
    }

    /// <summary>
    /// <see cref="DependencyProperty"/> for specifying MouseOverForeground.
    /// </summary>
    public static readonly DependencyProperty MouseOverForegroundProperty = DependencyProperty.RegisterAttached("MouseOverForeground", typeof(Brush), typeof(RibbonProperties), new PropertyMetadata(default(Brush)));

    /// <summary>
    /// Sets <see cref="MouseOverForegroundProperty"/> for <paramref name="element"/>.
    /// </summary>
    public static void SetMouseOverForeground(DependencyObject element, Brush? value)
    {
        element.SetValue(MouseOverForegroundProperty, value);
    }

    /// <summary>
    /// Gets <see cref="MouseOverForegroundProperty"/> for <paramref name="element"/>.
    /// </summary>
    // [AttachedPropertyBrowsableForType(typeof(IRibbonControl))]
    public static Brush? GetMouseOverForeground(DependencyObject element)
    {
        return (Brush?)element.GetValue(MouseOverForegroundProperty);
    }

    /// <summary>
    /// <see cref="DependencyProperty"/> for specifying IsSelectedBackground.
    /// </summary>
    public static readonly DependencyProperty IsSelectedBackgroundProperty = DependencyProperty.RegisterAttached("IsSelectedBackground", typeof(Brush), typeof(RibbonProperties), new PropertyMetadata(default(Brush)));

    /// <summary>
    /// Sets <see cref="IsSelectedBackgroundProperty"/> for <paramref name="element"/>.
    /// </summary>
    public static void SetIsSelectedBackground(DependencyObject element, Brush? value)
    {
        element.SetValue(IsSelectedBackgroundProperty, value);
    }

    /// <summary>
    /// Gets <see cref="IsSelectedBackgroundProperty"/> for <paramref name="element"/>.
    /// </summary>
    // [AttachedPropertyBrowsableForType(typeof(IRibbonControl))]
    public static Brush? GetIsSelectedBackground(DependencyObject element)
    {
        return (Brush?)element.GetValue(IsSelectedBackgroundProperty);
    }

    /// <summary>
    /// Stores the last visible width of an element.
    /// </summary>
    public static readonly DependencyProperty LastVisibleWidthProperty = DependencyProperty.RegisterAttached(
        "LastVisibleWidth", typeof(double), typeof(RibbonProperties), new PropertyMetadata(DoubleBoxes.Zero));

    /// <summary>Helper for setting <see cref="LastVisibleWidthProperty"/> on <paramref name="element"/>.</summary>
    public static void SetLastVisibleWidth(DependencyObject element, double value)
    {
        element.SetValue(LastVisibleWidthProperty, value);
    }

    /// <summary>Helper for getting <see cref="LastVisibleWidthProperty"/> on <paramref name="element"/>.</summary>
    public static double GetLastVisibleWidth(DependencyObject? element)
    {
#pragma warning disable WPF0042 // Avoid side effects in CLR accessors.
        if (element is null)
        {
            return 0;
        }
#pragma warning restore WPF0042 // Avoid side effects in CLR accessors.

        return (double)element.GetValue(LastVisibleWidthProperty);
    }
#pragma warning disable WPF0010
    /// <summary>
    /// Defines the desired icon size for the element.
    /// </summary>
    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.RegisterAttached(
        "IconSize", typeof(IconSize), typeof(RibbonProperties), new PropertyMetadata(IconSizeBoxes.Small));
#pragma warning restore WPF0010

    /// <summary>Helper for setting <see cref="IconSizeProperty"/> on <paramref name="element"/>.</summary>
    public static void SetIconSize(DependencyObject element, IconSize value)
    {
        element.SetValue(IconSizeProperty, IconSizeBoxes.Box(value));
    }

    /// <summary>Helper for getting <see cref="IconSizeProperty"/> from <paramref name="element"/>.</summary>
    [AttachedPropertyBrowsableForType(typeof(IRibbonControl))]
    [AttachedPropertyBrowsableForType(typeof(IMediumIconProvider))]
    [AttachedPropertyBrowsableForType(typeof(ILargeIconProvider))]
    public static IconSize GetIconSize(DependencyObject element)
    {
        return (IconSize)element.GetValue(IconSizeProperty);
    }

#pragma warning disable WPF0010
    /// <summary>
    /// Defines the custom icon size for the element.
    /// </summary>
    public static readonly DependencyProperty CustomIconSizeProperty = DependencyProperty.RegisterAttached(
        "CustomIconSize", typeof(Size), typeof(RibbonProperties), new PropertyMetadata(default(Size)));
#pragma warning restore WPF0010

    /// <summary>Helper for setting <see cref="CustomIconSizeProperty"/> on <paramref name="element"/>.</summary>
    public static void SetCustomIconSize(DependencyObject element, Size value)
    {
        element.SetValue(CustomIconSizeProperty, value);
    }

    /// <summary>Helper for getting <see cref="CustomIconSizeProperty"/> from <paramref name="element"/>.</summary>
    [AttachedPropertyBrowsableForType(typeof(IRibbonControl))]
    [AttachedPropertyBrowsableForType(typeof(IMediumIconProvider))]
    [AttachedPropertyBrowsableForType(typeof(ILargeIconProvider))]
    public static Size GetCustomIconSize(DependencyObject element)
    {
        return (Size)element.GetValue(CustomIconSizeProperty);
    }

    /// <summary>
    /// Defines the corner radius used in template parts.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(RibbonProperties), new PropertyMetadata(default(CornerRadius)));

    /// <summary>Helper for setting <see cref="CornerRadiusProperty"/> on <paramref name="element"/>.</summary>
    public static void SetCornerRadius(DependencyObject element, CornerRadius value)
    {
        element.SetValue(CornerRadiusProperty, value);
    }

    /// <summary>Helper for getting <see cref="CornerRadiusProperty"/> on <paramref name="element"/>.</summary>
    public static CornerRadius GetCornerRadius(DependencyObject element)
    {
        return (CornerRadius)element.GetValue(CornerRadiusProperty);
    }
}