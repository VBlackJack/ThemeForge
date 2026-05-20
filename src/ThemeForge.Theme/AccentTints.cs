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
/// Canonical list of supported accent tints.
/// </summary>
public static class AccentTints
{
    /// <summary>Every supported tint in stable display order.</summary>
    public static IReadOnlyList<AccentTint> All { get; } = new[]
    {
        AccentTint.Default,
        AccentTint.Cyan,
        AccentTint.Green,
        AccentTint.Orange,
        AccentTint.Pink,
        AccentTint.Purple,
        AccentTint.Red,
        AccentTint.Yellow,
    };
}
