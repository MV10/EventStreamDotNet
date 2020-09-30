
using Microsoft.Extensions.Logging;
using System;

namespace EventStreamDotNet
{
    /// <summary>
    /// Standard ILogger implementation that is null-safe (can be called even if client application
    /// does not provide an ILoggerFactory in the library configuration).
    /// </summary>
    internal class DebugLogger<T> : ILogger
    {
        private readonly ILogger<T> logger;

        public bool Available => logger != null;

        public DebugLogger(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory?.CreateLogger<T>();
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state)
            => logger?.BeginScope(state);

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
            => logger?.IsEnabled(logLevel) ?? false;

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => logger?.Log(logLevel, eventId, state, exception, formatter);
    }
}
