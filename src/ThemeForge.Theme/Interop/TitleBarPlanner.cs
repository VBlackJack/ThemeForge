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

namespace ThemeForge.Theme.Interop;

/// <summary>
/// Pure title bar decision logic: resolve effective colors, derive the dark/light
/// caption mode from the theme background, and select the DWM attributes the running
/// Windows build supports. No window and no native call, so it is fully unit-tested.
/// </summary>
internal static class TitleBarPlanner
{
    /// <summary>Enables the immersive dark caption on build 19041 and later.</summary>
    public const int DwmwaUseImmersiveDarkMode = 20;

    /// <summary>Immersive dark caption identifier on builds 18985 to 19040.</summary>
    public const int DwmwaUseImmersiveDarkModeLegacy = 19;

    /// <summary>Window border color (Windows 11 build 22000 and later).</summary>
    public const int DwmwaBorderColor = 34;

    /// <summary>Caption background color (Windows 11 build 22000 and later).</summary>
    public const int DwmwaCaptionColor = 35;

    /// <summary>Caption text color (Windows 11 build 22000 and later).</summary>
    public const int DwmwaTextColor = 36;

    private const int BuildImmersiveDarkModern = 19041;
    private const int BuildImmersiveDarkLegacy = 18985;
    private const int BuildCaptionColors = 22000;
    private const double DarkLuminanceThreshold = 0.5;

    /// <summary>A single attribute write to apply to a window.</summary>
    public readonly record struct DwmInstruction(int Attribute, int Value);

    /// <summary>
    /// Resolves the effective caption, text, and border colors, applying option
    /// overrides over the theme-derived defaults.
    /// </summary>
    public static (Color Caption, Color Text, Color? Border) ResolveColors(
        Color background, Color foreground, TitleBarOptions options)
    {
        Color caption = options.CaptionColor ?? background;
        Color text = options.TextColor ?? foreground;
        return (caption, text, options.BorderColor);
    }

    /// <summary>Converts a WPF <see cref="Color"/> to a Win32 COLORREF (0x00BBGGRR).</summary>
    public static int ToColorRef(Color color)
        => color.R | (color.G << 8) | (color.B << 16);

    /// <summary>
    /// Builds the ordered attribute writes for the supplied OS build. The dark/light
    /// caption mode is derived from the Oklab lightness of the theme background.
    /// </summary>
    public static IReadOnlyList<DwmInstruction> BuildPlan(
        Color background, Color caption, Color text, Color? border, Version osVersion)
    {
        ArgumentNullException.ThrowIfNull(osVersion);

        List<DwmInstruction> plan = new List<DwmInstruction>();
        int build = osVersion.Build;
        int darkValue = OklabConverter.FromColor(background).L < DarkLuminanceThreshold ? 1 : 0;

        if (build >= BuildImmersiveDarkModern)
        {
            plan.Add(new DwmInstruction(DwmwaUseImmersiveDarkMode, darkValue));
        }
        else if (build >= BuildImmersiveDarkLegacy)
        {
            plan.Add(new DwmInstruction(DwmwaUseImmersiveDarkModeLegacy, darkValue));
        }

        if (build >= BuildCaptionColors)
        {
            plan.Add(new DwmInstruction(DwmwaCaptionColor, ToColorRef(caption)));
            plan.Add(new DwmInstruction(DwmwaTextColor, ToColorRef(text)));
            if (border is Color borderColor)
            {
                plan.Add(new DwmInstruction(DwmwaBorderColor, ToColorRef(borderColor)));
            }
        }

        return plan;
    }
}
