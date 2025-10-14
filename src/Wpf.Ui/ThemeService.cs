// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.Appearance;

namespace Wpf.Ui;

/// <summary>
/// Lets you set the app theme.
/// </summary>
public partial class ThemeService : IThemeService
{
    /// <inheritdoc />
    public virtual ApplicationTheme GetTheme() => ApplicationThemeManager.GetAppTheme();

    /// <inheritdoc />
    public virtual SystemTheme GetNativeSystemTheme() => ApplicationThemeManager.GetSystemTheme();

    /// <inheritdoc />
    public virtual ApplicationTheme GetSystemTheme()
    {
        SystemTheme systemTheme = ApplicationThemeManager.GetSystemTheme();

        // .NET 9 최적화: or 패턴으로 그룹화하여 성능 향상
        return systemTheme switch
        {
            SystemTheme.Light or SystemTheme.Sunrise or SystemTheme.Flow => ApplicationTheme.Light,
            SystemTheme.Dark or SystemTheme.Glow or SystemTheme.CapturedMotion => ApplicationTheme.Dark,
            SystemTheme.HCBlack or SystemTheme.HC1 or SystemTheme.HC2 or SystemTheme.HCWhite => ApplicationTheme.HighContrast,
            _ => ApplicationTheme.Unknown,
        };
    }

    /// <inheritdoc />
    public virtual bool SetTheme(ApplicationTheme applicationTheme)
    {
        if (ApplicationThemeManager.GetAppTheme() == applicationTheme)
        {
            return false;
        }

        ApplicationThemeManager.Apply(applicationTheme);

        return true;
    }

    /// <inheritdoc />
    public bool SetSystemAccent()
    {
        ApplicationAccentColorManager.ApplySystemAccent();

        return true;
    }

    /// <summary>
    /// Sets the .NET 9 system accent color with automatic updates.
    /// This provides integration with Windows 11 Fluent design system.
    /// </summary>
    /// <returns><see langword="true"/> if operation was successful.</returns>
    public bool SetNet9SystemAccent()
    {
        try
        {
            ApplicationAccentColorManager.ApplyNet9SystemAccent();
            return true;
        }
        catch
        {
            // Fallback to traditional method
            return SetSystemAccent();
        }
    }

    /// <inheritdoc />
    public bool SetAccent(Color accentColor)
    {
        ApplicationAccentColorManager.Apply(accentColor);

        return true;
    }

    /// <inheritdoc />
    public bool SetAccent(SolidColorBrush accentSolidBrush)
    {
        Color color = accentSolidBrush.Color;
        color.A = (byte)Math.Round(accentSolidBrush.Opacity * byte.MaxValue);

        ApplicationAccentColorManager.Apply(color);

        return true;
    }
}
