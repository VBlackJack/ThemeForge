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

namespace ThemeForge.Theme;

/// <summary>
/// Payload of <see cref="IThemeService.ThemeChanged"/>. Carries enough context
/// for handlers to log, animate transitions, or selectively re-render.
/// </summary>
public sealed class ThemeChangedEventArgs : EventArgs
{
    public ThemeChangedEventArgs(string previousTheme, string currentTheme, int revision)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currentTheme);
        PreviousTheme = previousTheme;
        CurrentTheme = currentTheme;
        Revision = revision;
    }

    /// <summary>The theme that was active before the swap, or empty on first apply.</summary>
    public string PreviousTheme { get; }

    /// <summary>The theme that has just been applied.</summary>
    public string CurrentTheme { get; }

    /// <summary>Value of <see cref="IThemeService.ThemeRevision"/> after the swap.</summary>
    public int Revision { get; }
}
