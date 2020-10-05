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

            _logger.Log("Main", "Hello world!");

            
        }
    }
}
