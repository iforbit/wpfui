// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.Interop;

namespace Wpf.Ui.Appearance;

/// <summary>
/// Allows updating the accents used by controls in the application by swapping dynamic resources.
/// </summary>
/// <example>
/// <code lang="csharp">
/// ApplicationAccentColorManager.Apply(
///     Color.FromArgb(0xFF, 0xEE, 0x00, 0xBB),
///     ApplicationTheme.Dark,
///     false
/// );
/// </code>
/// <code lang="csharp">
/// ApplicationAccentColorManager.Apply(
///     ApplicationAccentColorManager.GetColorizationColor(),
///     ApplicationTheme.Dark,
///     false
/// );
/// </code>
/// </example>
public static class ApplicationAccentColorManager
{
    /// <summary>
    /// The maximum value of the background HSV brightness after which the text on the accent will be turned dark.
    /// </summary>
    private const double BackgroundBrightnessThresholdValue = 80d;

    /// <summary>
    /// Gets the SystemAccentColor.
    /// </summary>
    public static Color SystemAccent
    {
        get
        {
            object? resource = UiApplication.Current.Resources["SystemAccentColor"];

            if (resource is Color color)
            {
                return color;
            }

            return Colors.Transparent;
        }
    }

    private static SolidColorBrush? _cachedSystemAccentBrush;
    private static Color _cachedSystemAccentColor;

    /// <summary>
    /// Gets the <see cref="Brush"/> of the SystemAccentColor.
    /// Returns a frozen brush for optimal performance.
    /// </summary>
    public static Brush SystemAccentBrush
    {
        get
        {
            Color currentColor = SystemAccent;
            if (_cachedSystemAccentBrush == null || _cachedSystemAccentColor != currentColor)
            {
                var brush = new SolidColorBrush(currentColor);
                brush.Freeze();
                _cachedSystemAccentBrush = brush;
                _cachedSystemAccentColor = currentColor;
            }

            return _cachedSystemAccentBrush;
        }
    }

    /// <summary>
    /// Gets the SystemAccentColorPrimary.
    /// </summary>
    public static Color PrimaryAccent
    {
        get
        {
            object? resource = UiApplication.Current.Resources["SystemAccentColorPrimary"];

            if (resource is Color color)
            {
                return color;
            }

            return Colors.Transparent;
        }
    }

    private static SolidColorBrush? _cachedPrimaryAccentBrush;
    private static Color _cachedPrimaryAccentColor;

    /// <summary>
    /// Gets the <see cref="Brush"/> of the SystemAccentColorPrimary.
    /// Returns a frozen brush for optimal performance.
    /// </summary>
    public static Brush PrimaryAccentBrush
    {
        get
        {
            Color currentColor = PrimaryAccent;
            if (_cachedPrimaryAccentBrush == null || _cachedPrimaryAccentColor != currentColor)
            {
                var brush = new SolidColorBrush(currentColor);
                brush.Freeze();
                _cachedPrimaryAccentBrush = brush;
                _cachedPrimaryAccentColor = currentColor;
            }

            return _cachedPrimaryAccentBrush;
        }
    }

    /// <summary>
    /// Gets the SystemAccentColorSecondary.
    /// </summary>
    public static Color SecondaryAccent
    {
        get
        {
            object? resource = UiApplication.Current.Resources["SystemAccentColorSecondary"];

            if (resource is Color color)
            {
                return color;
            }

            return Colors.Transparent;
        }
    }

    private static SolidColorBrush? _cachedSecondaryAccentBrush;
    private static Color _cachedSecondaryAccentColor;

    /// <summary>
    /// Gets the <see cref="Brush"/> of the SystemAccentColorSecondary.
    /// Returns a frozen brush for optimal performance.
    /// </summary>
    public static Brush SecondaryAccentBrush
    {
        get
        {
            Color currentColor = SecondaryAccent;
            if (_cachedSecondaryAccentBrush == null || _cachedSecondaryAccentColor != currentColor)
            {
                var brush = new SolidColorBrush(currentColor);
                brush.Freeze();
                _cachedSecondaryAccentBrush = brush;
                _cachedSecondaryAccentColor = currentColor;
            }

            return _cachedSecondaryAccentBrush;
        }
    }

    /// <summary>
    /// Gets the .NET 9 system accent color directly from SystemColors.
    /// This provides automatic updates when user changes accent color in Windows settings.
    /// </summary>
    public static Color Net9SystemAccent
    {
        get
        {
            // .NET 9 WPF Fluent 테마 시스템 액센트 컬러 지원
            try
            {
                return SystemColors.AccentColor;
            }
            catch
            {
                // Fallback to current system if .NET 9 properties are not available
                return GetColorizationColor();
            }
        }
    }

    /// <summary>
    /// Gets the .NET 9 system accent color brush.
    /// Uses DynamicResource for automatic updates.
    /// </summary>
    public static Brush Net9SystemAccentBrush
    {
        get
        {
            try
            {
                return SystemColors.AccentColorBrush;
            }
            catch
            {
                // Fallback to current system with frozen brush
                var brush = new SolidColorBrush(GetColorizationColor());
                brush.Freeze();
                return brush;
            }
        }
    }

