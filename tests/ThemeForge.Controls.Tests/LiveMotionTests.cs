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
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using FluentAssertions;
using ThemeForge.Controls.Theming;

namespace ThemeForge.Controls.Tests;

public sealed class LiveMotionTests
{
    [StaFact]
    public void WindowsResourceChange_StopsAndResumesIndeterminateStoryboard()
    {
        ResourceDictionary styles = TemplateRegressionTests.Load("Styles/ProgressBarStyle.xaml");
        ProgressBar progress = new ProgressBar { Style = (Style)styles[typeof(ProgressBar)], IsIndeterminate = true };
        progress.Resources[SystemParameters.ClientAreaAnimationKey] = true;
        WindowTestHost.Render(progress, _ =>
        {
            FrameworkElement segment = (FrameworkElement)progress.Template.FindName("IndeterminateBar", progress);
            Tick();
            segment.RenderTransform.HasAnimatedProperties.Should().BeTrue();
            progress.Resources[SystemParameters.ClientAreaAnimationKey] = false;
            Motion.GetSystemAnimationsEnabled(progress).Should().BeFalse();
            Tick();
            segment.RenderTransform.HasAnimatedProperties.Should().BeFalse();
            ((TranslateTransform)segment.RenderTransform).X.Should().Be(0);
            progress.Resources[SystemParameters.ClientAreaAnimationKey] = true;
            Tick();
            segment.RenderTransform.HasAnimatedProperties.Should().BeTrue();
            Motion.SetReduceMotion(progress, true);
            Tick();
            segment.RenderTransform.HasAnimatedProperties.Should().BeFalse();
        });
    }

    private static void Tick()
    {
        DispatcherFrame frame = new DispatcherFrame();
        DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(50) };
        timer.Tick += (_, _) => { timer.Stop(); frame.Continue = false; };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }
}
