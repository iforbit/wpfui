// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.Controls;

namespace Wpf.Ui.Demo.Mvvm.Helpers;

public class FixedPointFormatter : INumberFormatter, INumberParser
{
    // 숫자를 F7 포맷으로 변환합니다.
    public string FormatDouble(double? value)
    {
        return value.HasValue
           ? value.Value.ToString("F7", CultureInfo.InvariantCulture)
           : string.Empty;
    }

    // 문자열을 double 값으로 파싱합니다.
    public double? ParseDouble(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }

        return null;
    }

    public string FormatInt(int? value)
    {
        return value?.ToString(GetFormatSpecifier(), GetCurrentCultureConverter()) ?? string.Empty;
    }

    /// <inheritdoc />
    public string FormatUInt(uint? value)
    {
        return value?.ToString(GetFormatSpecifier(), GetCurrentCultureConverter()) ?? string.Empty;
    }

    private static string GetFormatSpecifier()
    {
        return "G";
    }

    private static CultureInfo GetCurrentCultureConverter()
    {
        return CultureInfo.CurrentCulture;
    }

    public int? ParseInt(string? value)
    {
        throw new NotImplementedException();
    }

    public uint? ParseUInt(string? value)
    {
        throw new NotImplementedException();
    }
}