    /// <summary>
    /// Gets the SystemAccentColorTertiary.
    /// </summary>
    public static Color TertiaryAccent
    {
        get
        {
            object? resource = UiApplication.Current.Resources["SystemAccentColorTertiary"];

            if (resource is Color color)
            {
                return color;
            }

            return Colors.Transparent;
        }
    }

    private static SolidColorBrush? _cachedTertiaryAccentBrush;
    private static Color _cachedTertiaryAccentColor;

    /// <summary>
    /// Gets the <see cref="Brush"/> of the SystemAccentColorTertiary.
    /// Returns a frozen brush for optimal performance.
    /// </summary>
    public static Brush TertiaryAccentBrush
    {
        get
        {
            Color currentColor = TertiaryAccent;
            if (_cachedTertiaryAccentBrush == null || _cachedTertiaryAccentColor != currentColor)
            {
                var brush = new SolidColorBrush(currentColor);
                brush.Freeze();
                _cachedTertiaryAccentBrush = brush;
                _cachedTertiaryAccentColor = currentColor;
            }

            return _cachedTertiaryAccentBrush;
        }
    }

    /// <summary>
    /// Changes the color accents of the application based on the color entered.
    /// </summary>
    /// <param name="systemAccent">Primary accent color.</param>
    /// <param name="applicationTheme">If <see cref="ApplicationTheme.Dark"/>, the colors will be different.</param>
    /// <param name="systemGlassColor">If the color is taken from the Glass Color System, its brightness will be increased with the help of the operations on HSV space.</param>
    public static void Apply(
        Color systemAccent,
        ApplicationTheme applicationTheme = ApplicationTheme.Light,
        bool systemGlassColor = false
    )
    {
        if (systemGlassColor)
        {
            // WindowGlassColor is little darker than accent color
            systemAccent = systemAccent.UpdateBrightness(6f);
        }

        Color primaryAccent;
        Color secondaryAccent;
        Color tertiaryAccent;

        if (applicationTheme == ApplicationTheme.Dark)
        {
            primaryAccent = systemAccent.Update(15f, -12f);
            secondaryAccent = systemAccent.Update(30f, -24f);
            tertiaryAccent = systemAccent.Update(45f, -36f);
        }
        else
        {
            primaryAccent = systemAccent.UpdateBrightness(-5f);
            secondaryAccent = systemAccent.UpdateBrightness(-10f);
            tertiaryAccent = systemAccent.UpdateBrightness(-15f);
        }

        UpdateColorResources(systemAccent, primaryAccent, secondaryAccent, tertiaryAccent);
    }

    /// <summary>
    /// Changes the color accents of the application based on the entered colors.
    /// </summary>
    /// <param name="systemAccent">Primary color.</param>
    /// <param name="primaryAccent">Alternative light or dark color.</param>
    /// <param name="secondaryAccent">Second alternative light or dark color (most used).</param>
    /// <param name="tertiaryAccent">Third alternative light or dark color.</param>
    public static void Apply(
        Color systemAccent,
        Color primaryAccent,
        Color secondaryAccent,
        Color tertiaryAccent
    )
    {
        UpdateColorResources(systemAccent, primaryAccent, secondaryAccent, tertiaryAccent);
    }

    /// <summary>
    /// Applies system accent color to the application.
    /// </summary>
    public static void ApplySystemAccent()
    {
        Apply(GetColorizationColor(), ApplicationThemeManager.GetAppTheme());
    }

    /// <summary>
    /// Applies .NET 9 system accent color with automatic updates.
    /// This method enables integration with Windows 11 Fluent design system.
    /// </summary>
    public static void ApplyNet9SystemAccent()
    {
        try
        {
            // .NET 9 WPF Fluent 테마 시스템 액센트 컬러 적용
            Color net9AccentColor = Net9SystemAccent;
            Apply(net9AccentColor, ApplicationThemeManager.GetAppTheme());

            // 추가로 .NET 9 시스템 컬러 리소스 업데이트
            UpdateNet9SystemColorResources();
        }
        catch
        {
            // Fallback to traditional method if .NET 9 features are not available
            ApplySystemAccent();
        }
    }

    /// <summary>
    /// Updates .NET 9 system color resources for dynamic accent color support.
    /// </summary>
    private static void UpdateNet9SystemColorResources()
    {
        try
        {
            // 동적 시스템 액센트 컬러 브러시 리소스 업데이트
            if (UiApplication.Current?.Resources != null)
            {
                // .NET 9 시스템 액센트 컬러를 WPF UI 리소스와 연결
                UiApplication.Current.Resources["SystemAccentColorBrush"] = Net9SystemAccentBrush;
                UiApplication.Current.Resources["SystemFillColorAttentionBrush"] = Net9SystemAccentBrush;

                System.Diagnostics.Debug.WriteLine(
                    "INFO | .NET 9 System Accent Colors applied successfully",
                    "Wpf.Ui.Accent"
                );
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"WARNING | Failed to update .NET 9 system color resources: {ex.Message}",
                "Wpf.Ui.Accent"
            );
        }
    }

