// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Markup;
using Wpf.Ui.Internal.KnowBoxes;

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents specific label to use in particular ribbon controls
/// </summary>
[DefaultProperty(nameof(Text))]
[ContentProperty(nameof(Text))]
[TemplatePart(Name = "PART_TextRun", Type = typeof(AccessText))]
[TemplatePart(Name = "PART_TextRun2", Type = typeof(AccessText))]
public class TwoLineLabel : Control
{
    /// <summary>
    /// Run with text
    /// </summary>
    private AccessText? textRun;

    private AccessText? textRun2;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether label must have two lines
    /// </summary>
    public bool HasTwoLines
    {
        get => (bool)this.GetValue(HasTwoLinesProperty);
        set => this.SetValue(HasTwoLinesProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="HasTwoLines"/> dependency property.</summary>
    public static readonly DependencyProperty HasTwoLinesProperty =
        DependencyProperty.Register(nameof(HasTwoLines), typeof(bool), typeof(TwoLineLabel), new PropertyMetadata(BooleanBoxes.TrueBox, OnHasTwoLinesChanged));

    /// <summary>
    /// Handles HasTwoLines property changes
    /// </summary>
    /// <param name="d">Object</param>
    /// <param name="e">The event data</param>
    private static void OnHasTwoLinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((TwoLineLabel)d).UpdateTextRun();
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether label has glyph
    /// </summary>
    public bool HasGlyph
    {
        get => (bool)this.GetValue(HasGlyphProperty);
        set => this.SetValue(HasGlyphProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="HasGlyph"/> dependency property.</summary>
    public static readonly DependencyProperty HasGlyphProperty =
        DependencyProperty.Register(nameof(HasGlyph), typeof(bool), typeof(TwoLineLabel), new PropertyMetadata(BooleanBoxes.FalseBox, OnHasGlyphChanged));

    /// <summary>
    /// Handles HasGlyph property changes
    /// </summary>
    /// <param name="d">Object</param>
    /// <param name="e">The event data</param>
    private static void OnHasGlyphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((TwoLineLabel)d).UpdateTextRun();
    }

    /// <summary>
    /// Gets or sets the text
    /// </summary>
#pragma warning disable WPF0012
    public string? Text
#pragma warning restore WPF0012
    {
        get { return (string?)this.GetValue(TextProperty); }
        set { this.SetValue(TextProperty, value); }
    }

    /// <summary>Identifies the <see cref="Text"/> dependency property.</summary>
    public static readonly DependencyProperty TextProperty =
#pragma warning disable WPF0010 // Default value type must match registered type.
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(TwoLineLabel), new PropertyMetadata(StringBoxes.Empty, OnTextChanged));
#pragma warning restore WPF0010 // Default value type must match registered type.

    /// <summary>
    /// Initializes static members of the <see cref="TwoLineLabel"/> class.
    /// Static constructor
    /// </summary>
    static TwoLineLabel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TwoLineLabel), new FrameworkPropertyMetadata(typeof(TwoLineLabel)));

        FocusableProperty.OverrideMetadata(typeof(TwoLineLabel), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        this.textRun = this.GetTemplateChild("PART_TextRun") as AccessText;
        this.textRun2 = this.GetTemplateChild("PART_TextRun2") as AccessText;

        this.UpdateTextRun();
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer() => new TwoLineLabelAutomationPeer(this);

    /// <summary>
    /// Handles text property changes
    /// </summary>
    /// <param name="d">Object</param>
    /// <param name="e">The event data</param>
    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var label = (TwoLineLabel)d;
        label.UpdateTextRun();
    }

    /// <summary>
    /// Updates text runs and adds newline if HasTwoLines == true
    /// </summary>
    private void UpdateTextRun()
    {
        if (this.textRun is null
            || this.textRun2 is null)
        {
            return;
        }

        var text = this.Text?.Trim();

        if (this.HasTwoLines == false
            || string.IsNullOrEmpty(text))
        {
            this.textRun.SetCurrentValue(AccessText.TextProperty, text);
            this.textRun2.SetCurrentValue(AccessText.TextProperty, string.Empty);
            return;
        }

        // Find soft hyphen, break at its position and display a normal hyphen.
#pragma warning disable CA1307 // Specify StringComparison for clarity
        var hyphenIndex = text!.IndexOf((char)173);
#pragma warning restore CA1307 // Specify StringComparison for clarity

        if (hyphenIndex >= 0)
        {
            this.textRun.SetCurrentValue(AccessText.TextProperty, text.Substring(0, hyphenIndex) + "-");
            this.textRun2.SetCurrentValue(AccessText.TextProperty, text.Substring(hyphenIndex) + " ");
        }
        else
        {
            var centerIndex = text.Length / 2;

            // Find spaces nearest to center from left and right
            var leftSpaceIndex = text.LastIndexOf(" ", centerIndex, centerIndex, StringComparison.CurrentCulture);
            var rightSpaceIndex = text.IndexOf(" ", centerIndex, StringComparison.CurrentCulture);

            if (leftSpaceIndex == -1
                && rightSpaceIndex == -1)
            {
                this.textRun.SetCurrentValue(AccessText.TextProperty, text);
                this.textRun2.SetCurrentValue(AccessText.TextProperty, string.Empty);
            }
            else if (leftSpaceIndex == -1)
            {
                // Finds only space from right. New line adds on it
                this.textRun.SetCurrentValue(AccessText.TextProperty, text.Substring(0, rightSpaceIndex));
                this.textRun2.SetCurrentValue(AccessText.TextProperty, text.Substring(rightSpaceIndex) + " ");
            }
            else if (rightSpaceIndex == -1)
            {
                // Finds only space from left. New line adds on it
                this.textRun.SetCurrentValue(AccessText.TextProperty, text.Substring(0, leftSpaceIndex));
                this.textRun2.SetCurrentValue(AccessText.TextProperty, text.Substring(leftSpaceIndex) + " ");
            }
            else
            {
                // Find nearest to center space and add new line on it
                if (Math.Abs(centerIndex - leftSpaceIndex) < Math.Abs(centerIndex - rightSpaceIndex))
                {
                    this.textRun.SetCurrentValue(AccessText.TextProperty, text.Substring(0, leftSpaceIndex));
                    this.textRun2.SetCurrentValue(AccessText.TextProperty, text.Substring(leftSpaceIndex) + " ");
                }
                else
                {
                    this.textRun.SetCurrentValue(AccessText.TextProperty, text.Substring(0, rightSpaceIndex));
                    this.textRun2.SetCurrentValue(AccessText.TextProperty, text.Substring(rightSpaceIndex) + " ");
                }
            }
        }
    }
}
