// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections;
using System.Diagnostics;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Markup;

using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents button
/// </summary>
[ContentProperty(nameof(Header))]
[DebuggerDisplay("{GetType().FullName}: Header = {Header}, Size = {Size}, IsSimplified = {IsSimplified}")]
public class RibbonButton : System.Windows.Controls.Button, IRibbonControl, ILargeIconProvider, IMediumIconProvider, ISimplifiedStateControl
{
    /// <inheritdoc />
    public RibbonControlSize Size
    {
        get => (RibbonControlSize)this.GetValue(SizeProperty);
        set => this.SetValue(SizeProperty, value);
    }

    /// <summary>Identifies the <see cref="Size"/> dependency property.</summary>
    public static readonly DependencyProperty SizeProperty = RibbonProperties.SizeProperty.AddOwner(typeof(RibbonButton));

    /// <inheritdoc />
    public RibbonControlSizeDefinition SizeDefinition
    {
        get => (RibbonControlSizeDefinition)this.GetValue(SizeDefinitionProperty);
        set => this.SetValue(SizeDefinitionProperty, value);
    }

    /// <summary>Identifies the <see cref="SizeDefinition"/> dependency property.</summary>
    public static readonly DependencyProperty SizeDefinitionProperty = RibbonProperties.SizeDefinitionProperty.AddOwner(typeof(RibbonButton));

    /// <inheritdoc />
    public RibbonControlSizeDefinition SimplifiedSizeDefinition
    {
        get => (RibbonControlSizeDefinition)this.GetValue(SimplifiedSizeDefinitionProperty);
        set => this.SetValue(SimplifiedSizeDefinitionProperty, value);
    }

    /// <summary>Identifies the <see cref="SimplifiedSizeDefinition"/> dependency property.</summary>
    public static readonly DependencyProperty SimplifiedSizeDefinitionProperty = RibbonProperties.SimplifiedSizeDefinitionProperty.AddOwner(typeof(RibbonButton));

    /// <inheritdoc />
    public object? Header
    {
        get => this.GetValue(HeaderProperty);
        set => this.SetValue(HeaderProperty, value);
    }

    /// <summary>Identifies the <see cref="Header"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderProperty = RibbonControl.HeaderProperty.AddOwner(typeof(RibbonButton), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <inheritdoc />
    public DataTemplate? HeaderTemplate
    {
        get => (DataTemplate?)this.GetValue(HeaderTemplateProperty);
        set => this.SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>Identifies the <see cref="HeaderTemplate"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderTemplateProperty = RibbonControl.HeaderTemplateProperty.AddOwner(typeof(RibbonButton), new PropertyMetadata());

    /// <inheritdoc />
    public DataTemplateSelector? HeaderTemplateSelector
    {
        get => (DataTemplateSelector?)this.GetValue(HeaderTemplateSelectorProperty);
        set => this.SetValue(HeaderTemplateSelectorProperty, value);
    }

    /// <summary>Identifies the <see cref="HeaderTemplateSelector"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderTemplateSelectorProperty = RibbonControl.HeaderTemplateSelectorProperty.AddOwner(typeof(RibbonButton), new PropertyMetadata());

    /// <inheritdoc />
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Localizable(false)]
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Identifies the <see cref="Icon"/> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = RibbonControl.IconProperty.AddOwner(typeof(RibbonButton), new FrameworkPropertyMetadata(
            default(object),
            FrameworkPropertyMetadataOptions.None,
            LogicalChildSupportHelper.OnLogicalChildPropertyChanged,
            IconElement.Coerce));

