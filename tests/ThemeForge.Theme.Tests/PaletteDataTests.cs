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

using System.Text;
using System.Text.Json;
using FluentAssertions;
using ThemeForge.Theme.Palettes;

namespace ThemeForge.Theme.Tests;

public sealed class PaletteDataTests
{
    internal static ThemePalette Example() => new ThemePalette
    {
        Name = "TestPalette", DisplayName = "Test Palette", Family = "Light", Attribution = "Original test palette, Apache-2.0.",
        Colors = PaletteSlots.All.ToDictionary(slot => slot, _ => "#123456"),
    };
    [Fact]
    public void RoundTrip_PreservesAllMetadataAndColors()
    {
        ThemePalette source = Example();
        ThemePalette loaded = PaletteJson.Parse(PaletteJson.Serialize(source));
        loaded.Should().BeEquivalentTo(source);
    }
    [Theory]
    [InlineData("#123456", true)] [InlineData("#FFaabbcc", true)]
    [InlineData("#00123456", false)] [InlineData("Red", false)] [InlineData("#12345Z", false)]
    [InlineData("", false)] [InlineData(null, false)]
    public void Colors_AcceptOnlyOpaqueHex(string? input, bool valid)
        => PaletteValidation.TryParseColor(input, out _).Should().Be(valid);
    [Fact]
    public void Normalize_TakesAnImmutableSnapshot()
    {
        ThemePalette source = Example();
        ThemePalette snapshot = PaletteValidation.Normalize(source);
        ((Dictionary<string, string>)source.Colors)["Accent"] = "#FFFFFF";
        snapshot.Colors["Accent"].Should().Be("#123456");
    }
    [Theory]
    [InlineData("../Other")] [InlineData("1Theme")] [InlineData("With Spaces")] [InlineData("Thème")]
    public void InvalidIdentifiers_AreRejected(string name)
    {
        Action validate = () => PaletteValidation.Normalize(Example() with { Name = name });
        validate.Should().Throw<ArgumentException>();
    }
    [Fact]
    public void UnsupportedVersionMissingAndUnknownSlots_AreRejected()
    {
        Action version = () => PaletteValidation.Normalize(Example() with { Version = 99 });
        version.Should().Throw<ArgumentException>();
        Dictionary<string, string> colors = new Dictionary<string, string>(Example().Colors);
        colors.Remove("Accent"); colors["Unknown"] = "#123456";
        Action slots = () => PaletteValidation.Normalize(Example() with { Colors = colors });
        slots.Should().Throw<ArgumentException>();
    }
    [Fact]
    public void DuplicatePropertiesAndUnknownMembers_AreRejected()
    {
        string json = Encoding.UTF8.GetString(PaletteJson.Serialize(Example()));
        Action duplicate = () => PaletteJson.Parse(Encoding.UTF8.GetBytes(json.Replace("\"version\": 1", "\"version\": 1, \"version\": 1")));
        duplicate.Should().Throw<ArgumentException>();
        Action unknown = () => PaletteJson.Parse(Encoding.UTF8.GetBytes(json.Replace("\"version\": 1", "\"run\": \"anything\", \"version\": 1")));
        unknown.Should().Throw<JsonException>();
        Action large = () => PaletteJson.Parse(new byte[PaletteValidation.MaximumFileBytes + 1]);
        large.Should().Throw<ArgumentException>();
    }
    [Fact]
    public async Task Save_InvalidOrCancelledDocument_PreservesPreviousBytes()
    {
        string path = System.IO.Path.GetTempFileName();
        try
        {
            await PaletteJson.SaveAsync(path, Example(), TestContext.Current.CancellationToken);
            byte[] before = await System.IO.File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken);
            Func<Task> invalid = () => PaletteJson.SaveAsync(path, Example() with { Version = 99 });
            await invalid.Should().ThrowAsync<ArgumentException>();
            Func<Task> cancelled = () => PaletteJson.SaveAsync(path, Example(), new CancellationToken(true));
            await cancelled.Should().ThrowAsync<OperationCanceledException>();
            (await System.IO.File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken)).Should().Equal(before);
            (await PaletteJson.LoadAsync(path, TestContext.Current.CancellationToken)).Should().BeEquivalentTo(Example());
        }
        finally { System.IO.File.Delete(path); }
    }
}
