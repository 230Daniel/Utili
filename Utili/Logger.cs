using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Utili
{
    internal class Logger
    {
        public LogSeverity LogSeverity { get; set; }
        private Timer Timer { get; set; }
        private StringBuilder Buffer { get; set; } = new StringBuilder();

        public Logger(LogSeverity logSeverity)
        {
            LogSeverity = logSeverity;
            Initialise();
        }

        private void Initialise()
        {
            Buffer = new StringBuilder();

            Timer = new Timer(5000);
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();

            if (!Directory.Exists("Logs")) Directory.CreateDirectory("Logs");

            Title();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string output = Buffer.ToString();
            Buffer.Clear();

            File.AppendAllText($"Logs/{DateTime.Now:yyyy-MM-dd}.txt", output);
        }

        public void Log(string module, string message, LogSeverity severity = LogSeverity.Dbug)
        {
            string time = $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}";
            string output = $"{time}  {severity,-4}  {module,-10}  {message}";

            LogRaw(output, severity);
        }

        public void LogEmpty(bool fileOnly = false)
        {
            if(!fileOnly) Console.Write('\n');
            Buffer.Append('\n');
        }

        private void LogRaw(string message, LogSeverity logSeverity)
        {
            WriteToConsole(message, logSeverity);
            Buffer.Append(message + "\n");
        }

        public void ReportError(string module, Exception exception, LogSeverity severity = LogSeverity.Errr)
        {
            if (severity == LogSeverity.Crit) Log(module, $"{exception.Message}\n{exception.StackTrace}", severity);
            else Log(module, $"{exception.Message}", severity);

            _ = Task.Run(() =>
            {
                if (!Directory.Exists("Errors")) Directory.CreateDirectory("Errors");

                string errorReportFilename = $"Errors/Error-{DateTime.Now.Year:0000}-{DateTime.Now.Month:00}-{DateTime.Now.Day:00} {DateTime.Now.Hour:00}-{DateTime.Now.Minute:00}-{DateTime.Now.Second:00}-{DateTime.Now.Millisecond}.txt";

                StreamWriter errorReport = File.CreateText(errorReportFilename);

                errorReport.WriteLine($"Error report for error at time: {DateTime.Now}\n");

                int errorNumber = 0;
                while (exception != null)
                {
                    errorReport.WriteLine($"Error {errorNumber}:\n{exception.Message}\n{exception.StackTrace}\n");
                    exception = exception.InnerException;
                    errorNumber += 1;
                }

                errorReport.Close();
            });
        }

        private void Title()
        {
            string title = @"

                               //
                           ////
                       //////
                  ////////
              //////////
          ////////////
      //////////////////////                     Utili v2
  /////////////////////////////////           Daniel Baynton
         //////////////////////
               ///////////
             /////////
          ////////
        //////
      ////
    //

";

            LogRaw(title, LogSeverity.Info);
        }

        private static object _messageLock= new object();
        private void WriteToConsole(string message, LogSeverity logSeverity)
        {
            ConsoleColor colour = logSeverity switch
            {
                LogSeverity.Dbug => ConsoleColor.Gray,
                LogSeverity.Info => ConsoleColor.Gray,
                LogSeverity.Warn => ConsoleColor.DarkYellow,
                LogSeverity.Errr => ConsoleColor.Red,
                LogSeverity.Crit => ConsoleColor.DarkRed,
                _ => ConsoleColor.Gray
            };

            lock (_messageLock)
            {
                Console.ForegroundColor = colour;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }
    }

    public enum LogSeverity
    {
        Title,
        Dbug,
        Info,
        Warn,
        Errr,
        Crit
    }
}
