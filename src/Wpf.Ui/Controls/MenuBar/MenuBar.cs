// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Lighted Solutions and Contributors.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using Wpf.Ui.Appearance;

namespace Wpf.Ui.Controls;

/// <summary>
/// A Visual Studio-style menu bar that combines a title with a menu, designed for integration in the window title bar area.
/// </summary>
[TemplatePart(Name = "PART_MinimizeButton", Type = typeof(MenuBarButton))]
[TemplatePart(Name = "PART_MaximizeButton", Type = typeof(MenuBarButton))]
[TemplatePart(Name = "PART_CloseButton", Type = typeof(MenuBarButton))]
[TemplatePart(Name = "PART_Menu", Type = typeof(Menu))]

public class MenuBar : Control, IThemeControl
{
    /// <summary>
    /// Identifies the <see cref="Title"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(MenuBar),
        new PropertyMetadata(string.Empty)
    );

    /// <summary>
    /// Identifies the <see cref="Icon"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(IconElement),
        typeof(MenuBar),
        new PropertyMetadata(null, null, IconElement.Coerce)
    );

    /// <summary>
    /// Identifies the <see cref="ShowIcon"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register(
        nameof(ShowIcon),
        typeof(bool),
        typeof(MenuBar),
        new PropertyMetadata(true)
    );

    /// <summary>
    /// Identifies the <see cref="ShowTitle"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ShowTitleProperty = DependencyProperty.Register(
        nameof(ShowTitle),
        typeof(bool),
        typeof(MenuBar),
        new PropertyMetadata(true)
    );

    /// <summary>
    /// Identifies the <see cref="TitleFontSize"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register(
        nameof(TitleFontSize),
        typeof(double),
        typeof(MenuBar),
        new PropertyMetadata(12.0)
    );

    /// <summary>
    /// Identifies the <see cref="TitleForeground"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TitleForegroundProperty = DependencyProperty.Register(
        nameof(TitleForeground),
        typeof(Brush),
        typeof(MenuBar),
        new PropertyMetadata(null)
    );

    /// <summary>
    /// Identifies the <see cref="MenuItems"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MenuItemsProperty = DependencyProperty.Register(
        nameof(MenuItems),
        typeof(ObservableCollection<object>),
        typeof(MenuBar),
        new PropertyMetadata(null)
    );

    /// <summary>
    /// Identifies the <see cref="TrailingContent"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TrailingContentProperty = DependencyProperty.Register(
        nameof(TrailingContent),
        typeof(object),
        typeof(MenuBar),
        new PropertyMetadata(null)
    );

    /// <summary>
    /// Identifies the <see cref="ShowMinimize"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ShowMinimizeProperty = DependencyProperty.Register(
        nameof(ShowMinimize),
        typeof(bool),
        typeof(MenuBar),
        new PropertyMetadata(true)
    );

    /// <summary>
    /// Identifies the <see cref="ShowMaximize"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ShowMaximizeProperty = DependencyProperty.Register(
        nameof(ShowMaximize),
        typeof(bool),
        typeof(MenuBar),
        new PropertyMetadata(true)
    );

    /// <summary>
    /// Identifies the <see cref="ShowClose"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ShowCloseProperty = DependencyProperty.Register(
        nameof(ShowClose),
        typeof(bool),
        typeof(MenuBar),
        new PropertyMetadata(true)
    );

    /// <summary>
    /// Identifies the <see cref="IsMaximized"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsMaximizedProperty = DependencyProperty.Register(
        nameof(IsMaximized),
        typeof(bool),
        typeof(MenuBar),
        new PropertyMetadata(false)
    );

    /// <summary>
    /// Identifies the <see cref="ShowCloseConfirmation"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ShowCloseConfirmationProperty = DependencyProperty.Register(
        nameof(ShowCloseConfirmation),
        typeof(bool),
        typeof(MenuBar),
        new PropertyMetadata(false)
    );

    /// <summary>
    /// Identifies the <see cref="ForceShutdown"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ForceShutdownProperty = DependencyProperty.Register(
        nameof(ForceShutdown),
        typeof(bool),
        typeof(MenuBar),
        new PropertyMetadata(false)
    );

    /// <summary>
    /// Identifies the <see cref="CloseCommand"/> dependency property.
    /// When set, this command is executed instead of default close behavior.
    /// The command can return false to cancel the close operation.
    /// </summary>
    public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register(
        nameof(CloseCommand),
        typeof(ICommand),
        typeof(MenuBar),
        new PropertyMetadata(null)
    );

    /// <summary>
    /// Identifies the <see cref="ApplicationTheme"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ApplicationThemeProperty = DependencyProperty.Register(
        nameof(ApplicationTheme),
        typeof(ApplicationTheme),
        typeof(MenuBar),
        new PropertyMetadata(ApplicationTheme.Unknown, OnApplicationThemeChanged)
    );

    /// <summary>
    /// Identifies the <see cref="Height"/> dependency property.
    /// </summary>
    public static readonly new DependencyProperty HeightProperty = DependencyProperty.Register(
        nameof(Height),
        typeof(double),
        typeof(MenuBar),
        new PropertyMetadata(32.0)
    );

    /// <summary>
    /// Identifies the <see cref="ButtonCommand"/> routed event.
    /// </summary>
    public static readonly RoutedEvent ButtonCommandEvent = EventManager.RegisterRoutedEvent(
        nameof(ButtonCommand),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(MenuBar)
    );

    private MenuBarButton? _minimizeButton;
    private MenuBarButton? _maximizeButton;
    private MenuBarButton? _closeButton;
    private Menu? _menuControl;

    /// <summary>
    /// Gets or sets the title displayed in the menu bar.
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon displayed before the title.
    /// </summary>
    public IconElement? Icon
    {
        get => (IconElement?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether to show the icon.
    /// </summary>
    public bool ShowIcon
    {
        get => (bool)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether to show the title.
    /// </summary>
    public bool ShowTitle
    {
        get => (bool)GetValue(ShowTitleProperty);
        set => SetValue(ShowTitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the font size of the title.
    /// </summary>
    public double TitleFontSize
    {
        get => (double)GetValue(TitleFontSizeProperty);
        set => SetValue(TitleFontSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the foreground brush of the title.
    /// </summary>
    public Brush? TitleForeground
    {
        get => (Brush?)GetValue(TitleForegroundProperty);
        set => SetValue(TitleForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the collection of menu items.
    /// </summary>
    public ObservableCollection<object>? MenuItems
    {
        get => (ObservableCollection<object>?)GetValue(MenuItemsProperty);
        set => SetValue(MenuItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets additional content displayed after the menu items.
    /// </summary>
    public object? TrailingContent
    {
        get => GetValue(TrailingContentProperty);
        set => SetValue(TrailingContentProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether to show the minimize button.
    /// </summary>
    public bool ShowMinimize
    {
        get => (bool)GetValue(ShowMinimizeProperty);
        set => SetValue(ShowMinimizeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether to show the maximize button.
    /// </summary>
    public bool ShowMaximize
    {
        get => (bool)GetValue(ShowMaximizeProperty);
        set => SetValue(ShowMaximizeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether to show the close button.
    /// </summary>
    public bool ShowClose
    {
        get => (bool)GetValue(ShowCloseProperty);
        set => SetValue(ShowCloseProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the window is maximized.
    /// </summary>
    public bool IsMaximized
    {
        get => (bool)GetValue(IsMaximizedProperty);
        set => SetValue(IsMaximizedProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show a close confirmation dialog.
    /// </summary>
    public bool ShowCloseConfirmation
    {
        get => (bool)GetValue(ShowCloseConfirmationProperty);
        set => SetValue(ShowCloseConfirmationProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the controls affect main application window.
    /// </summary>
    public bool ForceShutdown
    {
        get => (bool)GetValue(ForceShutdownProperty);
        set => SetValue(ForceShutdownProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the close button is clicked.
    /// If set, this command handles the close logic instead of default behavior.
    /// </summary>
    public ICommand? CloseCommand
    {
        get => (ICommand?)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the current application theme.
    /// </summary>
    public ApplicationTheme ApplicationTheme
    {
        get => (ApplicationTheme)GetValue(ApplicationThemeProperty);
        set => SetValue(ApplicationThemeProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the menu bar.
    /// </summary>
    public new double Height
    {
        get => (double)GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    /// <summary>
    /// Occurs when a window control button is clicked.
    /// </summary>
    public event RoutedEventHandler ButtonCommand
    {
        add => AddHandler(ButtonCommandEvent, value);
        remove => RemoveHandler(ButtonCommandEvent, value);
    }

    static MenuBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MenuBar),
            new FrameworkPropertyMetadata(typeof(MenuBar))
        );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuBar"/> class.
    /// </summary>
    public MenuBar()
    {
        MenuItems = new ObservableCollection<object>();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _minimizeButton = GetTemplateChild("PART_MinimizeButton") as MenuBarButton;
        _maximizeButton = GetTemplateChild("PART_MaximizeButton") as MenuBarButton;
        _closeButton = GetTemplateChild("PART_CloseButton") as MenuBarButton;
        _menuControl = GetTemplateChild("PART_Menu") as Menu;

        if (_minimizeButton != null)
        {
            _minimizeButton.Click += OnMinimizeClick;
        }

        if (_maximizeButton != null)
        {
            _maximizeButton.Click += OnMaximizeClick;
        }

        if (_closeButton != null)
        {
            _closeButton.Click += OnCloseClick;
        }

        UpdateTheme();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is Window parentWindow)
        {
            parentWindow.StateChanged += OnWindowStateChanged;
            UpdateWindowState(parentWindow.WindowState);

            // Setup WindowChrome for title bar drag functionality
            SetupWindowChrome(parentWindow);
        }

        ApplicationThemeManager.Changed += OnThemeChanged;
    }

    private void SetupWindowChrome(Window window)
    {
        var existingChrome = WindowChrome.GetWindowChrome(window);

        if (existingChrome == null)
        {
            var chrome = new WindowChrome
            {
                CaptionHeight = Height,
                CornerRadius = new CornerRadius(0),
                GlassFrameThickness = new Thickness(-1),
                ResizeBorderThickness = new Thickness(4),
                UseAeroCaptionButtons = false
            };

            WindowChrome.SetWindowChrome(window, chrome);
        }
        else
        {
            // Update existing chrome's caption height
            existingChrome.CaptionHeight = Height;
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is Window parentWindow)
        {
            parentWindow.StateChanged -= OnWindowStateChanged;
        }

        ApplicationThemeManager.Changed -= OnThemeChanged;
    }

    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        if (sender is Window window)
        {
            UpdateWindowState(window.WindowState);
        }
    }

    private void UpdateWindowState(WindowState state)
    {
        SetCurrentValue(IsMaximizedProperty, state == WindowState.Maximized);
    }

    private void OnMinimizeClick(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is Window window)
        {
            window.WindowState = WindowState.Minimized;
        }

        RaiseEvent(new MenuBarButtonCommandEventArgs(ButtonCommandEvent, this, TitleBarButtonType.Minimize));
    }

    private void OnMaximizeClick(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is Window window)
        {
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        RaiseEvent(new MenuBarButtonCommandEventArgs(ButtonCommandEvent, this, TitleBarButtonType.Maximize));
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        // CloseCommand가 있으면 커스텀 핸들러에 위임 (저장 확인 다이얼로그 등)
        if (CloseCommand != null && CloseCommand.CanExecute(null))
        {
            CloseCommand.Execute(null);
            return; // Command가 종료 처리를 담당
        }

        // 기본 동작: ShowCloseConfirmation 다이얼로그
        if (ShowCloseConfirmation)
        {
            var result = new MessageBox
            {
                Content = "Do you really want to close the window?",
                Title = "Close Confirmation"
            };

            if (!result.ShowDialog(MessageBoxButton.YesNo).Equals(MessageBoxResult.Yes))
            {
                return;
            }
        }

        RaiseEvent(new MenuBarButtonCommandEventArgs(ButtonCommandEvent, this, TitleBarButtonType.Close));

        if (ForceShutdown)
        {
            System.Windows.Application.Current.Shutdown();
            return;
        }

        if (Window.GetWindow(this) is Window window)
        {
            window.Close();
        }
    }

    private void OnThemeChanged(ApplicationTheme currentTheme, Color accent)
    {
        SetCurrentValue(ApplicationThemeProperty, currentTheme);
    }

    private static void OnApplicationThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuBar menuBar)
        {
            menuBar.UpdateTheme();
        }
    }

    private void UpdateTheme()
    {
        // Theme update logic if needed
    }
}
