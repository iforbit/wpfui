// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Wpf.Ui.Controls;

/// <summary>
/// Synchronizes a target collection with a source collection in a one way fashion.
/// </summary>
/// /// <typeparam name="TItem">
/// The type of items contained in the source collection. This type represents the element type of the <see cref="Source"/>.
/// </typeparam>
public class CollectionSyncHelper<TItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionSyncHelper{TItem}"/> class.
    /// Creates a new instance with <paramref name="source"/> as <see cref="Source"/> and <paramref name="target"/> as <see cref="Target"/>.
    /// </summary>
    public CollectionSyncHelper( ObservableCollection<TItem> source, IList target )
    {
        this.Source = source ?? throw new ArgumentNullException(nameof(source));
        this.Target = target ?? throw new ArgumentNullException(nameof(target));

        this.SyncTarget();

        this.Source.CollectionChanged += this.SourceOnCollectionChanged;
    }

    /// <summary>
    /// Gets the source collection.
    /// </summary>
    public ObservableCollection<TItem> Source { get; }

    /// <summary>
    /// Gets the target collection.
    /// </summary>
    public IList Target { get; }

    /// <summary>
    /// Clears <see cref="Target"/> and then copies all items from <see cref="Source"/> to <see cref="Target"/>.
    /// </summary>
    private void SyncTarget()
    {
        this.Target.Clear();

        foreach (TItem? item in this.Source)
        {
            _ = this.Target.Add(item);
        }
    }

    private void SourceOnCollectionChanged( object? sender, NotifyCollectionChangedEventArgs e )
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                for (var i = 0; i < e.NewItems?.Count; i++)
                {
                    this.Target.Insert(e.NewStartingIndex + i, e.NewItems![i]);
                }

                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems is not null)
                {
                    foreach (var item in e.OldItems)
                    {
                        this.Target.Remove(item);
                    }
                }

                break;

            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems is not null)
                {
                    foreach (var item in e.OldItems)
                    {
                        this.Target.Remove(item);
                    }
                }

                if (e.NewItems is not null)
                {
                    foreach (var item in e.NewItems)
                    {
                        _ = this.Target.Add(item);
                    }
                }

                break;

            case NotifyCollectionChangedAction.Reset:
                this.SyncTarget();

                break;
        }
    }
}