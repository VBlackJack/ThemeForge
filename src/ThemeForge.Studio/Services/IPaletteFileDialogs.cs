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

namespace ThemeForge.Studio.Services;

/// <summary>Host-owned file selection and discard decisions; no filesystem access in dialogs.</summary>
public interface IPaletteFileDialogs
{
    /// <summary>Returns a selected input path, or null on cancellation.</summary>
    string? Open();
    /// <summary>Returns an output path with native overwrite confirmation.</summary>
    string? Save(string name);
    /// <summary>Returns true only when the user accepts discarding unsaved work.</summary>
    bool ConfirmDiscard();
}
