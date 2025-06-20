// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.IO;

namespace Wpf.Ui.DirectX;

public static class ShaderResourceLoader
{
    public static string Load(string resourceName)
    {
        System.Reflection.Assembly asm = typeof(ShaderResourceLoader).Assembly;

        using Stream stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Shader resource not found: {resourceName}");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}