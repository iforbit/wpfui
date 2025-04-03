// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;

using Wpf.Ui.Controls.Helpers;
using Wpf.Ui.Extensions;
using Wpf.Ui.Internal;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represent base class for Fluent controls
/// </summary>
public abstract class RibbonControl : System.Windows.Controls.Control, ICommandSource, IRibbonControl
{
    /*
    /// <inheritdoc />
    public string? KeyTip
    {
        get => (string?)this.GetValue(KeyTipProperty);
        set => this.SetValue(KeyTipProperty, value);
    }

    /// <summary>
    /// Using a DependencyProperty as the backing store for Keys.
    /// This enables animation, styling, binding, etc...
    /// </summary>
    public static readonly DependencyProperty KeyTipProperty = Fluent.KeyTip.KeysProperty.AddOwner(typeof(RibbonControl));
    */

    /// <inheritdoc />
    public object? Header
    {
        get => this.GetValue(HeaderProperty);
        set => this.SetValue(HeaderProperty, value);
    }

    /// <summary>Identifies the <see cref="Header"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(RibbonControl), new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <inheritdoc />
    public DataTemplate? HeaderTemplate
    {
        get => (DataTemplate?)this.GetValue(HeaderTemplateProperty);
        set => this.SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>Identifies the <see cref="HeaderTemplate"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(RibbonControl), new PropertyMetadata());

    /// <inheritdoc />
    public DataTemplateSelector? HeaderTemplateSelector
    {
        get => (DataTemplateSelector?)this.GetValue(HeaderTemplateSelectorProperty);
        set => this.SetValue(HeaderTemplateSelectorProperty, value);
    }

    /// <summary>Identifies the <see cref="HeaderTemplateSelector"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderTemplateSelectorProperty =
        DependencyProperty.Register(nameof(HeaderTemplateSelector), typeof(DataTemplateSelector), typeof(RibbonControl), new PropertyMetadata());

