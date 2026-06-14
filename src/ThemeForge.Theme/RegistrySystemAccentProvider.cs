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

using System.Windows.Media;
using Microsoft.Win32;

namespace ThemeForge.Theme;

/// <summary>Reads the Windows accent color from the current user's DWM registry hive.</summary>
public sealed class RegistrySystemAccentProvider : ISystemAccentProvider, IDisposable
{
    private const string DwmKey = @"Software\Microsoft\Windows\DWM";
    private const string AccentColorValue = "AccentColor";

    private bool _subscribed;

    public RegistrySystemAccentProvider()
    {
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        _subscribed = true;
    }

    /// <inheritdoc/>
    public event EventHandler? Changed;

    /// <inheritdoc/>
    public Color? GetCurrentAccent()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(DwmKey);
        if (key?.GetValue(AccentColorValue) is int abgr)
        {
            byte red = (byte)(abgr & 0xFF);
            byte green = (byte)((abgr >> 8) & 0xFF);
            byte blue = (byte)((abgr >> 16) & 0xFF);
            return Color.FromRgb(red, green, blue);
        }

        return null;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_subscribed)
        {
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
            _subscribed = false;
        }
    }

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.Color)
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