    /// <inheritdoc />
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Localizable(false)]
    public object? LargeIcon
    {
        get => this.GetValue(LargeIconProperty);
        set => this.SetValue(LargeIconProperty, value);
    }

    /// <summary>Identifies the <see cref="LargeIcon"/> dependency property.</summary>
    public static readonly DependencyProperty LargeIconProperty = LargeIconProviderProperties.LargeIconProperty.AddOwner(typeof(RibbonButton), new FrameworkPropertyMetadata(
            default,
            FrameworkPropertyMetadataOptions.None,
            LogicalChildSupportHelper.OnLogicalChildPropertyChanged,
            IconElement.Coerce));

    /// <inheritdoc />
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Localizable(false)]
    public object? MediumIcon
    {
        get => this.GetValue(MediumIconProperty);
        set => this.SetValue(MediumIconProperty, value);
    }

    /// <summary>Identifies the <see cref="MediumIcon"/> dependency property.</summary>
    public static readonly DependencyProperty MediumIconProperty = MediumIconProviderProperties.MediumIconProperty.AddOwner(typeof(RibbonButton), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether ribbon control click must close backstage
    /// </summary>
    public bool IsDefinitive
    {
        get => (bool)this.GetValue(IsDefinitiveProperty);
        set => this.SetValue(IsDefinitiveProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsDefinitive"/> dependency property.</summary>
    public static readonly DependencyProperty IsDefinitiveProperty =
        DependencyProperty.Register(nameof(IsDefinitive), typeof(bool), typeof(RibbonButton), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets the ControlAppearance.
    /// </summary>
    public ControlAppearance Appearance
    {
        get => (ControlAppearance)this.GetValue(AppearanceProperty);
        set => this.SetValue(AppearanceProperty, value);
    }

    /// <summary>Identifies the <see cref="Appearance"/> dependency property.</summary>
    public static readonly DependencyProperty AppearanceProperty = DependencyProperty.Register(
        nameof(Appearance),
        typeof(ControlAppearance),
        typeof(RibbonButton),
        new PropertyMetadata(ControlAppearance.Primary));

    /// <summary>
    /// Gets a value indicating whether gets or sets whether or not the ribbon is in Simplified mode
    /// </summary>
    public bool IsSimplified
    {
        get => (bool)this.GetValue(IsSimplifiedProperty);
        private set => this.SetValue(IsSimplifiedPropertyKey, BooleanBoxes.Box(value));
    }

    private static readonly DependencyPropertyKey IsSimplifiedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsSimplified), typeof(bool), typeof(RibbonButton), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>Identifies the <see cref="IsSimplified"/> dependency property.</summary>
    public static readonly DependencyProperty IsSimplifiedProperty = IsSimplifiedPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets or sets a value indicating whether this control should be shown in Simplified mode
    /// </summary>
    public bool ShowInSimplified
    {
        get => (bool)this.GetValue(ShowInSimplifiedProperty);
        set => this.SetValue(ShowInSimplifiedProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="ShowInSimplified"/> dependency property.</summary>
    public static readonly DependencyProperty ShowInSimplifiedProperty =
        DependencyProperty.Register(nameof(ShowInSimplified), typeof(bool), typeof(RibbonButton), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>Identifies the <see cref="CornerRadius"/> dependency property.</summary>
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius),
        typeof(CornerRadius),
        typeof(RibbonButton),
        new FrameworkPropertyMetadata(
            default(CornerRadius),
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender
        )
    );

    /// <summary>
    /// Gets or sets a value that represents the degree to which the corners of a <see cref="T:System.Windows.Controls.Border" /> are rounded.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Initializes static members of the <see cref="RibbonButton"/> class.
    /// Static constructor
    /// </summary>
    static RibbonButton()
    {
        Type type = typeof(Button);
        DefaultStyleKeyProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(type));

        // ContextMenuService.Attach(type);
        // ToolTipService.Attach(type);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonButton"/> class.
    /// Default constructor
    /// </summary>
    public RibbonButton()
    {
        // ContextMenuService.Coerce(this);
    }

    /// <inheritdoc />
    protected override void OnClick()
    {
        // Close popup on click
        if (this.IsDefinitive)
        {
            PopupService.RaiseDismissPopupEvent(this, DismissPopupMode.Always);
        }

        base.OnClick();
    }

    /// <inheritdoc />
    public void UpdateSimplifiedState(bool isSimplified)
    {
        this.IsSimplified = isSimplified;
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
            System.Collections.IEnumerator baseEnumerator = base.LogicalChildren;
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
        }
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer() => new RibbonButtonAutomationPeer(this);
}