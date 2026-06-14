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

using ThemeForge.Theme;

namespace ThemeForge.WpfApp;

/// <summary>
/// Single source of the app's default light/dark pair, shared by the startup
/// bootstrap and the view model so both reason about the same themes. Edit the
/// pair here to rebrand the app.
/// </summary>
public sealed class AppThemeConfig
{
    /// <summary>Theme used when Windows is in light mode.</summary>
    public string LightTheme => ThemeNames.Folio;

    /// <summary>Theme used when Windows is in dark mode, and as the default theme.</summary>
    public string DarkTheme => ThemeNames.Drakul;

    /// <summary>The combined Follow Windows mapping built from the pair above.</summary>
    public WindowsFollowOptions Follow => new WindowsFollowOptions
    {
        LightTheme = LightTheme,
        DarkTheme = DarkTheme,
        FollowAccent = true,
    };
}
