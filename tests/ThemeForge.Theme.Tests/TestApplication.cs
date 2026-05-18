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

namespace ThemeForge.Theme.Tests;

/// <summary>
/// Process-wide WPF <see cref="Application"/> harness for tests that need to
/// instantiate <see cref="ThemeService"/>. WPF only allows one Application per
/// AppDomain, so all tests share the same instance.
/// </summary>
/// <remarks>
/// Access only from <c>[StaFact]</c> tests — Application construction requires
/// the STA threading model.
/// </remarks>
internal static class TestApplication
{
    private static readonly object Lock = new();
    private static Application? _instance;

    public static Application Instance
    {
        get
        {
            lock (Lock)
            {
                _instance ??= Application.Current ?? new Application();
                return _instance;
            }
        }
    }
}
