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

using FluentAssertions;
using Xunit;

namespace ThemeForge.Theme.Tests;

/// <summary>
/// Phase A: contract-level tests that exercise validation and initial state.
/// These do not perform any ResourceDictionary swap and therefore do not need
/// to resolve the theme pack URI — only an Application reference is required.
/// </summary>
public sealed class ThemeServiceContractTests
{
    [Fact]
    public void Ctor_WithNullApplication_Throws()
    {
        Action act = () => new ThemeService(application: null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [StaFact]
    public void Ctor_WithDefaults_ExposesAllCanonicalThemes()
    {
        var service = new ThemeService(TestApplication.Instance);

        service.AvailableThemes.Should().Equal(ThemeNames.All);
    }

    [StaFact]
    public void Ctor_WithInjectedAvailableThemes_PreservesProvidedList()
    {
        string[] custom = new[] { ThemeNames.Dracula, ThemeNames.Drakul };

        var service = new ThemeService(TestApplication.Instance, custom);

        service.AvailableThemes.Should().Equal(custom);
    }

    [StaFact]
    public void InitialState_CurrentThemeEmpty_RevisionZero()
    {
        var service = new ThemeService(TestApplication.Instance);

        service.CurrentTheme.Should().BeEmpty();
        service.ThemeRevision.Should().Be(0);
    }

    [StaFact]
    public void ApplyTheme_WithNull_Throws()
    {
        var service = new ThemeService(TestApplication.Instance);

        Action act = () => service.ApplyTheme(null!);

        act.Should().Throw<ArgumentException>();
    }

    [StaFact]
    public void ApplyTheme_WithEmpty_Throws()
    {
        var service = new ThemeService(TestApplication.Instance);

        Action act = () => service.ApplyTheme(string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [StaFact]
    public void ApplyTheme_WithWhitespace_Throws()
    {
        var service = new ThemeService(TestApplication.Instance);

        Action act = () => service.ApplyTheme("   ");

        act.Should().Throw<ArgumentException>();
    }

    [StaFact]
    public void ApplyTheme_WithUnknownName_ThrowsWithNameInMessage()
    {
        var service = new ThemeService(TestApplication.Instance);

        Action act = () => service.ApplyTheme("NotARealTheme");

        act.Should().Throw<ArgumentException>()
           .WithMessage("*NotARealTheme*");
    }
}
