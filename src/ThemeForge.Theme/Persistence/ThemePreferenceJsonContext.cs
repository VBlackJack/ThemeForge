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

using System.Text.Json.Serialization;

namespace ThemeForge.Theme.Persistence;

/// <summary>
/// Source-generated serialization metadata for <see cref="ThemePreference"/>.
/// Avoids runtime reflection (trim- and AOT-friendly) and serializes enums as
/// their names rather than numeric indices.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(ThemePreference))]
internal sealed partial class ThemePreferenceJsonContext : JsonSerializerContext
{
}
