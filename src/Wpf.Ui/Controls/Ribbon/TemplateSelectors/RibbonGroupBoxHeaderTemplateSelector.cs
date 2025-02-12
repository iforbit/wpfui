// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;

namespace Wpf.Ui.Controls.TemplateSelectors;

/// <summary>
/// <see cref="DataTemplateSelector"/> for the header of <see cref="RibbonGroupBox"/>.
/// </summary>
public class RibbonGroupBoxHeaderTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// Gets a static instance of <see cref="RibbonGroupBoxHeaderTemplateSelector"/>.
    /// </summary>
    public static readonly RibbonGroupBoxHeaderTemplateSelector Instance = new();

    /// <inheritdoc />
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        var element = (FrameworkElement)container;

        if (RibbonGroupBox.GetIsCollapsedHeaderContentPresenter(element))
        {
            return (DataTemplate)element.FindResource("controls.Ribbon.DataTemplates.RibbonGroupBox.TwoLineHeader");
        }

        return (DataTemplate)element.FindResource("controls.Ribbon.DataTemplates.RibbonGroupBox.OneLineHeader");
    }
}
