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
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using FluentAssertions;
using ThemeForge.Controls.Theming;
using ThemeForge.Theme;

namespace ThemeForge.Controls.Tests;

[Collection("Controls application resources")]
public sealed class ButtonContrastTests
{
    [StaFact]
    public void AllPalettesAndTints_KeepRenderedButtonTextAboveMinimumContrast()
    {
        Application app = TestApplication.Instance;
        using ThemeService service = new ThemeService(app);
        ResourceDictionary styles = TemplateRegressionTests.Load("Styles/ButtonStyle.xaml");
        Button button = new Button { Content = "Action", Style = (Style)styles[typeof(Button)] };
        WindowTestHost.Render(button, _ =>
        {
            Border border = (Border)button.Template.FindName("border", button);
            ContentPresenter presenter = TemplateRegressionTests.Descendants(button).OfType<ContentPresenter>().First();
            foreach (string theme in service.AvailableThemes)
            {
                service.ApplyTheme(theme);
                foreach (AccentTint tint in Enum.GetValues<AccentTint>())
                {
                    service.ApplyAccentTint(tint);
                    button.Foreground = (Brush)app.Resources["BackgroundBrush"];
                    foreach (string key in new[] { "AccentBrush", "AccentHoverBrush", "AccentPressedBrush" })
                    {
                        border.Background = (Brush)app.Resources[key];
                        button.UpdateLayout();
                        SolidColorBrush foreground = (SolidColorBrush)TextElement.GetForeground(presenter);
                        double contrast = Ratio(foreground.Color, ((SolidColorBrush)border.Background).Color);
                        contrast.Should().BeGreaterThanOrEqualTo(4.5, $"{theme}/{tint}/{key}");
                    }
                }
            }
        });
    }

    [Fact]
    public void Converter_PreservesAnAlreadyAccessibleForeground()
    {
        ContrastForegroundConverter converter = new ContrastForegroundConverter();
        object result = converter.Convert(new object[] { Brushes.Yellow, Brushes.Black, Brushes.Black, Brushes.White },
            typeof(Brush), null!, CultureInfo.InvariantCulture);
        result.Should().BeSameAs(Brushes.Yellow);
    }

    private static double Ratio(Color left, Color right)
    {
        double first = Luminance(left);
        double second = Luminance(right);
        return (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
    }

    private static double Luminance(Color color)
    {
        static double Linear(byte component)
        {
            double value = component / 255.0;
            return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }
        return 0.2126 * Linear(color.R) + 0.7152 * Linear(color.G) + 0.0722 * Linear(color.B);
    }
}
