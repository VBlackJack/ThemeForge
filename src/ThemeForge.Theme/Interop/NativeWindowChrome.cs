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

using System.Runtime.InteropServices;

namespace ThemeForge.Theme.Interop;

/// <summary>
/// Real <see cref="INativeWindowChrome"/> backed by <c>dwmapi.dll</c> through a
/// source-generated P/Invoke.
/// </summary>
internal sealed partial class NativeWindowChrome : INativeWindowChrome
{
    private const int AttributeValueSize = sizeof(int);

    /// <inheritdoc/>
    public bool TrySetAttribute(IntPtr handle, int attribute, int value)
    {
        int result = DwmSetWindowAttribute(handle, attribute, ref value, AttributeValueSize);
        return result >= 0;
    }

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetWindowAttribute(
        IntPtr hwnd, int attribute, ref int attributeValue, int attributeSize);
}