    /// <inheritdoc />
    [Localizability(LocalizationCategory.NeverLocalize)]
    [Localizable(false)]
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Identifies the <see cref="Icon"/> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(object), typeof(RibbonControl), new FrameworkPropertyMetadata(
            default,
            FrameworkPropertyMetadataOptions.None,
            LogicalChildSupportHelper.OnLogicalChildPropertyChanged,
            IconElement.Coerce));

    private bool currentCanExecute = true;

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
    public static readonly DependencyProperty CommandParameterProperty = System.Windows.Controls.Primitives.ButtonBase.CommandParameterProperty.AddOwner(typeof(RibbonControl), new PropertyMetadata());

    /// <summary>Identifies the <see cref="Command"/> dependency property.</summary>
    public static readonly DependencyProperty CommandProperty = System.Windows.Controls.Primitives.ButtonBase.CommandProperty.AddOwner(typeof(RibbonControl), new PropertyMetadata(OnCommandChanged));

    /// <summary>Identifies the <see cref="CommandTarget"/> dependency property.</summary>
    public static readonly DependencyProperty CommandTargetProperty = System.Windows.Controls.Primitives.ButtonBase.CommandTargetProperty.AddOwner(typeof(RibbonControl), new PropertyMetadata());

    /// <summary>
    /// Handles Command changed
    /// </summary>
    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RibbonControl control)
        {
            return;
        }

        if (e.OldValue is ICommand oldCommand)
        {
            oldCommand.CanExecuteChanged -= control.OnCommandCanExecuteChanged;
        }

        if (e.NewValue is ICommand newCommand)
        {
            newCommand.CanExecuteChanged += control.OnCommandCanExecuteChanged;

            if (e.NewValue is RoutedUICommand routedUiCommand
                && control.Header is null)
            {
                control.Header = routedUiCommand.Text;
            }
        }

        control.UpdateCanExecute();
    }

    /// <summary>
    /// Handles Command CanExecute changed
    /// </summary>
    private void OnCommandCanExecuteChanged(object? sender, EventArgs e)
    {
        this.UpdateCanExecute();
    }

    private void UpdateCanExecute()
    {
        var canExecute = this.Command is not null
                         && this.CanExecuteCommand();

        if (this.currentCanExecute != canExecute)
        {
            this.currentCanExecute = canExecute;
            this.CoerceValue(IsEnabledProperty);
        }
    }

    /// <inheritdoc />
    protected override bool IsEnabledCore => base.IsEnabledCore && (this.currentCanExecute || this.Command is null);

    /// <inheritdoc />
    public RibbonControlSize Size
    {
        get => (RibbonControlSize)this.GetValue(SizeProperty);
        set => this.SetValue(SizeProperty, value);
    }

    /// <summary>Identifies the <see cref="Size"/> dependency property.</summary>
    public static readonly DependencyProperty SizeProperty = RibbonProperties.SizeProperty.AddOwner(typeof(RibbonControl));

    /// <inheritdoc />
    public RibbonControlSizeDefinition SizeDefinition
    {
        get => (RibbonControlSizeDefinition)this.GetValue(SizeDefinitionProperty);
        set => this.SetValue(SizeDefinitionProperty, value);
    }

    /// <summary>Identifies the <see cref="SizeDefinition"/> dependency property.</summary>
    public static readonly DependencyProperty SizeDefinitionProperty = RibbonProperties.SizeDefinitionProperty.AddOwner(typeof(RibbonControl));

    /// <summary>
    /// Initializes static members of the <see cref="RibbonControl"/> class.
    /// Static constructor
    /// </summary>
    static RibbonControl()
    {
        _ = typeof(RibbonControl);

        // ContextMenuService.Attach(type);
        // ToolTipService.Attach(type);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonControl"/> class.
    /// Default Constructor
    /// </summary>
    protected RibbonControl()
    {
        // ContextMenuService.Coerce(this);
    }

    internal static void Bind(object source, FrameworkElement target, string path, DependencyProperty property, BindingMode mode)
    {
        Bind(source, target, new PropertyPath(path), property, mode);
    }

    internal static void Bind(object source, FrameworkElement target, string path, DependencyProperty property, BindingMode mode, UpdateSourceTrigger updateSourceTrigger)
    {
        Bind(source, target, new PropertyPath(path), property, mode, updateSourceTrigger);
    }

    internal static void Bind(object source, FrameworkElement target, PropertyPath path, DependencyProperty property, BindingMode mode)
    {
        Bind(source, target, path, property, mode, UpdateSourceTrigger.Default);
    }

    internal static void Bind(object source, FrameworkElement target, PropertyPath path, DependencyProperty property, BindingMode mode, UpdateSourceTrigger updateSourceTrigger)
    {
        var binding = new System.Windows.Data.Binding
        {
            Path = path,
            Source = source,
            Mode = mode,
            UpdateSourceTrigger = updateSourceTrigger
        };
        _ = target.SetBinding(property, binding);
    }

    /*
    /// <inheritdoc />
    public virtual KeyTipPressedResult OnKeyTipPressed()
    {
        return KeyTipPressedResult.Empty;
    }

    /// <inheritdoc />
    public virtual void OnKeyTipBack()
    {
    }
    */

    /// <summary>
    /// Returns screen workarea in witch control is placed
    /// </summary>
    /// <param name="control">Control</param>
    /// <returns>Workarea in witch control is placed</returns>
    public static Rect GetControlWorkArea(FrameworkElement control)
    {
        if (PresentationSource.FromVisual(control) is null)
        {
            return default;
        }

        // 1️ 컨트롤의 화면 위치 가져오기
        Point controlPosOnScreen = control.PointToScreen(new Point(0, 0));
        /*
        var controlRect = new RECT
        {
            left = (int)controlPosOnScreen.X,
            top = (int)controlPosOnScreen.Y,
            right = (int)controlPosOnScreen.X + (int)control.ActualWidth,
            bottom = (int)controlPosOnScreen.Y + (int)control.ActualHeight
        };
        var monitor = PInvoke.MonitorFromRect(&controlRect, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        if (monitor != IntPtr.Zero)
        {
            var monitorInfo = new MONITORINFO { cbSize = (uint)Marshal.SizeOf<MONITORINFO>() };
            PInvoke.GetMonitorInfo(monitor, &monitorInfo);
            return new Rect(monitorInfo.rcWork.left, monitorInfo.rcWork.top, monitorInfo.rcWork.right - monitorInfo.rcWork.left, monitorInfo.rcWork.bottom - monitorInfo.rcWork.top);
        }

        return default;
        */

        // 2️ 해당 위치가 속한 Screen 찾기
        Screen screen = Screen.FromPoint(new System.Drawing.Point((int)controlPosOnScreen.X, (int)controlPosOnScreen.Y));

        // 3️ Work Area (작업 표시줄 제외) 가져오기
        System.Drawing.Rectangle workArea = screen.WorkingArea;

        // 4️ WPF Rect로 변환하여 반환
        return new Rect(workArea.Left, workArea.Top, workArea.Width, workArea.Height);
    }

    /// <summary>
    /// Returns monitor in witch control is placed
    /// </summary>
    /// <param name="control">Control</param>
    /// <returns>Workarea in witch control is placed</returns>
    public static Rect GetControlMonitor(FrameworkElement control)
    {
        if (PresentationSource.FromVisual(control) is null)
        {
            return default;
        }

        // 1️ 컨트롤의 화면 좌표 가져오기
        Point controlPosOnScreen = control.PointToScreen(new Point(0, 0));
        /*
        var controlRect = new RECT
        {
            left = (int)controlPosOnScreen.X,
            top = (int)controlPosOnScreen.Y,
            right = (int)controlPosOnScreen.X + (int)control.ActualWidth,
            bottom = (int)controlPosOnScreen.Y + (int)control.ActualHeight
        };
        var monitor = PInvoke.MonitorFromRect(&controlRect, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        if (monitor != IntPtr.Zero)
        {
            var monitorInfo = new MONITORINFO { cbSize = (uint)Marshal.SizeOf<MONITORINFO>() };
            PInvoke.GetMonitorInfo(monitor, &monitorInfo);
            return new Rect(monitorInfo.rcMonitor.left, monitorInfo.rcMonitor.top, monitorInfo.rcMonitor.right - monitorInfo.rcMonitor.left, monitorInfo.rcMonitor.bottom - monitorInfo.rcMonitor.top);
        }

        return default;
        */

        // 2️ 해당 좌표가 속한 모니터 찾기
        Screen screen = Screen.FromPoint(new System.Drawing.Point((int)controlPosOnScreen.X, (int)controlPosOnScreen.Y));

        // 3️ 모니터 전체 영역(Rect) 변환
        System.Drawing.Rectangle monitorArea = screen.Bounds;
        return new Rect(monitorArea.Left, monitorArea.Top, monitorArea.Width, monitorArea.Height);
    }

    /// <summary>
    /// Get the parent <see cref="Ribbon"/>.
    /// </summary>
    /// <returns>The found <see cref="Ribbon"/> or <c>null</c> of no parent <see cref="Ribbon"/> could be found.</returns>
    public static Ribbon? GetParentRibbon(DependencyObject obj)
    {
        return UIHelper.GetParent<Ribbon>(obj);
    }

    /// <inheritdoc />
    public new void AddLogicalChild(object child)
    {
        this.AddLogicalChild(child);
    }

    /// <inheritdoc />
    public new void RemoveLogicalChild(object child)
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

            if (this.Header is not null)
            {
                yield return this.Header;
            }
        }
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer() => new Wpf.Ui.Controls.Automation.Peers.RibbonControlAutomationPeer(this);
}