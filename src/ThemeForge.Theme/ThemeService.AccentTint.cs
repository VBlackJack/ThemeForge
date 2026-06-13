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
using System.Windows.Media;

namespace ThemeForge.Theme;

public sealed partial class ThemeService
{
    private ResourceDictionary CreateAccentTintDictionary(AccentTint tint)
    {
        string sourceBrushKey = GetSourceBrushKey(tint);
        object? resource = _application.Resources[sourceBrushKey];
        SolidColorBrush sourceBrush = resource as SolidColorBrush
            ?? throw new InvalidOperationException(
                $"Accent tint '{tint}' requires SolidColorBrush resource '{sourceBrushKey}'.");

        Color sourceColor = sourceBrush.Color;
        OklabConverter.Oklab seed = OklabConverter.FromColor(sourceColor);
        Color hoverColor = OklabConverter.ToColor(OklabConverter.Lighten(seed, AccentTintLightDelta));
        Color pressedColor = OklabConverter.ToColor(OklabConverter.Darken(seed, AccentTintLightDelta));

        ResourceDictionary dict = new ResourceDictionary
        {
            [AccentTintMarkerKey] = tint.ToString(),
            ["AccentBrush"] = CreateFrozenBrush(sourceColor),
            ["AccentHoverBrush"] = CreateFrozenBrush(hoverColor),
            ["AccentPressedBrush"] = CreateFrozenBrush(pressedColor),
        };

        return dict;
    }

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        SolidColorBrush brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static string GetSourceBrushKey(AccentTint tint)
        => tint switch
        {
            AccentTint.Blue => "BlueBrush",
            AccentTint.Cyan => "CyanBrush",
            AccentTint.Green => "GreenBrush",
            AccentTint.Orange => "OrangeBrush",
            AccentTint.Pink => "PinkBrush",
            AccentTint.Purple => "PurpleBrush",
            AccentTint.Red => "RedBrush",
            AccentTint.Yellow => "YellowBrush",
            _ => throw new ArgumentOutOfRangeException(nameof(tint), tint, "Unsupported accent tint."),
        };
}
