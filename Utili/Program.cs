using System;

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

            Database.Main.Initialise();

            _logger.Log("Main", "Connected to the database");

            var channels = Database.Types.Autopurge.GetRowsWhere();
            _logger.Log("Main", $"There are {channels.Count} autopurge channels.");
        }
    }
}
