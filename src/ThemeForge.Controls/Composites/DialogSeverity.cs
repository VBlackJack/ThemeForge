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

namespace ThemeForge.Controls.Composites;

/// <summary>
/// Severity signal carried by a <see cref="Dialog"/> accent bar.
/// </summary>
public enum DialogSeverity
{
    /// <summary>Neutral dialog with no semantic accent.</summary>
    Default,
    /// <summary>Informational signal — maps to InfoBrush.</summary>
    Info,
    /// <summary>Positive / completed signal — maps to SuccessBrush.</summary>
    Success,
    /// <summary>Caution signal — maps to WarningBrush.</summary>
    Warning,
    /// <summary>Failure / destructive signal — maps to ErrorBrush.</summary>
    Error,
}
