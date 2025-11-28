// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Media.Effects;

namespace Wpf.Ui.Controls;

/// <summary>
/// An effect that turns the input into shades of a single color.
/// </summary>
public class GrayscaleEffect : ShaderEffect
{
    /// <summary>
    /// Dependency property for Input
    /// </summary>
    public static readonly DependencyProperty InputProperty =
        RegisterPixelShaderSamplerProperty(nameof(Input), typeof(GrayscaleEffect), 0);

    /// <summary>Identifies the <see cref="FilterColor"/> dependency property.</summary>
    public static readonly DependencyProperty FilterColorProperty =
        DependencyProperty.Register(
            nameof(FilterColor),
            typeof(Color),
            typeof(GrayscaleEffect),
            new PropertyMetadata(Color.FromArgb(255, 255, 255, 255), PixelShaderConstantCallback(0)));

    /// <summary>
    /// Initializes a new instance of the <see cref="GrayscaleEffect"/> class.
    /// Default constructor
    /// </summary>
    public GrayscaleEffect()
    {
        this.PixelShader = this.CreatePixelShader();

        this.UpdateShaderValue(InputProperty);
        this.UpdateShaderValue(FilterColorProperty);
    }

    private PixelShader CreatePixelShader()
    {
        var pixelShader = new PixelShader { UriSource = new Uri("pack://application:,,,/Wpf.Ui;component/Resources/Theme/Effects/Grayscale.ps", UriKind.RelativeOrAbsolute) };

        return pixelShader;
    }

    /// <summary>
    /// Gets or sets implicit input
    /// </summary>
    public Brush Input
    {
        get => (Brush)this.GetValue(InputProperty);
        set => this.SetValue(InputProperty, value);
    }

    /// <summary>
    /// Gets or sets the color used to tint the input.
    /// </summary>
    public Color FilterColor
    {
        get => (Color)this.GetValue(FilterColorProperty);
        set => this.SetValue(FilterColorProperty, value);
    }
}