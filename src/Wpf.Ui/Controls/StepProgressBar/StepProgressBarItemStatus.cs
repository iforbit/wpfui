// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

// ReSharper disable once CheckNamespace
namespace Wpf.Ui.Controls;

/// <summary>
/// Defines the progress status of a <see cref="StepProgressBarItem"/>.
/// </summary>
public enum StepProgressBarItemStatus
{
    /// <summary>The step has not been started yet.</summary>
    NotStarted,

    /// <summary>The step is currently active.</summary>
    Active,

    /// <summary>The step has been completed.</summary>
    Completed,
}
