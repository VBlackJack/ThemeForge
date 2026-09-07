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

using System.Windows;

namespace ThemeForge.Theme;

public sealed partial class ThemeService
{
    /// <inheritdoc/>
    public void ApplyAccentTint(AccentTint tint)
    {
        if (!Enum.IsDefined(tint))
        {
            throw new ArgumentOutOfRangeException(nameof(tint), tint, "Unsupported accent tint.");
        }

        if (tint != AccentTint.Default && string.IsNullOrWhiteSpace(_currentTheme))
        {
            throw new InvalidOperationException("Apply a theme before applying an accent tint.");
        }

        if (tint == _currentAccentTint)
        {
            if (IsFollowingSystemAccent && !_applyingFromSystemAccent) { DisableSystemAccentFollow(); }
            return;
        }

        ResourceDictionary? replacement = tint == AccentTint.Default
            ? null : CreateAccentTintDictionary(tint, _application.Resources);
        if (IsFollowingSystemAccent && !_applyingFromSystemAccent)
        {
            DisableSystemAccentFollow();
        }

        IList<ResourceDictionary> merged = _application.Resources.MergedDictionaries;
        RemoveMarkedDictionary(merged, AccentTintMarkerKey);
        if (replacement is not null) { merged.Add(replacement); }
        _currentAccentTint = tint;
        RaiseCurrentThemeChanged();
    }
}
