using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Ae.Synthetics.Runner.Internal
{
    internal sealed class SyntheticTestLogger : ILogger
    {
        private readonly Func<DateTimeOffset, LogLevel, EventId, string, string> _logFormatter;

        public SyntheticTestLogger(Func<DateTimeOffset, LogLevel, EventId, string, string> logFormatter) => _logFormatter = logFormatter;

        public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();

        public bool IsEnabled(LogLevel logLevel) => throw new NotImplementedException();

        public List<string> Entries { get; } = new List<string>();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) => Entries.Add(_logFormatter(DateTimeOffset.UtcNow, logLevel, eventId, formatter(state, exception)));
    }
}
