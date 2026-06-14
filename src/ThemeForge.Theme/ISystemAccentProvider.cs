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

/// <summary>OS-level accent color signal. Abstracted so the engine stays testable.</summary>
public interface ISystemAccentProvider
{
    /// <summary>Gets the current operating system accent color, or null when unavailable.</summary>
    Color? GetCurrentAccent();

    /// <summary>Raised when the operating system accent color may have changed.</summary>
    event EventHandler? Changed;
}
