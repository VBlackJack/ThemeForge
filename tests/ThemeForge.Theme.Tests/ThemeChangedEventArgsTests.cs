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

public sealed class ThemeChangedEventArgsTests
{
    [Fact]
    public void Ctor_WithValidArgs_StoresAllProperties()
    {
        var args = new ThemeChangedEventArgs("Dracula", "Drakul", 2);

        args.PreviousTheme.Should().Be("Dracula");
        args.CurrentTheme.Should().Be("Drakul");
        args.Revision.Should().Be(2);
    }

    [Fact]
    public void Ctor_WithEmptyPrevious_IsAccepted()
    {
        // First apply: previous is the empty initial state.
        var args = new ThemeChangedEventArgs(string.Empty, "Dracula", 1);

        args.PreviousTheme.Should().BeEmpty();
        args.CurrentTheme.Should().Be("Dracula");
        args.Revision.Should().Be(1);
    }

    [Fact]
    public void Ctor_WithNullCurrent_Throws()
    {
        var act = () => new ThemeChangedEventArgs("Dracula", null!, 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Ctor_WithEmptyCurrent_Throws()
    {
        var act = () => new ThemeChangedEventArgs("Dracula", string.Empty, 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Ctor_WithWhitespaceCurrent_Throws()
    {
        var act = () => new ThemeChangedEventArgs("Dracula", "   ", 1);

        act.Should().Throw<ArgumentException>();
    }
}
