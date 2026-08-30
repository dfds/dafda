using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Dafda.Tests.TestDoubles
{
    internal class LoggerSpy<T> : ILogger<T>
    {
        public IList<LogEntry> LogEntries { get; } = new List<LogEntry>();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            LogEntries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        internal class LogEntry
        {
            public LogEntry(LogLevel logLevel, string message, Exception exception)
            {
                LogLevel = logLevel;
                Message = message;
                Exception = exception;
            }

            public LogLevel LogLevel { get; }
            public string Message { get; }
            public Exception Exception { get; }
        }

        private class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new NullScope();

            public void Dispose()
            {
            }
        }
    }
}
