// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections;
using System.Diagnostics;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Markup;

using Wpf.Ui.Controls.Helpers;
using Wpf.Ui.Extensions;
using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents toggle button
/// </summary>
[ContentProperty(nameof(Header))]
[DebuggerDisplay("{GetType().FullName}: Header = {Header}, IsChecked = {IsChecked}, Size = {Size}, IsSimplified = {IsSimplified}")]
public class RibbonToggleButton : System.Windows.Controls.Primitives.ToggleButton, IToggleButton, IRibbonControl, ILargeIconProvider, IMediumIconProvider, ISimplifiedRibbonControl
{
    /// <inheritdoc />
    public RibbonControlSize Size
    {
        get => (RibbonControlSize)this.GetValue(SizeProperty);
        set => this.SetValue(SizeProperty, value);
    }

    /// <summary>Identifies the <see cref="Size"/> dependency property.</summary>
    public static readonly DependencyProperty SizeProperty = RibbonProperties.SizeProperty.AddOwner(typeof(RibbonToggleButton));

    /// <inheritdoc />
    public RibbonControlSizeDefinition SizeDefinition
    {
        get => (RibbonControlSizeDefinition)this.GetValue(SizeDefinitionProperty);
        set => this.SetValue(SizeDefinitionProperty, value);
    }

    /// <summary>Identifies the <see cref="SizeDefinition"/> dependency property.</summary>
    public static readonly DependencyProperty SizeDefinitionProperty = RibbonProperties.SizeDefinitionProperty.AddOwner(typeof(RibbonToggleButton));

    /// <inheritdoc />
    public RibbonControlSizeDefinition SimplifiedSizeDefinition
    {
        get => (RibbonControlSizeDefinition)this.GetValue(SimplifiedSizeDefinitionProperty);
        set => this.SetValue(SimplifiedSizeDefinitionProperty, value);
    }

    /// <summary>Identifies the <see cref="SimplifiedSizeDefinition"/> dependency property.</summary>
    public static readonly DependencyProperty SimplifiedSizeDefinitionProperty = RibbonProperties.SimplifiedSizeDefinitionProperty.AddOwner(typeof(RibbonToggleButton));

    /// <inheritdoc />
    public string? GroupName
    {
        get => (string?)this.GetValue(GroupNameProperty);
        set => this.SetValue(GroupNameProperty, value);
    }

    /// <summary>Identifies the <see cref="GroupName"/> dependency property.</summary>
    public static readonly DependencyProperty GroupNameProperty =
        DependencyProperty.Register(nameof(GroupName), typeof(string), typeof(RibbonToggleButton), new PropertyMetadata(ToggleButtonHelper.OnGroupNameChanged));

    /// <inheritdoc />
    public object? Header
    {
        get => this.GetValue(HeaderProperty);
        set => this.SetValue(HeaderProperty, value);
    }

    /// <summary>Identifies the <see cref="Header"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderProperty = RibbonControl.HeaderProperty.AddOwner(typeof(RibbonToggleButton), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <inheritdoc />
    public DataTemplate? HeaderTemplate
    {
        get => (DataTemplate?)this.GetValue(HeaderTemplateProperty);
        set => this.SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>Identifies the <see cref="HeaderTemplate"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderTemplateProperty = RibbonControl.HeaderTemplateProperty.AddOwner(typeof(RibbonToggleButton), new PropertyMetadata());

    /// <inheritdoc />
    public DataTemplateSelector? HeaderTemplateSelector
    {
        get => (DataTemplateSelector?)this.GetValue(HeaderTemplateSelectorProperty);
        set => this.SetValue(HeaderTemplateSelectorProperty, value);
    }

    /// <summary>Identifies the <see cref="HeaderTemplateSelector"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderTemplateSelectorProperty = RibbonControl.HeaderTemplateSelectorProperty.AddOwner(typeof(RibbonToggleButton), new PropertyMetadata());

    /// <inheritdoc />
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Localizable(false)]
    public object? Icon
    {
        get => this.GetValue(IconProperty);
        set => this.SetValue(IconProperty, value);
    }

    /// <summary>Identifies the <see cref="Icon"/> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = RibbonControl.IconProperty.AddOwner(typeof(RibbonToggleButton), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <inheritdoc />
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Localizable(false)]
    public object? LargeIcon
    {
        get => this.GetValue(LargeIconProperty);
        set => this.SetValue(LargeIconProperty, value);
    }

    /// <summary>Identifies the <see cref="LargeIcon"/> dependency property.</summary>
    public static readonly DependencyProperty LargeIconProperty = LargeIconProviderProperties.LargeIconProperty.AddOwner(typeof(RibbonToggleButton), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <inheritdoc />
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Localizable(false)]
    public object? MediumIcon
    {
        get => this.GetValue(MediumIconProperty);
        set => this.SetValue(MediumIconProperty, value);
    }

    /// <summary>Identifies the <see cref="MediumIcon"/> dependency property.</summary>
    public static readonly DependencyProperty MediumIconProperty = MediumIconProviderProperties.MediumIconProperty.AddOwner(typeof(RibbonToggleButton), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

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
        DependencyProperty.Register(nameof(IsDefinitive), typeof(bool), typeof(RibbonToggleButton), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets a value indicating whether gets or sets whether or not the ribbon is in Simplified mode
    /// </summary>
    public bool IsSimplified
    {
        get => (bool)this.GetValue(IsSimplifiedProperty);
        private set => this.SetValue(IsSimplifiedPropertyKey, BooleanBoxes.Box(value));
    }

    private static readonly DependencyPropertyKey IsSimplifiedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsSimplified), typeof(bool), typeof(RibbonToggleButton), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>Identifies the <see cref="IsSimplified"/> dependency property.</summary>
    public static readonly DependencyProperty IsSimplifiedProperty = IsSimplifiedPropertyKey.DependencyProperty;

    /// <summary>
    /// Initializes static members of the <see cref="RibbonToggleButton"/> class.
    /// </summary>
    static RibbonToggleButton()
    {
        Type type = typeof(RibbonToggleButton);
        DefaultStyleKeyProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(type));

        // ContextMenuService.Attach(type);
        // ToolTipService.Attach(type);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonToggleButton"/> class.
    /// </summary>
    public RibbonToggleButton()
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

        // fix for #481
        // We can't overwrite OnToggle because it's "internal protected"...
        if (string.IsNullOrEmpty(this.GroupName) == false)
        {
            // Only forward click if button is not checked to prevent wrong bound values
            if (this.IsChecked == false)
            {
                base.OnClick();
            }
            else
            {
                // Fix for #1196
                var newEvent = new RoutedEventArgs(ClickEvent, this);
                this.RaiseEvent(newEvent);

                this.ExecuteCommand();
            }
        }
        else
        {
            base.OnClick();
        }
    }

    /// <inheritdoc />
    protected override void OnChecked(RoutedEventArgs e)
    {
        ToggleButtonHelper.UpdateButtonGroup(this);
        base.OnChecked(e);
    }

    /// <summary>
    /// Used to call OnClick (which is protected)
    /// </summary>
    public void InvokeClick()
    {
        this.OnClick();
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
        }
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer() => new Wpf.Ui.Controls.Automation.Peers.RibbonToggleButtonAutomationPeer(this);
}
