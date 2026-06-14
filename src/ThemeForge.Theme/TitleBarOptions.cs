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

namespace ThemeForge.Theme;

/// <summary>
/// Overrides for the themed title bar. Every color is nullable: a null value means
/// "derive from the current theme" rather than a hardcoded default.
/// </summary>
public sealed class TitleBarOptions
{
    /// <summary>Caption (title bar) color. When null, the theme background is used.</summary>
    public Color? CaptionColor { get; set; }

    /// <summary>Caption text color. When null, the theme foreground is used.</summary>
    public Color? TextColor { get; set; }

    /// <summary>
    /// Window border color. When null, the border attribute is left at the OS default
    /// (not sent). Only honored on Windows 11 build 22000 and later.
    /// </summary>
    public Color? BorderColor { get; set; }

    /// <summary>
    /// Optional sink invoked when a native attribute call does not succeed. Title bar
    /// theming is best-effort, so failures never throw; they are reported here when set.
    /// </summary>
    public Action<Exception>? OnError { get; set; }
}
