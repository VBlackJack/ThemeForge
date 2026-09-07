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

namespace ThemeForge.Quality;

/// <summary>Repeatable workload and budgets, loaded from the caller's JSON configuration.</summary>
public sealed record QualityOptions
{
    public int Iterations { get; init; }
    public int WindowCycles { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
    public string[] Themes { get; init; } = Array.Empty<string>();
    public double[] Scales { get; init; } = Array.Empty<double>();
    public string SampleText { get; init; } = string.Empty;
    public string ActionText { get; init; } = string.Empty;
    public string DisabledText { get; init; } = string.Empty;
    public string ErrorText { get; init; } = string.Empty;
    public string EditedPaletteName { get; init; } = string.Empty;
    public string EditedBackground { get; init; } = string.Empty;
    public double MaximumSwitchP95Ms { get; init; }
    public double MaximumFirstGalleryMs { get; init; }
    public long MaximumAllocatedBytesPerSwitch { get; init; }
    public long MaximumRetainedBytes { get; init; }
    public void Validate()
    {
        if (Iterations < 10 || Iterations > 10000 || WindowCycles < 1 || WindowCycles > 1000 ||
            Width < 640 || Width > 4096 || Height < 480 || Height > 4096 || Themes.Length < 2 ||
            Scales.Length == 0 || Scales.Any(scale => scale < 1 || scale > 3) || string.IsNullOrWhiteSpace(SampleText))
        { throw new ArgumentException("Invalid quality workload configuration."); }
    }
}
