// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.
//
// Based on Fluent.Ribbon: https://github.com/fluentribbon/Fluent.Ribbon

using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Wpf.Ui.Internal;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents adorner for Backstage
/// </summary>
internal class BackstageAdorner : Adorner
{
    // Content of Backstage
    private readonly UIElement? backstageContent;

    // Collection of visual children
    private readonly VisualCollection visualChildren;
    private readonly Rectangle background;
    private readonly BackstageTabControl? backstageTabControl;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackstageAdorner"/> class.
    /// </summary>
    /// <param name="adornedElement">Adorned element</param>
    /// <param name="backstage">Backstage</param>
    public BackstageAdorner(FrameworkElement adornedElement, Backstage backstage)
        : base(adornedElement)
    {
        KeyboardNavigation.SetTabNavigation(this, KeyboardNavigationMode.Contained);
        KeyboardNavigation.SetControlTabNavigation(this, KeyboardNavigationMode.Contained);
        KeyboardNavigation.SetDirectionalNavigation(this, KeyboardNavigationMode.Contained);

        this.Backstage = backstage;
        this.backstageContent = this.Backstage.Content;

        if (this.backstageContent is not null)
        {
            this.backstageTabControl = this.backstageContent as BackstageTabControl
                                       ?? UIHelper.FindVisualChild<BackstageTabControl>(this.backstageContent);
        }

        // Create opaque background to block content behind
        this.background = new Rectangle
        {
            IsHitTestVisible = false,
            Fill = Brushes.White, // 기본값, 곧 바인딩으로 덮어씀
            Opacity = 1.0 // 완전 불투명
        };

        if (this.backstageTabControl is not null)
        {
            _ = BindingOperations.SetBinding(this.background, Shape.FillProperty, new Binding
            {
                Path = new PropertyPath(Control.BackgroundProperty),
                Source = this.backstageTabControl
            });

            _ = BindingOperations.SetBinding(this.background, MarginProperty, new Binding
            {
                Path = new PropertyPath(MarginProperty),
                Source = this.backstageTabControl
            });
        }
        else
        {
            // WPF.UI theme system: Use dynamic resource instead of hard-coded color
            this.background.SetResourceReference(Shape.FillProperty, "ApplicationBackgroundBrush");
        }

        this.visualChildren = new VisualCollection(this)
        {
            this.background,
            this.backstageContent
        };
    }

    /// <summary>
    /// Gets the <see cref="Backstage"/>.
    /// </summary>
    public Backstage Backstage { get; }

    public void Clear()
    {
        BindingOperations.ClearAllBindings(this.background);

        this.visualChildren.Clear();
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        double yOffset = this.GetTitleBarHeight();

        // Arrange background to cover entire area (compensate margin used by animation)
        this.background.Arrange(new Rect(this.Margin.Left * -1, yOffset, Math.Max(0, finalSize.Width), Math.Max(0, finalSize.Height - yOffset)));

        this.backstageContent?.Arrange(new Rect(0, yOffset, Math.Max(0, finalSize.Width), Math.Max(0, finalSize.Height - yOffset)));

        return finalSize;
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size constraint)
    {
        double yOffset = this.GetTitleBarHeight();

        var size = new Size(Math.Max(0, this.AdornedElement.RenderSize.Width), Math.Max(0, this.AdornedElement.RenderSize.Height - yOffset));

        this.background.Measure(size);
        this.backstageContent?.Measure(size);

        return this.AdornedElement.RenderSize;
    }

    /// <summary>
    /// Gets the height of TitleBar (Grid.Row="0") to avoid covering it with the Backstage.
    /// </summary>
    private double GetTitleBarHeight()
    {
        // Find the Window that contains this adorner
        if (Window.GetWindow(this.AdornedElement) is not Window window)
        {
            return 0;
        }

        // Find the main Grid in the window
        if (window.Content is not Grid mainGrid)
        {
            return 0;
        }

        // Check if the Grid has at least one row
        if (mainGrid.RowDefinitions.Count == 0)
        {
            return 0;
        }

        // Find the first child element in Grid.Row="0" (typically TitleBar)
        UIElement? titleBarElement = null;
        foreach (UIElement child in mainGrid.Children)
        {
            int row = Grid.GetRow(child);
            if (row == 0)
            {
                titleBarElement = child;
                break;
            }
        }

        // Return the ActualHeight of the TitleBar element
        if (titleBarElement is FrameworkElement titleBar)
        {
            return titleBar.ActualHeight;
        }

        return 0;
    }

    /// <inheritdoc />
    protected override int VisualChildrenCount => this.visualChildren.Count;

    /// <inheritdoc />
    protected override Visual GetVisualChild(int index)
    {
        return this.visualChildren[index];
    }
}
