// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.
//
// Based on Fluent.Ribbon: https://github.com/fluentribbon/Fluent.Ribbon

using System.Windows.Input;

namespace Wpf.Ui.Controls;

/// <summary>
/// Class for several commands belonging to the Ribbon.
/// </summary>
public static class RibbonCommands
{
    /// <summary>
    /// Gets the value that represents the Open Backstage command.
    /// </summary>
    public static readonly RoutedCommand OpenBackstage = new RoutedUICommand("Open backstage", nameof(OpenBackstage), typeof(RibbonCommands));
}
