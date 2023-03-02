using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deribit.ApiClient.Tests.Integration
{
    internal class TestDebugLogger<T> : ILogger<T>
    {
        public LogLevel LogLevel { get; set; }
        private static readonly string TypeName = typeof(T).Name;

        public TestDebugLogger(LogLevel logLevel)
        {
            LogLevel = logLevel;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= this.LogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            string? error = (exception == null || formatter == null)
                ? null
                : formatter(state, exception);
            string eventIdString = eventId == 0 ? string.Empty : $"{eventId} ";

            if (error != null)
                Debug.WriteLine($"{DateTime.UtcNow} {logLevel} {TypeName}: {eventIdString}{state}");
            else
                Debug.WriteLine($"{DateTime.UtcNow} {logLevel} {TypeName}: {eventIdString}{state}{Environment.NewLine}{error}");
        }
    }
}
