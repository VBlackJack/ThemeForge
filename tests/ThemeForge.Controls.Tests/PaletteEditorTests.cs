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
using FluentAssertions;
using ThemeForge.Studio.ViewModels;
using ThemeForge.Theme;

namespace ThemeForge.Controls.Tests;

[Collection("Controls application resources")]
public sealed class PaletteEditorTests
{
    [StaFact]
    public void EditSwitchAndReset_RestoresActualPaletteAndPreservesHostResources()
    {
        Application app = TestApplication.Instance;
        using ThemeService service = new ThemeService(app);
        ResourceDictionary resources = app.Resources;
        service.ApplyTheme(ThemeNames.Folio);
        Color folio = ((SolidColorBrush)resources["BackgroundBrush"]).Color;
        service.ApplyTheme(ThemeNames.Dracula);
        object host = new object();
        resources["HostMarker"] = host;
        using PaletteEditorViewModel editor = new PaletteEditorViewModel(service, resources);
        editor.CanonicalSlots.First(slot => slot.Name == "Background").Hex = "#010203";
        service.ApplyTheme(ThemeNames.Folio);
        ((SolidColorBrush)resources["BackgroundBrush"]).Color.Should().Be(folio);
        editor.CanonicalSlots.First(slot => slot.Name == "Background").Hex = "#102030";
        editor.ResetAllCommand.Execute(null);
        ((SolidColorBrush)resources["BackgroundBrush"]).Color.Should().Be(folio);
        resources["HostMarker"].Should().BeSameAs(host);
        resources.Remove("HostMarker");
    }
}
