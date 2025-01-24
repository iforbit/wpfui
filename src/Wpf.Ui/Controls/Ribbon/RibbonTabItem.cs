// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Wpf.Ui.Extensions;
using Wpf.Ui.Internal;
using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls.Ribbon;

public class RibbonTabItem : HeaderedContentControl
{
    // Content container
    private Border? contentContainer;

    // Collection of ribbon groups
    private ObservableCollection<RibbonGroupBox>? groups;

    /// <summary>
    /// get collection of ribbon groups
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<RibbonGroupBox> Groups
    {
        get
        {
            if (this.groups is null)
            {
                this.groups = new ObservableCollection<RibbonGroupBox>();
                this.groups.CollectionChanged += this.OnGroupsCollectionChanged;
            }

            return this.groups;
        }
    }

    // handles ribbon groups collection changes
    private void OnGroupsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                break;
            case NotifyCollectionChangedAction.Remove:
                break;
            case NotifyCollectionChangedAction.Replace:
                break;
            case NotifyCollectionChangedAction.Reset:
                break;
        }
    }

    /// <summary>
    /// Gets or sets whether or not the ribbon is in Simplified mode
    /// </summary>
    public bool IsSimplified
    {
        get => (bool)this.GetValue(IsSimplifiedProperty);
        private set => this.SetValue(IsSimplifiedPropertyKey, BooleanBoxes.Box(value));
    }

    private static readonly DependencyPropertyKey IsSimplifiedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsSimplified), typeof(bool), typeof(RibbonTabItem), new PropertyMetadata(BooleanBoxes.FalseBox, OnIsSimplifiedChanged));

    /// <summary>Identifies the <see cref="IsSimplified"/> dependency property.</summary>
    public static readonly DependencyProperty IsSimplifiedProperty = IsSimplifiedPropertyKey.DependencyProperty;

    private static void OnIsSimplifiedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RibbonTabItem ribbonTabItem)
        {
            var isSimplified = (bool)e.NewValue;
            foreach (var item in ribbonTabItem.Groups.OfType<ISimplifiedStateControl>())
            {
                item.UpdateSimplifiedState(isSimplified);
            }
        }
    }

    /// <summary>
    /// Gets or sets whether tab item is selected
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public bool IsSelected
    {
        get => (bool)this.GetValue(IsSelectedProperty);

        set => this.SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Using a DependencyProperty as the backing store for IsSelected.
    /// This enables animation, styling, binding, etc...
    /// </summary>
    public static readonly DependencyProperty IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(typeof(RibbonTabItem), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSelectedChanged));

    /// <summary>
    /// Gets ribbon tab control parent
    /// </summary>
    internal RibbonTabControl? TabControlParent => UIHelper.GetParent<RibbonTabControl>(this);

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var container = (RibbonTabItem)d;
        var newValue = (bool)e.NewValue;

        if (newValue)
        {
            container.OnSelected(new RoutedEventArgs(Selector.SelectedEvent, container));
            container.BringIntoView();
        }
        else
        {
            container.OnUnselected(new RoutedEventArgs(Selector.UnselectedEvent, container));
        }

        // Raise UI automation events on this RibbonTabItem
        if (AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected)
            || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection))
        {
        }
    }

    /// <summary>
    /// Handles selected
    /// </summary>
    /// <param name="e">The event data</param>
    protected virtual void OnSelected(RoutedEventArgs e)
    {
        this.HandleIsSelectedChanged(e);
    }

    /// <summary>
    /// handles unselected
    /// </summary>
    /// <param name="e">The event data</param>
    protected virtual void OnUnselected(RoutedEventArgs e)
    {
        this.HandleIsSelectedChanged(e);
    }

    // Handles IsSelected property changes
    private void HandleIsSelectedChanged(RoutedEventArgs e)
    {
        this.RaiseEvent(e);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        this.SubscribeEvents();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        this.UnSubscribeEvents();
    }

    private void SubscribeEvents()
    {
        // Always unsubscribe events to ensure we don't subscribe twice
        this.UnSubscribeEvents();

        if (this.groups is not null)
        {
            this.groups.CollectionChanged += this.OnGroupsCollectionChanged;
        }
    }

    private void UnSubscribeEvents()
    {
        if (this.groups is not null)
        {
            this.groups.CollectionChanged -= this.OnGroupsCollectionChanged;
        }
    }
    /// <summary>
    /// Gets or sets the <see cref="Brush"/> which is used to render the background if this <see cref="RibbonTabItem"/> is the currently active/selected one.
    /// </summary>
    public Brush? ActiveTabBackground
    {
        get => (Brush?)this.GetValue(ActiveTabBackgroundProperty);
        set => this.SetValue(ActiveTabBackgroundProperty, value);
    }

    /// <summary>Identifies the <see cref="ActiveTabBackground"/> dependency property.</summary>
    public static readonly DependencyProperty ActiveTabBackgroundProperty =
        DependencyProperty.Register(nameof(ActiveTabBackground), typeof(Brush), typeof(RibbonTabItem), new PropertyMetadata());

    static RibbonTabItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonTabItem), new FrameworkPropertyMetadata(typeof(RibbonTabItem)));
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public RibbonTabItem()
    {
        this.Loaded += this.OnLoaded;
        this.Unloaded += this.OnUnloaded;
    }
}
