// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;

using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents a TextBox control in a Ribbon interface.
/// Extended <see cref="Wpf.Ui.Controls.TextBox"/> with Ribbon layout support.
/// </summary>
[ContentProperty(nameof(Header))]
[DebuggerDisplay("{GetType().FullName}: Header = {Header}, IsSimplified = {IsSimplified}")]
public class RibbonTextBox : TextBox, IRibbonControl, ISimplifiedStateControl
{
    /// <inheritdoc />
    public RibbonControlSize Size
    {
        get => (RibbonControlSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Identifies the <see cref="Size"/> dependency property.</summary>
    public static readonly DependencyProperty SizeProperty = RibbonProperties.SizeProperty.AddOwner(typeof(RibbonTextBox));

    /// <inheritdoc />
    public RibbonControlSizeDefinition SizeDefinition
    {
        get => (RibbonControlSizeDefinition)GetValue(SizeDefinitionProperty);
        set => SetValue(SizeDefinitionProperty, value);
    }

    /// <summary>Identifies the <see cref="SizeDefinition"/> dependency property.</summary>
    public static readonly DependencyProperty SizeDefinitionProperty = RibbonProperties.SizeDefinitionProperty.AddOwner(typeof(RibbonTextBox));

    /// <inheritdoc />
    public RibbonControlSizeDefinition SimplifiedSizeDefinition
    {
        get => (RibbonControlSizeDefinition)GetValue(SimplifiedSizeDefinitionProperty);
        set => SetValue(SimplifiedSizeDefinitionProperty, value);
    }

    /// <summary>Identifies the <see cref="SimplifiedSizeDefinition"/> dependency property.</summary>
    public static readonly DependencyProperty SimplifiedSizeDefinitionProperty = RibbonProperties.SimplifiedSizeDefinitionProperty.AddOwner(typeof(RibbonTextBox));

    /// <inheritdoc />
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>Identifies the <see cref="Header"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderProperty = RibbonControl.HeaderProperty.AddOwner(
        typeof(RibbonTextBox),
        new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <inheritdoc />
    public DataTemplate? HeaderTemplate
    {
        get => (DataTemplate?)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>Identifies the <see cref="HeaderTemplate"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderTemplateProperty = RibbonControl.HeaderTemplateProperty.AddOwner(
        typeof(RibbonTextBox),
        new PropertyMetadata());

    /// <inheritdoc />
    public DataTemplateSelector? HeaderTemplateSelector
    {
        get => (DataTemplateSelector?)GetValue(HeaderTemplateSelectorProperty);
        set => SetValue(HeaderTemplateSelectorProperty, value);
    }

    /// <summary>Identifies the <see cref="HeaderTemplateSelector"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderTemplateSelectorProperty = RibbonControl.HeaderTemplateSelectorProperty.AddOwner(
        typeof(RibbonTextBox),
        new PropertyMetadata());

    // Note: Icon property is inherited from Wpf.Ui.Controls.TextBox
    // Explicit interface implementation to satisfy IRibbonControl
    object? IRibbonControl.Icon
    {
        get => Icon;
        set => SetValue(IconProperty, value as IconElement);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this control should be shown in Simplified mode
    /// </summary>
    public bool ShowInSimplified
    {
        get => (bool)GetValue(ShowInSimplifiedProperty);
        set => SetValue(ShowInSimplifiedProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="ShowInSimplified"/> dependency property.</summary>
    public static readonly DependencyProperty ShowInSimplifiedProperty =
        DependencyProperty.Register(
            nameof(ShowInSimplified),
            typeof(bool),
            typeof(RibbonTextBox),
            new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets a value indicating whether the ribbon is in Simplified mode
    /// </summary>
    public bool IsSimplified
    {
        get => (bool)GetValue(IsSimplifiedProperty);
        private set => SetValue(IsSimplifiedPropertyKey, BooleanBoxes.Box(value));
    }

    private static readonly DependencyPropertyKey IsSimplifiedPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsSimplified),
            typeof(bool),
            typeof(RibbonTextBox),
            new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>Identifies the <see cref="IsSimplified"/> dependency property.</summary>
    public static readonly DependencyProperty IsSimplifiedProperty = IsSimplifiedPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets or sets the width of the TextBox in Classic mode
    /// </summary>
    public double TextBoxWidth
    {
        get => (double)GetValue(TextBoxWidthProperty);
        set => SetValue(TextBoxWidthProperty, value);
    }

    /// <summary>Identifies the <see cref="TextBoxWidth"/> dependency property.</summary>
    public static readonly DependencyProperty TextBoxWidthProperty =
        DependencyProperty.Register(
            nameof(TextBoxWidth),
            typeof(double),
            typeof(RibbonTextBox),
            new PropertyMetadata(150.0));

    /// <summary>
    /// Gets or sets the width of the TextBox in Simplified mode
    /// </summary>
    public double SimplifiedTextBoxWidth
    {
        get => (double)GetValue(SimplifiedTextBoxWidthProperty);
        set => SetValue(SimplifiedTextBoxWidthProperty, value);
    }

    /// <summary>Identifies the <see cref="SimplifiedTextBoxWidth"/> dependency property.</summary>
    public static readonly DependencyProperty SimplifiedTextBoxWidthProperty =
        DependencyProperty.Register(
            nameof(SimplifiedTextBoxWidth),
            typeof(double),
            typeof(RibbonTextBox),
            new PropertyMetadata(120.0));

    /// <summary>
    /// Initializes static members of the <see cref="RibbonTextBox"/> class.
    /// </summary>
    static RibbonTextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonTextBox), new FrameworkPropertyMetadata(typeof(RibbonTextBox)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonTextBox"/> class.
    /// </summary>
    public RibbonTextBox()
    {
    }

    /// <inheritdoc />
    public void UpdateSimplifiedState(bool isSimplified)
    {
        IsSimplified = isSimplified;
    }

    /// <inheritdoc />
    void ILogicalChildSupport.AddLogicalChild(object child)
    {
        AddLogicalChild(child);
    }

    /// <inheritdoc />
    void ILogicalChildSupport.RemoveLogicalChild(object child)
    {
        RemoveLogicalChild(child);
    }

    /// <inheritdoc />
    protected override IEnumerator LogicalChildren
    {
        get
        {
            IEnumerator? baseEnumerator = base.LogicalChildren;
            while (baseEnumerator?.MoveNext() == true)
            {
                yield return baseEnumerator.Current;
            }

            if (Header is not null)
            {
                yield return Header;
            }

            if (Icon is not null)
            {
                yield return Icon;
            }
        }
    }
}
