// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Internal;

/// <summary>
/// Scope guard to prevent reentrancy.
/// </summary>
#pragma warning disable CA1063 // Implement IDisposable Correctly
public class ScopeGuard : IDisposable
#pragma warning restore CA1063 // Implement IDisposable Correctly
{
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Action? onEntry;
    private readonly Action? onDispose;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScopeGuard"/> class.
    /// Creates a new instance.
    /// </summary>
    public ScopeGuard()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScopeGuard"/> class.
    /// Creates a new instance.
    /// </summary>
    /// <param name="onEntry">Action being called on entry.</param>
    /// <param name="onDispose">Action being called on dispose.</param>
    public ScopeGuard( Action onEntry, Action onDispose )
    {
        this.onEntry = onEntry ?? throw new ArgumentNullException(nameof(onEntry));
        this.onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
    }

    /// <summary>
    /// Gets a value indicating whether gets whether this instance is still active (not disposed) or not.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Starts the scope guard.
    /// </summary>
    /// <returns>The current instance for fluent usage.</returns>
    public ScopeGuard Start()
    {
        if (this.IsActive)
        {
            return this;
        }

        this.IsActive = true;
        this.onEntry?.Invoke();

        return this;
    }

    /// <inheritdoc />
#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
#pragma warning restore CA1063 // Implement IDisposable Correctly
    {
        var wasActive = this.IsActive;
        this.IsActive = false;

        if (wasActive)
        {
            this.onDispose?.Invoke();
        }
    }
}
