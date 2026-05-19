// Copyright 2026 Julien Bombled
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Globalization;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ThemeForge.Studio.ViewModels;

/// <summary>
/// One editable palette slot: pairs a display name + the application
/// resource key that exposes the brush, lets the user edit the hex value,
/// and patches Application.Resources on every change so the live UI updates
/// through DynamicResource.
/// </summary>
public sealed partial class SlotViewModel : ObservableObject
{
    private readonly string _resourceKey;
    private readonly Color _originalColor;
    private bool _suppressHexEcho;

    public SlotViewModel(string name, string resourceKey, Color initial)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceKey);
        Name = name;
        _resourceKey = resourceKey;
        _originalColor = initial;
        _color = initial;
        _hex = ColorToHex(initial);
    }

    /// <summary>Display label shown in the editor (e.g. "Background").</summary>
    public string Name { get; }

    /// <summary>Current color shown in the swatch.</summary>
    [ObservableProperty]
    private Color _color;

    /// <summary>Hex value bound to a TextBox; parsing happens in OnHexChanged.</summary>
    [ObservableProperty]
    private string _hex;

    partial void OnHexChanged(string value)
    {
        if (_suppressHexEcho)
        {
            return;
        }

        if (TryParseHex(value, out Color parsed))
        {
            Color = parsed;
            Application.Current.Resources[_resourceKey] = new SolidColorBrush(parsed);
        }
    }

    [RelayCommand]
    private void Reset()
    {
        _suppressHexEcho = true;
        try
        {
            Color = _originalColor;
            Hex = ColorToHex(_originalColor);
        }
        finally
        {
            _suppressHexEcho = false;
        }
        Application.Current.Resources[_resourceKey] = new SolidColorBrush(_originalColor);
    }

    private static string ColorToHex(Color c) =>
        $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    private static bool TryParseHex(string? raw, out Color color)
    {
        color = Colors.Transparent;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        string s = raw.Trim().TrimStart('#');
        if (s.Length != 6 && s.Length != 8)
        {
            return false;
        }

        try
        {
            if (s.Length == 6)
            {
                s = "FF" + s;
            }
            byte a = byte.Parse(s.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            byte r = byte.Parse(s.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            byte g = byte.Parse(s.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            byte b = byte.Parse(s.AsSpan(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            color = Color.FromArgb(a, r, g, b);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
