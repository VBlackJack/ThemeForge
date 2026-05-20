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
/// Orthogonal accent recolour applied on top of the active theme.
/// </summary>
/// <remarks>
/// Picks one of the canonical Dracula accent slots present in every variant
/// and re-wires the accent brush triad at runtime without touching surface,
/// foreground, comment, border, or semantic brushes.
/// </remarks>
public enum AccentTint
{
    /// <summary>Use the theme's native accent brush without an override.</summary>
    Default,

    /// <summary>Use the extended blue accent slot.</summary>
    Blue,

    /// <summary>Use the canonical cyan accent slot.</summary>
    Cyan,

    /// <summary>Use the canonical green accent slot.</summary>
    Green,

    /// <summary>Use the canonical orange accent slot.</summary>
    Orange,

    /// <summary>Use the canonical pink accent slot.</summary>
    Pink,

    /// <summary>Use the canonical purple accent slot.</summary>
    Purple,

    /// <summary>Use the canonical red accent slot.</summary>
    Red,

    /// <summary>Use the canonical yellow accent slot.</summary>
    Yellow,
}
