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

namespace ThemeForge.Controls.Theming;

/// <summary>Inheritable motion opt-out, combined with the live Windows animation preference.</summary>
public static class Motion
{
    /// <summary>Allows a host to disable animations for an entire visual subtree.</summary>
    public static readonly DependencyProperty ReduceMotionProperty = DependencyProperty.RegisterAttached(
        "ReduceMotion", typeof(bool), typeof(Motion), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>Stores the dynamic Windows resource used by the control templates.</summary>
    public static readonly DependencyProperty SystemAnimationsEnabledProperty = DependencyProperty.RegisterAttached(
        "SystemAnimationsEnabled", typeof(bool), typeof(Motion), new FrameworkPropertyMetadata(false));

    /// <summary>Gets the host's motion opt-out.</summary>
    public static bool GetReduceMotion(DependencyObject element) => (bool)element.GetValue(ReduceMotionProperty);
    /// <summary>Sets the host's motion opt-out.</summary>
    public static void SetReduceMotion(DependencyObject element, bool value) => element.SetValue(ReduceMotionProperty, value);
    /// <summary>Gets the current template animation preference.</summary>
    public static bool GetSystemAnimationsEnabled(DependencyObject element) => (bool)element.GetValue(SystemAnimationsEnabledProperty);
    /// <summary>Sets the current template animation preference.</summary>
    public static void SetSystemAnimationsEnabled(DependencyObject element, bool value) => element.SetValue(SystemAnimationsEnabledProperty, value);
}
