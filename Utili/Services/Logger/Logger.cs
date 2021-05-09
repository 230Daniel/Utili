using System;
using Microsoft.Extensions.Logging;

namespace Utili.Services
{
    class Logger : ILogger
    {
        string _categoryName;

        public Logger(string categoryCategoryName)
        {
            _categoryName = categoryCategoryName;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if(!IsEnabled(logLevel)) return;

            string exceptionString = exception is null ? "" : $"    {exception.ToString().Replace("\n", "\n    ")}\n";

            LogWriter.Write(
                ($"{DateTime.UtcNow.Hour:00}:{DateTime.UtcNow.Minute:00}:{DateTime.UtcNow.Second:00}  ", ConsoleColor.White),
                ($"{GetShortLogLevel(logLevel),-4}  ", GetLogLevelConsoleColour(logLevel)),
                ($"{_categoryName,-16}  ", GetLogLevelConsoleColour(logLevel)),
                ("»  ", ConsoleColor.Gray),
                ($"{formatter.Invoke(state, exception)}\n", ConsoleColor.White),
                (exceptionString, ConsoleColor.White));

            if(exception is not null && logLevel > LogLevel.Debug)
            {
                LogWriter.CreateErrorReport(exception);
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        string GetShortLogLevel(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "TRCE",
                LogLevel.Debug => "DBUG",
                LogLevel.Information => "INFO",
                LogLevel.Warning => "WARN",
                LogLevel.Error => "FAIL",
                LogLevel.Critical => "CRIT",
                LogLevel.None => "NONE",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
            };
        }

        ConsoleColor GetLogLevelConsoleColour(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => ConsoleColor.Gray,
                LogLevel.Debug => ConsoleColor.White,
                LogLevel.Information => ConsoleColor.Green,
                LogLevel.Warning => ConsoleColor.DarkYellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.DarkRed,
                LogLevel.None => ConsoleColor.Magenta,
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
            };
        }

        public IDisposable BeginScope<TState>(TState state) => default;
    }
}
