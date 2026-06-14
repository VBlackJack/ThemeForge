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

namespace ThemeForge.Theme.Interop;

/// <summary>
/// Seam over the native window-attribute call. Isolates the P/Invoke so the title bar
/// logic can be unit-tested without a real window or operating system call.
/// </summary>
internal interface INativeWindowChrome
{
    /// <summary>
    /// Sets a single DWM window attribute. Returns <see langword="true"/> on a
    /// successful HRESULT and <see langword="false"/> otherwise. Never throws.
    /// </summary>
    /// <param name="handle">The native window handle.</param>
    /// <param name="attribute">The DWMWINDOWATTRIBUTE identifier.</param>
    /// <param name="value">The attribute value (a BOOL or a COLORREF).</param>
    /// <returns>Whether the call succeeded.</returns>
    bool TrySetAttribute(IntPtr handle, int attribute, int value);
}
