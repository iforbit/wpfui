// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Media;

namespace Wpf.Ui.Gallery.ViewModels.Pages.Media;

/// <summary>
/// Brush 프리셋 (ComboBox 표시용)
/// </summary>
public record BrushPreset(string Name, Brush Brush);
