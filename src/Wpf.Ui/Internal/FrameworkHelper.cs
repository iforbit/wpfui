// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Reflection;
using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Internal;

/// <summary>
/// Represents class to determine .NET Framework version difference
/// </summary>
public static class FrameworkHelper
{
    /// <summary>
    /// Version of WPF
    /// </summary>
    public static readonly Version PresentationFrameworkVersion = Assembly.GetAssembly(typeof(Window))!.GetName()!.Version!;

    /// <summary>
    /// Gets UseLayoutRounding attached property value
    /// </summary>
    /// <returns></returns>
    public static bool GetUseLayoutRounding(DependencyObject obj)
    {
        return (bool)obj.GetValue(UseLayoutRoundingProperty);
    }

    /// <summary>
    /// Gets UseLayoutRounding attached property value
    /// </summary>
    public static void SetUseLayoutRounding(DependencyObject obj, bool value)
    {
        obj.SetValue(UseLayoutRoundingProperty, BooleanBoxes.Box(value));
    }

    /// <summary>
    ///  Using a DependencyProperty as the backing store for UseLayoutRounding.  This enables animation, styling, binding, etc...
    /// </summary>
    public static readonly DependencyProperty UseLayoutRoundingProperty =
        DependencyProperty.RegisterAttached("UseLayoutRounding", typeof(bool), typeof(FrameworkHelper), new PropertyMetadata(BooleanBoxes.FalseBox, OnUseLayoutRoundingChanged));

    private static void OnUseLayoutRoundingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.SetCurrentValue(UIElement.SnapsToDevicePixelsProperty, BooleanBoxes.TrueBox);
        d.SetCurrentValue(FrameworkElement.UseLayoutRoundingProperty, BooleanBoxes.TrueBox);
    }
}