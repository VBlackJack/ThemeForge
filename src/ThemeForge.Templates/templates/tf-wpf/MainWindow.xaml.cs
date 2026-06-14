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
using ThemeForge.Theme;

namespace ThemeForge.WpfApp;

/// <summary>
/// Main window. Code-behind is limited to window-level wiring: setting the injected
/// view model and theming the title bar. No business logic.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel, IThemeService themeService)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Themes the OS title bar from the current theme; re-syncs on theme changes
        // and disposes itself when the window closes.
        this.ApplyThemeForgeTitleBar(themeService);
    }
}
