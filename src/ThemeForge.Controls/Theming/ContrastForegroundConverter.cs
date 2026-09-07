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

using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ThemeForge.Controls.Theming;

/// <summary>Keeps the requested text brush when legible; otherwise selects a supplied contrast fallback.</summary>
public sealed class ContrastForegroundConverter : IMultiValueConverter
{
    private const double MinimumContrast = 4.5;

    /// <inheritdoc/>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 4 || values[0] is not SolidColorBrush foreground ||
            values[1] is not SolidColorBrush background || values[2] is not SolidColorBrush dark ||
            values[3] is not SolidColorBrush light)
        {
            return DependencyProperty.UnsetValue;
        }
        if (Contrast(foreground.Color, background.Color) >= MinimumContrast) { return foreground; }
        return Contrast(dark.Color, background.Color) >= Contrast(light.Color, background.Color) ? dark : light;
    }

    /// <inheritdoc/>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException("Contrast foreground binding is one-way.");

    private static double Contrast(Color a, Color b)
    {
        double x = Luminance(a), y = Luminance(b);
        return (Math.Max(x, y) + 0.05) / (Math.Min(x, y) + 0.05);
    }

    private static double Luminance(Color color)
        => 0.2126 * Linear(color.R) + 0.7152 * Linear(color.G) + 0.0722 * Linear(color.B);

    private static double Linear(byte channel)
    {
        double value = channel / 255.0;
        return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
    }
}
