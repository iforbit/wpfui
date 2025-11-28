// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Demo.Writer.ViewModels;

namespace Wpf.Ui.Demo.Writer.Views.Pages;

public partial class WriteBoardPage : INavigableView<ViewModels.WriteBoardViewModel>
{
    public ViewModels.WriteBoardViewModel ViewModel { get; }

    public WriteBoardPage(WriteBoardViewModel viewModel)
    {
        ViewModel = viewModel;

        DataContext = viewModel; // Bind directly to ViewModel
        InitializeComponent();

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {

    }
}