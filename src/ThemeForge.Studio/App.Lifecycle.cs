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

using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ThemeForge.Studio.Services;
using ThemeForge.Studio.ViewModels;

namespace ThemeForge.Studio;

public partial class App
{
    private StudioFileLoggerProvider? _fileLogger;
    private bool _closing;
    private bool _shutdownReady;
    private async void OnWindowClosing(object? sender, CancelEventArgs args)
    {
        if (_shutdownReady) { return; }
        args.Cancel = true;
        if (_closing || _services?.GetRequiredService<PaletteEditorViewModel>().ConfirmClose() != true) { return; }
        _closing = true;
        if (sender is Window window)
        {
            window.IsEnabled = false;
            if (_fileLogger is not null) { await _fileLogger.DisposeAsync(); }
            _shutdownReady = true; window.Close();
        }
    }
}
