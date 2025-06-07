// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Input;

namespace Wpf.Ui.Controls;

public class BindableNavigationViewItem : NavigationViewItem, ICommandSource
{
    public static new readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(BindableNavigationViewItem));

    public static new readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(BindableNavigationViewItem));

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(object), typeof(BindableNavigationViewItem));

    public static readonly DependencyProperty CommandIconProperty =
    DependencyProperty.Register(
        nameof(CommandIcon),
        typeof(object),
        typeof(BindableNavigationViewItem),
        new PropertyMetadata(null)
    );

    public new ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public new object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public object? CommandIcon
    {
        get => GetValue(CommandIconProperty);
        set => SetValue(CommandIconProperty, value);
    }

    public object? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    protected override void OnClick()
    {
        base.OnClick();

        if (Command != null)
        {
            var param = CommandParameter ?? this; // 기본값은 자기 자신

            if (Command.CanExecute(param))
            {
                Command.Execute(param);
            }
        }
    }
}