using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
namespace Utili
{
    class Logger
    {
        public bool Initialised { get; private set; }
        public LogSeverity LogSeverity { get; set; }
        private Timer Timer { get; set; }
        
        private StringBuilder Buffer { get; set; }

        public void Initialise()
        // Start the regular save of the log
        {
            if (Initialised)
            {
                throw new Exception("The logger is already initialised.");
            }

            Buffer = new StringBuilder();

            Timer = new Timer(5000);
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();

            Initialised = true;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        // Save the log to the output file, called every 5 seconds
        {
            if (!Initialised)
            {
                throw new Exception("The logger is not initialised.");
            }

            if (!Directory.Exists("Logs")) Directory.CreateDirectory("Logs");
            string logFilename = $"Logs/Log-{DateTime.Now.Year:0000}-{DateTime.Now.Month:00}-{DateTime.Now.Day:00}.txt";

            string output = Buffer.ToString();
            Buffer.Clear();

            File.AppendAllText(logFilename, output);
        }

        public void Log(string module, string message, LogSeverity severity = LogSeverity.Dbug)
        // Outputs relavent log messages to the console and adds them to the buffer
        {
            if (!Initialised)
            {
                throw new Exception("The logger is not initialised.");
            }

            string time = $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}";
            string output = $"{time} | {severity,-4} | {module,-4} | {message}\n";

            Console.Write(output);
            Buffer.Append(output);
        }

        public void ReportError(string module, Exception exception)
        // Returns instantly, in the background creates an error report for the exception
        {
            if (!Initialised)
            {
                throw new Exception("The logger is not initialised.");
            }

            _ = Task.Run(() =>
            {
                if (!Directory.Exists("Errors")) Directory.CreateDirectory("Errors");

                string errorReportFilename = $"Errors/Error-{DateTime.Now.Year:0000}-{DateTime.Now.Month:00}-{DateTime.Now.Day:00} {DateTime.Now.Hour:00}-{DateTime.Now.Minute:00}-{DateTime.Now.Second:00}-{DateTime.Now.Millisecond}.txt";

                Log(module, $"{exception.Message}", LogSeverity.Error);

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
}
