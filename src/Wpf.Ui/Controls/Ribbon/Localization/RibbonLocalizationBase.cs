// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Reflection;
using Wpf.Ui.Controls.Localization.Languages;

#pragma warning disable 8618
namespace Wpf.Ui.Controls.Localization;

/// <summary>
/// Base class for localizations.
/// </summary>
public abstract class RibbonLocalizationBase : INotifyPropertyChanged, IEquatable<RibbonLocalizationBase>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonLocalizationBase"/> class.
    /// Creates a new instance and initializes <see cref="CultureName"/> and <see cref="DisplayName"/> from <see cref="RibbonLocalizationAttribute"/>.
    /// </summary>
    protected RibbonLocalizationBase()
    {
        this.CultureName = this.GetType().GetCustomAttribute<RibbonLocalizationAttribute>()?.CultureName;
        this.DisplayName = this.GetType().GetCustomAttribute<RibbonLocalizationAttribute>()?.DisplayName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonLocalizationBase"/> class.
    /// Creates a new instance.
    /// </summary>
    protected RibbonLocalizationBase(string cultureName, string displayName)
    {
        this.CultureName = cultureName;
        this.DisplayName = displayName;
    }

    /// <summary>
    /// Gets the culture name.
    /// </summary>
    public string? CultureName { get; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string? DisplayName { get; }

    /// <summary>
    /// Fallback instance of <see cref="English"/> for localization.
    /// </summary>
    public static readonly RibbonLocalizationBase FallbackLocalization = new English();

    /// <summary>
    /// Gets text for representing "Automatic"
    /// </summary>
    public abstract string Automatic { get; }

    /// <summary>
    /// Gets the Uid of the backstage back button
    /// </summary>
    public abstract string BackstageBackButtonUid { get; }

    /// <summary>
    /// Gets KeyTip of backstage button
    /// </summary>
    public abstract string BackstageButtonKeyTip { get; }

    /// <summary>
    /// Gets text of backstage button
    /// </summary>
    public abstract string BackstageButtonText { get; }

    /// <summary>
    /// Gets customize Status Bar
    /// </summary>
    public abstract string CustomizeStatusBar { get; }

    /// <summary>
    /// Gets text for representing "More colors..."
    /// </summary>
    public abstract string MoreColors { get; }

    /// <summary>
    /// Gets text for representing "No color"
    /// </summary>
    public abstract string NoColor { get; }

    /// <summary>
    /// Gets quick Access ToolBar DropDown Button ToolTip
    /// </summary>
    public abstract string QuickAccessToolBarDropDownButtonTooltip { get; }

    /// <summary>
    /// Gets quick Access ToolBar  Menu Header
    /// </summary>
    public abstract string QuickAccessToolBarMenuHeader { get; }

    /// <summary>
    /// Gets quick Access ToolBar Menu Minimize Quick Access Toolbar
    /// </summary>
    public abstract string QuickAccessToolBarMenuShowAbove { get; }

    /// <summary>
    /// Gets quick Access ToolBar Minimize Quick Access Toolbar
    /// </summary>
    public abstract string QuickAccessToolBarMenuShowBelow { get; }

    /// <summary>
    /// Gets quick Access ToolBar MoreControls Button ToolTip
    /// </summary>
    public abstract string QuickAccessToolBarMoreControlsButtonTooltip { get; }

    /// <summary>
    /// Gets quick Access ToolBar Menu Add Gallery
    /// </summary>
    public abstract string RibbonContextMenuAddGallery { get; }

    /// <summary>
    /// Gets quick Access ToolBar Menu Add Group
    /// </summary>
    public abstract string RibbonContextMenuAddGroup { get; }

    /// <summary>
    /// Gets quick Access ToolBar Menu Add Item
    /// </summary>
    public abstract string RibbonContextMenuAddItem { get; }

    /// <summary>
    /// Gets quick Access ToolBar Menu Add Menu
    /// </summary>
    public abstract string RibbonContextMenuAddMenu { get; }

    /// <summary>
    /// Gets ribbon Context Menu Customize Quick Access Toolbar
    /// </summary>
    public abstract string RibbonContextMenuCustomizeQuickAccessToolBar { get; }

    /// <summary>
    /// Gets ribbon Context Menu Customize the ribbon
    /// </summary>
    public abstract string RibbonContextMenuCustomizeRibbon { get; }

    /// <summary>
    /// Gets ribbon Context Menu Minimize the ribbon
    /// </summary>
    public abstract string RibbonContextMenuMinimizeRibbon { get; }

    /// <summary>
    /// Gets quick Access ToolBar Menu Remove Item
    /// </summary>
    public abstract string RibbonContextMenuRemoveItem { get; }

    /// <summary>
    /// Gets ribbon Context Menu Minimize Quick Access Toolbar
    /// </summary>
    public abstract string RibbonContextMenuShowAbove { get; }

    /// <summary>
    /// Gets ribbon Context Menu Minimize Quick Access Toolbar
    /// </summary>
    public abstract string RibbonContextMenuShowBelow { get; }

    /// <summary>
    /// Gets show Ribbon
    /// </summary>
    public virtual string ShowRibbon { get; }

    /// <summary>
    /// Gets expand Ribbon
    /// </summary>
    public virtual string ExpandRibbon { get; }

    /// <summary>
    /// Gets minimize Ribbon
    /// </summary>
    public virtual string MinimizeRibbon { get; }

    /// <summary>
    /// Gets ribbon Layout
    /// </summary>
    public virtual string RibbonLayout { get; }

    /// <summary>
    /// Gets use classic Ribbon
    /// </summary>
    public abstract string UseClassicRibbon { get; }

    /// <summary>
    /// Gets use simplified Ribbon
    /// </summary>
    public abstract string UseSimplifiedRibbon { get; }

    /// <summary>
    /// Gets displayOptions Button ScreenTip Title
    /// </summary>
    public virtual string DisplayOptionsButtonScreenTipTitle { get; }

    /// <summary>
    /// Gets displayOptions Button ScreenTip Text
    /// </summary>
    public virtual string DisplayOptionsButtonScreenTipText { get; }

    /// <summary>
    /// Gets ScreenTip's disable reason header
    /// </summary>
    public abstract string ScreenTipDisableReasonHeader { get; }

    /// <summary>
    /// Gets ScreenTip's disable reason header
    /// </summary>
    public abstract string ScreenTipF1LabelHeader { get; }

    /// <summary>
    /// Change notifications are not implemented.
    /// This class only implements <see cref="INotifyPropertyChanged"/> to prevent WPF from trying to listen to changes by using other ways than listening for this event.
    /// </summary>
#pragma warning disable 67
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67

    /// <inheritdoc />
    public bool Equals(RibbonLocalizationBase? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return this.CultureName == other.CultureName;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj is RibbonLocalizationBase localizationBase
               && this.Equals(localizationBase);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return this.CultureName is not null
#pragma warning disable CA1307 // Specify StringComparison for clarity
            ? this.CultureName.GetHashCode()
#pragma warning restore CA1307 // Specify StringComparison for clarity
            : 0;
    }

    /// <summary>
    ///
    /// </summary>
    public static bool operator ==(RibbonLocalizationBase left, RibbonLocalizationBase right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///
    /// </summary>
    public static bool operator !=(RibbonLocalizationBase left, RibbonLocalizationBase right)
    {
        return !Equals(left, right);
    }
}