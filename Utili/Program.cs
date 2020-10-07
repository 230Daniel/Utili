using Database.Data;
using System;
using System.Linq;

namespace Utili
{
    class Program
    {
        
        // ReSharper disable InconsistentNaming

        public static Logger _logger;

        // ReSharper enable InconsistentNaming

        private static void Main()
        {
            _logger = new Logger
            {
                LogSeverity = LogSeverity.Dbug
            };
            _logger.Initialise();

            _logger.Log("Main", "Connecting to the database...");

            // Initialise the database and use cache
            Database.Database.Initialise(true);

            _logger.Log("Main", "Connected to the database");

            var channels = Autopurge.GetRowsWhere();
            _logger.Log("Main", $"There are {channels.Count} autopurge channels.");

            AutopurgeRow row = Autopurge.GetRowsWhere(0).First();

            row.Messages = 10;

            Autopurge.SaveRow(row);
        }
    }
}
