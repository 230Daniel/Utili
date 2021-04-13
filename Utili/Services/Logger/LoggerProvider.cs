using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Utili.Services
{
    public sealed class LoggerProvider : ILoggerProvider
    {
        readonly ConcurrentDictionary<string, Logger> _loggers = new ConcurrentDictionary<string, Logger>();

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new Logger(categoryName.Split(".").Last().Replace("Default", "").Replace("Discord", "")));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
