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
        
        private StringBuilder Buffer { get; set; }

        public void Initialise()
        // Start the regular save of the log
        {
            Buffer = new StringBuilder();

            Timer = new Timer(5000);
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        // Save the log to the output file, called every 5 seconds
        {
            if (!Directory.Exists("Logs")) Directory.CreateDirectory("Logs");
            string logFilename = $"Logs/Log-{DateTime.Now.Year:0000}-{DateTime.Now.Month:00}-{DateTime.Now.Day:00}.txt";

            string output = Buffer.ToString();
            Buffer.Clear();

            File.AppendAllText(logFilename, output);
        }

        public void Log(string module, string message, LogSeverity severity = LogSeverity.Dbug)
        // Outputs relavent log messages to the console and adds them to the buffer
        {
            string time = $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}";
            string output = $"{time}  {severity,-4}  {module,-10}  {message}\n";

            Console.Write(output);
            Buffer.Append(output);
        }

        public void LogEmpty(bool fileOnly = false)
        {
            if(!fileOnly) Console.Write("\n");
            Buffer.Append("\n");
        }

        public void ReportError(string module, Exception exception, LogSeverity severity = LogSeverity.Errr)
        // Returns instantly, in the background creates an error report for the exception
        {
            _ = Task.Run(() =>
            {
                if (!Directory.Exists("Errors")) Directory.CreateDirectory("Errors");

                string errorReportFilename = $"Errors/Error-{DateTime.Now.Year:0000}-{DateTime.Now.Month:00}-{DateTime.Now.Day:00} {DateTime.Now.Hour:00}-{DateTime.Now.Minute:00}-{DateTime.Now.Second:00}-{DateTime.Now.Millisecond}.txt";

                Log(module, $"{exception.Message}", severity);

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
    }

    public enum LogSeverity
    {
        Dbug,
        Info,
        Warn,
        Errr,
        Crit
    }
}
