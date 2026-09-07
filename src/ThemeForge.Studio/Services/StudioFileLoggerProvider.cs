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

using System.Diagnostics;
using System.IO;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace ThemeForge.Studio.Services;

/// <summary>Serializes asynchronous daily file writes and drains before normal application shutdown.</summary>
public sealed class StudioFileLoggerProvider : ILoggerProvider, IAsyncDisposable
{
    private readonly StudioLogOptions _options;
    private readonly Channel<(DateTime Time, string Text)> _queue = Channel.CreateUnbounded<(DateTime, string)>();
    private readonly Task _writer;
    /// <summary>Starts the asynchronous writer; no UI thread performs file writes.</summary>
    public StudioFileLoggerProvider(StudioLogOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _writer = Task.Run(WriteAsync);
    }
    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName) => new EntryLogger(this, categoryName);
    /// <inheritdoc/>
    public void Dispose() => _queue.Writer.TryComplete();
    /// <summary>Completes pending writes without blocking the dispatcher.</summary>
    public async ValueTask DisposeAsync() { Dispose(); await _writer.ConfigureAwait(false); }
    private async Task WriteAsync()
    {
        await foreach ((DateTime time, string text) in _queue.Reader.ReadAllAsync().ConfigureAwait(false))
        {
            try
            {
                Directory.CreateDirectory(_options.DirectoryPath);
                string path = Path.Combine(_options.DirectoryPath, $"ThemeForge.Studio_{time:yyyyMMdd}.log");
                await File.AppendAllTextAsync(path, text + Environment.NewLine).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
            { Debug.WriteLine($"File logging unavailable: {exception.Message}"); }
        }
    }
    private sealed class EntryLogger(StudioFileLoggerProvider owner, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => owner._options.Enabled && logLevel >= owner._options.MinimumLevel && logLevel != LogLevel.None;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) { return; }
            DateTime time = DateTime.Now;
            string message = $"[{time:yyyy-MM-dd HH:mm:ss}] [{logLevel.ToString().ToUpperInvariant()}] {category}: {formatter(state, exception)}";
            if (exception is not null) { message += " " + exception; }
            Debug.WriteLine(message); owner._queue.Writer.TryWrite((time, message));
        }
    }
}
