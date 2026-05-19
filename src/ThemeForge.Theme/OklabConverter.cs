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

/// <summary>
/// sRGB to Oklab conversion helpers using the Ottosson 2020 matrices.
/// </summary>
internal static class OklabConverter
{
    public readonly record struct Oklab(double L, double A, double B);

    public static Oklab FromColor(Color color)
    {
        double r = SrgbToLinear(color.R / 255.0);
        double g = SrgbToLinear(color.G / 255.0);
        double b = SrgbToLinear(color.B / 255.0);

        double l = (0.4122214708 * r) + (0.5363325363 * g) + (0.0514459929 * b);
        double m = (0.2119034982 * r) + (0.6806995451 * g) + (0.1073969566 * b);
        double s = (0.0883024619 * r) + (0.2817188376 * g) + (0.6299787005 * b);

        double lp = Math.Cbrt(l);
        double mp = Math.Cbrt(m);
        double sp = Math.Cbrt(s);

        double labL = (0.2104542553 * lp) + (0.7936177850 * mp) - (0.0040720468 * sp);
        double labA = (1.9779984951 * lp) - (2.4285922050 * mp) + (0.4505937099 * sp);
        double labB = (0.0259040371 * lp) + (0.7827717662 * mp) - (0.8086757660 * sp);

        return new Oklab(labL, labA, labB);
    }

    public static Color ToColor(Oklab lab)
    {
        double lp = lab.L + (0.3963377774 * lab.A) + (0.2158037573 * lab.B);
        double mp = lab.L - (0.1055613458 * lab.A) - (0.0638541728 * lab.B);
        double sp = lab.L - (0.0894841775 * lab.A) - (1.2914855480 * lab.B);

        double l = lp * lp * lp;
        double m = mp * mp * mp;
        double s = sp * sp * sp;

        double r = (4.0767416621 * l) - (3.3077115913 * m) + (0.2309699292 * s);
        double g = (-1.2684380046 * l) + (2.6097574011 * m) - (0.3413193965 * s);
        double b = (-0.0041960863 * l) - (0.7034186147 * m) + (1.7076147010 * s);

        return Color.FromRgb(
            ClampToByte(LinearToSrgb(r)),
            ClampToByte(LinearToSrgb(g)),
            ClampToByte(LinearToSrgb(b)));
    }

    public static Oklab Lighten(Oklab lab, double delta)
        => new Oklab(Math.Clamp(lab.L + delta, 0.0, 1.0), lab.A, lab.B);

    public static Oklab Darken(Oklab lab, double delta)
        => new Oklab(Math.Clamp(lab.L - delta, 0.0, 1.0), lab.A, lab.B);

    private static double SrgbToLinear(double channel)
        => channel <= 0.04045
            ? channel / 12.92
            : Math.Pow((channel + 0.055) / 1.055, 2.4);

    private static double LinearToSrgb(double channel)
    {
        if (double.IsNaN(channel))
        {
            return 0.0;
        }

        double clamped = Math.Clamp(channel, 0.0, 1.0);
        return clamped <= 0.0031308
            ? clamped * 12.92
            : (1.055 * Math.Pow(clamped, 1.0 / 2.4)) - 0.055;
    }

    private static byte ClampToByte(double srgb)
        => (byte)Math.Clamp(Math.Round(srgb * 255.0), 0.0, 255.0);
}
