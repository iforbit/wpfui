// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Demo.Mvvm.ViewModels;

public partial class DashboardViewModel : ViewModel
{
    private readonly bool _isInitialized = false;

    [ObservableProperty]
    private object? _counter = 0.00001458;

    [ObservableProperty]
    private string? _message = string.Empty;

    [RelayCommand]
    private void messageTest()
    {
        var uiMessageBox = new Controls.MessageBox
        {
            Title = "tttt",
            Content = "TTTT",
        };

        _ = uiMessageBox.ShowDialog(Controls.MessageBoxButton.YesNo);
    }
    public override void OnNavigatedTo()
    {
        if (!_isInitialized)
        {
            InitializeViewModel();
        }
    }
    private void InitializeViewModel()
    {
    }
}