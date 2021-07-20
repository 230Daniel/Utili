using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace UtiliBackend.Services
{
    public sealed class LoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, Logger> _loggers = new();

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, _ => new Logger(categoryName
                .Split(".").Last()
                .Replace("Default", "")
                .Replace("Discord", "")
                .Replace("Service", "")
                .Replace("My", "")));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