    /// <summary>
    /// Gets current Desktop Window Manager colorization color.
    /// <para>It should be the color defined in the system Personalization.</para>
    /// </summary>
    public static Color GetColorizationColor()
    {
        return UnsafeNativeMethods.GetDwmColor();
    }

    /// <summary>
    /// Updates application resources.
    /// </summary>
    private static void UpdateColorResources(
        Color systemAccent,
        Color primaryAccent,
        Color secondaryAccent,
        Color tertiaryAccent
    )
    {
        // System.Diagnostics.Debug.WriteLine("INFO | SystemAccentColor: " + systemAccent, "Wpf.Ui.Accent");
        // System.Diagnostics.Debug.WriteLine(
        //     "INFO | SystemAccentColorPrimary: " + primaryAccent,
        //     "Wpf.Ui.Accent"
        // );
        // System.Diagnostics.Debug.WriteLine(
        //     "INFO | SystemAccentColorSecondary: " + secondaryAccent,
        //     "Wpf.Ui.Accent"
        // );
        // System.Diagnostics.Debug.WriteLine(
        //     "INFO | SystemAccentColorTertiary: " + tertiaryAccent,
        //     "Wpf.Ui.Accent"
        // );
        if (secondaryAccent.GetBrightness() > BackgroundBrightnessThresholdValue)
        {
            // System.Diagnostics.Debug.WriteLine("INFO | Text on accent is DARK", "Wpf.Ui.Accent");
            UiApplication.Current.Resources["TextOnAccentFillColorPrimary"] = Color.FromArgb(
                0xFF,
                0x00,
                0x00,
                0x00
            );
            UiApplication.Current.Resources["TextOnAccentFillColorSecondary"] = Color.FromArgb(
                0x80,
                0x00,
                0x00,
                0x00
            );
            UiApplication.Current.Resources["TextOnAccentFillColorDisabled"] = Color.FromArgb(
                0x77,
                0x00,
                0x00,
                0x00
            );
            UiApplication.Current.Resources["TextOnAccentFillColorSelectedText"] = Color.FromArgb(
                0x00,
                0x00,
                0x00,
                0x00
            );
            UiApplication.Current.Resources["AccentTextFillColorDisabled"] = Color.FromArgb(
                0x5D,
                0x00,
                0x00,
                0x00
            );
        }
        else
        {
            // System.Diagnostics.Debug.WriteLine("INFO | Text on accent is LIGHT", "Wpf.Ui.Accent");
            UiApplication.Current.Resources["TextOnAccentFillColorPrimary"] = Color.FromArgb(
                0xFF,
                0xFF,
                0xFF,
                0xFF
            );
            UiApplication.Current.Resources["TextOnAccentFillColorSecondary"] = Color.FromArgb(
                0x80,
                0xFF,
                0xFF,
                0xFF
            );
            UiApplication.Current.Resources["TextOnAccentFillColorDisabled"] = Color.FromArgb(
                0x87,
                0xFF,
                0xFF,
                0xFF
            );
            UiApplication.Current.Resources["TextOnAccentFillColorSelectedText"] = Color.FromArgb(
                0xFF,
                0xFF,
                0xFF,
                0xFF
            );
            UiApplication.Current.Resources["AccentTextFillColorDisabled"] = Color.FromArgb(
                0x5D,
                0xFF,
                0xFF,
                0xFF
            );
        }

        UiApplication.Current.Resources["SystemAccentColor"] = systemAccent;
        UiApplication.Current.Resources["SystemAccentColorPrimary"] = primaryAccent;
        UiApplication.Current.Resources["SystemAccentColorSecondary"] = secondaryAccent;
        UiApplication.Current.Resources["SystemAccentColorTertiary"] = tertiaryAccent;

        UiApplication.Current.Resources["SystemAccentBrush"] = secondaryAccent.ToBrush();
        UiApplication.Current.Resources["SystemFillColorAttentionBrush"] = secondaryAccent.ToBrush();
        UiApplication.Current.Resources["AccentTextFillColorPrimaryBrush"] = tertiaryAccent.ToBrush();
        UiApplication.Current.Resources["AccentTextFillColorSecondaryBrush"] = tertiaryAccent.ToBrush();
        UiApplication.Current.Resources["AccentTextFillColorTertiaryBrush"] = secondaryAccent.ToBrush();
        UiApplication.Current.Resources["AccentFillColorSelectedTextBackgroundBrush"] =
            systemAccent.ToBrush();
        UiApplication.Current.Resources["AccentFillColorDefaultBrush"] = secondaryAccent.ToBrush();

        UiApplication.Current.Resources["AccentFillColorSecondaryBrush"] = secondaryAccent.ToBrush(0.9);
        UiApplication.Current.Resources["AccentFillColorTertiaryBrush"] = secondaryAccent.ToBrush(0.8);
    }
}
