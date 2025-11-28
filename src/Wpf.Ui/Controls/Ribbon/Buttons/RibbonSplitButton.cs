// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections;
using System.Diagnostics;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents button control that allows
/// you to add menu and handle clicks
/// </summary>
[TemplatePart(Name = "PART_Button", Type = typeof(ButtonBase))]
[DebuggerDisplay("{GetType().FullName}: Header = {Header}, Items.Count = {Items.Count}, Size = {Size}, IsSimplified = {IsSimplified}")]
public class RibbonSplitButton : RibbonDropDownButton, IToggleButton, ICommandSource
{
#pragma warning disable IDE0032
    // Inner button
    private RibbonToggleButton? button;
#pragma warning restore IDE0032

    // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
    internal RibbonToggleButton? Button => this.button;

    /// <inheritdoc />
    [Category("Action")]
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Bindable(true)]
    public ICommand Command
    {
        get => (ICommand)this.GetValue(CommandProperty);

        set => this.SetValue(CommandProperty, value);
    }

    /// <inheritdoc />
    [Bindable(true)]
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Category("Action")]
    public object CommandParameter
    {
        get => this.GetValue(CommandParameterProperty);

        set => this.SetValue(CommandParameterProperty, value);
    }

    /// <inheritdoc />
    [Bindable(true)]
    [Category("Action")]
    public IInputElement CommandTarget
    {
        get => (IInputElement)this.GetValue(CommandTargetProperty);

        set => this.SetValue(CommandTargetProperty, value);
    }

    /// <summary>Identifies the <see cref="CommandParameter"/> dependency property.</summary>
    public static readonly DependencyProperty CommandParameterProperty = ButtonBase.CommandParameterProperty.AddOwner(typeof(RibbonSplitButton), new FrameworkPropertyMetadata());

    /// <summary>Identifies the <see cref="Command"/> dependency property.</summary>
    public static readonly DependencyProperty CommandProperty = ButtonBase.CommandProperty.AddOwner(typeof(RibbonSplitButton), new FrameworkPropertyMetadata());

    /// <summary>Identifies the <see cref="CommandTarget"/> dependency property.</summary>
    public static readonly DependencyProperty CommandTargetProperty = ButtonBase.CommandTargetProperty.AddOwner(typeof(RibbonSplitButton), new FrameworkPropertyMetadata());

    /// <inheritdoc />
    public string? GroupName
    {
        get => (string?)this.GetValue(GroupNameProperty);
        set => this.SetValue(GroupNameProperty, value);
    }

    /// <summary>Identifies the <see cref="GroupName"/> dependency property.</summary>
    public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register(nameof(GroupName), typeof(string), typeof(RibbonSplitButton));

    /// <inheritdoc />
    public bool? IsChecked
    {
        get => (bool?)this.GetValue(IsCheckedProperty);
        set => this.SetValue(IsCheckedProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsChecked"/> dependency property.</summary>
    public static readonly DependencyProperty IsCheckedProperty = System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty.AddOwner(typeof(RibbonSplitButton), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnIsCheckedChanged, CoerceIsChecked));

    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var button = (RibbonSplitButton)d;
        if (button.IsCheckable)
        {
            var nullable = (bool?)e.NewValue;
            if (nullable is null)
            {
                button.RaiseEvent(new RoutedEventArgs(IndeterminateEvent, button));
            }
            else if (nullable.Value)
            {
                button.RaiseEvent(new RoutedEventArgs(CheckedEvent, button));
            }
            else
            {
                button.RaiseEvent(new RoutedEventArgs(UncheckedEvent, button));
            }
        }
    }

    private static object? CoerceIsChecked(DependencyObject d, object? basevalue)
    {
        var button = (RibbonSplitButton)d;

        if (button.IsCheckable == false)
        {
            return BooleanBoxes.FalseBox;
        }

        return basevalue;
    }

    /// <summary>
    /// Gets or sets a value indicating whether SplitButton can be checked
    /// </summary>
    public bool IsCheckable
    {
        get => (bool)this.GetValue(IsCheckableProperty);
        set => this.SetValue(IsCheckableProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsCheckable"/> dependency property.</summary>
    public static readonly DependencyProperty IsCheckableProperty =
        DependencyProperty.Register(nameof(IsCheckable), typeof(bool), typeof(RibbonSplitButton), new PropertyMetadata(BooleanBoxes.FalseBox));

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
        typeof(RibbonSplitButton),
        new PropertyMetadata(ControlAppearance.Primary));

    /// <summary>
    /// Gets or sets tooltip of dropdown part of split button
    /// </summary>
    public object? DropDownToolTip
    {
        get => this.GetValue(DropDownToolTipProperty);
        set => this.SetValue(DropDownToolTipProperty, value);
    }

    /// <summary>Identifies the <see cref="DropDownToolTip"/> dependency property.</summary>
    public static readonly DependencyProperty DropDownToolTipProperty =
        DependencyProperty.Register(nameof(DropDownToolTip), typeof(object), typeof(RibbonSplitButton), new PropertyMetadata());

    /// <summary>
    /// Gets or sets a value indicating whether the button part of split button is enabled.
    /// If you want to disable the button part and the DropDown please use <see cref="UIElement.IsEnabled"/>.
    /// </summary>
    public bool IsButtonEnabled
    {
        get => (bool)this.GetValue(IsButtonEnabledProperty);
        set => this.SetValue(IsButtonEnabledProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsButtonEnabled"/> dependency property.</summary>
    public static readonly DependencyProperty IsButtonEnabledProperty =
        DependencyProperty.Register(nameof(IsButtonEnabled), typeof(bool), typeof(RibbonSplitButton), new PropertyMetadata(BooleanBoxes.TrueBox));

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
        DependencyProperty.Register(nameof(IsDefinitive), typeof(bool), typeof(RibbonSplitButton), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Occurs when user clicks
    /// </summary>
    public static readonly RoutedEvent ClickEvent = ButtonBase.ClickEvent.AddOwner(typeof(RibbonSplitButton));

    /// <summary>
    /// Occurs when user clicks
    /// </summary>
    public event RoutedEventHandler Click
    {
        add => this.AddHandler(ClickEvent, value);

        remove => this.RemoveHandler(ClickEvent, value);
    }

    /// <summary>
    /// Occurs when button is checked
    /// </summary>
    public static readonly RoutedEvent CheckedEvent = System.Windows.Controls.Primitives.ToggleButton.CheckedEvent.AddOwner(typeof(RibbonSplitButton));

    /// <summary>
    /// Occurs when button is checked
    /// </summary>
    public event RoutedEventHandler Checked
    {
        add => this.AddHandler(CheckedEvent, value);

        remove => this.RemoveHandler(CheckedEvent, value);
    }

    /// <summary>
    /// Occurs when button is unchecked
    /// </summary>
    public static readonly RoutedEvent UncheckedEvent = System.Windows.Controls.Primitives.ToggleButton.UncheckedEvent.AddOwner(typeof(RibbonSplitButton));

    /// <summary>
    /// Occurs when button is unchecked
    /// </summary>
    public event RoutedEventHandler Unchecked
    {
        add => this.AddHandler(UncheckedEvent, value);

        remove => this.RemoveHandler(UncheckedEvent, value);
    }

    /// <summary>
    /// Occurs when button is unchecked
    /// </summary>
    public static readonly RoutedEvent IndeterminateEvent = System.Windows.Controls.Primitives.ToggleButton.IndeterminateEvent.AddOwner(typeof(RibbonSplitButton));

    /// <summary>
    /// Occurs when button is unchecked
    /// </summary>
    public event RoutedEventHandler Indeterminate
    {
        add => this.AddHandler(IndeterminateEvent, value);

        remove => this.RemoveHandler(IndeterminateEvent, value);
    }

    static RibbonSplitButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonSplitButton), new FrameworkPropertyMetadata(typeof(RibbonSplitButton)));
        FocusVisualStyleProperty.OverrideMetadata(typeof(RibbonSplitButton), new FrameworkPropertyMetadata());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonSplitButton"/> class.
    /// Default constructor
    /// </summary>
    public RibbonSplitButton()
    {
        // ContextMenuService.Coerce(this);
        this.Click += this.OnClick;
        this.Loaded += this.OnLoaded;
        this.Unloaded += this.OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        this.SubscribeEvents();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        this.UnSubscribeEvents();
    }

    private void SubscribeEvents()
    {
        // Always unsubscribe events to ensure we don't subscribe twice
        this.UnSubscribeEvents();

        if (this.button is not null)
        {
            this.button.Click += this.OnButtonClick;
        }
    }

    private void UnSubscribeEvents()
    {
        if (this.button is not null)
        {
            this.button.Click -= this.OnButtonClick;
        }
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        // if (ReferenceEquals(e.OriginalSource, this) == false
        //    && ReferenceEquals(e.OriginalSource, this.quickAccessButton) == false)
        if (ReferenceEquals(e.OriginalSource, this) == false)
        {
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        this.UnSubscribeEvents();

        this.button = this.GetTemplateChild("PART_Button") as RibbonToggleButton;
        if (this.button is ISimplifiedStateControl control)
        {
            control.UpdateSimplifiedState(this.IsSimplified);
        }

        base.OnApplyTemplate();

        this.SubscribeEvents();
    }

    /// <inheritdoc />
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (!PopupService.IsMousePhysicallyOver(this.button))
        {
            base.OnPreviewMouseLeftButtonDown(e);
        }
        else
        {
            this.SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer() => new RibbonSplitButtonAutomationPeer(this);

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Enter)
        {
            this.button?.InvokeClick();
        }
    }

    internal void AutomationButtonClick()
    {
        this.button?.InvokeClick();
    }

    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        this.RaiseEvent(new RoutedEventArgs(ClickEvent, this));

        // Execute Command if it exists and can execute
        if (this.Command is not null && this.Command.CanExecute(this.CommandParameter))
        {
            this.Command.Execute(this.CommandParameter);
        }
    }

    /// <inheritdoc />
    protected override void OnIsSimplifiedChanged(bool oldValue, bool newValue)
    {
        base.OnIsSimplifiedChanged(oldValue, newValue);
        if (this.button is ISimplifiedStateControl control)
        {
            control.UpdateSimplifiedState(newValue);
        }
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

            if (this.button is not null)
            {
                yield return this.button;
            }
        }
    }
}