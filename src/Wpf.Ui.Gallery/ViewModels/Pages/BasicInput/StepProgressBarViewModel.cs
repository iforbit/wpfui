// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Gallery.ViewModels.Pages.BasicInput;

public partial class StepProgressBarViewModel : ViewModel
{
    // ── Scenario 1: Horizontal wizard ──────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    private int _wizardStep = 0;

    public bool CanGoBack => WizardStep > 0;
    public bool CanGoNext => WizardStep < 2;

    [RelayCommand]
    private void GoNext()
    {
        if (WizardStep < 2)
            WizardStep++;
    }

    [RelayCommand]
    private void GoBack()
    {
        if (WizardStep > 0)
            WizardStep--;
    }

    // ── Scenario 2: Vertical with icons ───────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoBackVertical))]
    [NotifyPropertyChangedFor(nameof(CanGoNextVertical))]
    private int _installStep = 0;

    public bool CanGoBackVertical => InstallStep > 0;
    public bool CanGoNextVertical => InstallStep < 3;

    [RelayCommand]
    private void GoNextVertical()
    {
        if (InstallStep < 3)
            InstallStep++;
    }

    [RelayCommand]
    private void GoBackVertical()
    {
        if (InstallStep > 0)
            InstallStep--;
    }

    // ── Scenario 3: CanUserSelect ──────────────────────────────────────
    [ObservableProperty]
    private int _interactiveStep = 1;
}
