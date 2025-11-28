// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

/// <summary>
/// Interface for handling loading and saving the state of a <see cref="Ribbon"/>.
/// </summary>
public interface IRibbonStateStorage : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether gets whether state is currently loading.
    /// </summary>
    bool IsLoading { get; }

    /// <summary>
    /// Gets a value indicating whether gets or sets whether state is loaded.
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Save current state to a temporary storage.
    /// </summary>
    void SaveTemporary();

    /// <summary>
    /// Save current state to a persistent storage.
    /// </summary>
    void Save();

    /// <summary>
    /// Load state from a temporary storage.
    /// </summary>
    void LoadTemporary();

    /// <summary>
    /// Loads the state from a persistent storage.
    /// </summary>
    /// <remarks>
    /// Sets <see cref="RibbonStateStorage.IsLoaded" /> after it's finished to prevent a race condition with saving the state to the temporary storage.
    /// </remarks>
    void Load();

    /// <summary>
    /// Resets saved state.
    /// </summary>
    void Reset();
}