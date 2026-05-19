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

using System.Windows.Input;
using FluentAssertions;
using ThemeForge.Controls.Composites;
using Xunit;

namespace ThemeForge.Controls.Tests;

/// <summary>
/// Behavioral tests for the <see cref="Dialog"/> control. AutomationPeer
/// invariants live in <see cref="DialogAutomationPeerTests"/>.
/// </summary>
public sealed class DialogTests
{
    [StaFact]
    public void Close_RaisesClosedEventOnce()
    {
        _ = TestApplication.Instance;
        Dialog dialog = new Dialog();
        int closedCount = 0;
        dialog.Closed += (_, _) => closedCount++;

        dialog.Close();

        closedCount.Should().Be(1);
    }

    [StaFact]
    public void Close_CalledTwice_RaisesEventOnce()
    {
        // Single-shot contract: Close raises the event at most once even
        // when called repeatedly. The host still owns visual teardown; a
        // second Close on an already-closed dialog remains safe.
        _ = TestApplication.Instance;
        Dialog dialog = new Dialog();
        int count = 0;
        dialog.Closed += (_, _) => count++;

        dialog.Close();
        dialog.Close();

        count.Should().Be(1);
    }

    [StaFact]
    public void Close_WithNoSubscriber_DoesNotThrow()
    {
        _ = TestApplication.Instance;
        Dialog dialog = new Dialog();

        Action act = dialog.Close;

        act.Should().NotThrow();
    }

    [StaFact]
    public void Close_WithCanExecuteTrue_InvokesCloseCommand()
    {
        _ = TestApplication.Instance;
        bool executed = false;
        TestCommand command = new TestCommand(() => executed = true);
        Dialog dialog = new Dialog { CloseCommand = command };

        dialog.Close();

        command.ExecuteCount.Should().Be(1);
        executed.Should().BeTrue();
    }

    [StaFact]
    public void Close_WithCanExecuteFalse_DoesNotInvokeCloseCommand()
    {
        _ = TestApplication.Instance;
        int closedCount = 0;
        TestCommand command = new TestCommand(static () => { }, static () => false);
        Dialog dialog = new Dialog { CloseCommand = command };
        dialog.Closed += (_, _) => closedCount++;

        dialog.Close();

        command.ExecuteCount.Should().Be(0);
        closedCount.Should().Be(1);
    }

    private sealed class TestCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public TestCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute ?? (static () => true);
        }

        public int ExecuteCount { get; private set; }

        public bool CanExecute(object? parameter) => _canExecute();

        public void Execute(object? parameter)
        {
            ExecuteCount++;
            _execute();
        }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
