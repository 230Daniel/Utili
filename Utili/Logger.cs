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
        private string Filename { get; set;}
        private StringBuilder Buffer { get; set; } = new StringBuilder();

        public void Initialise()
        {
            Buffer = new StringBuilder();

            Timer = new Timer(5000);
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();

            if (!Directory.Exists("Logs")) Directory.CreateDirectory("Logs");

            int highest = 0;
            foreach(string filename in Directory.GetFiles("Logs"))
            {
                if (int.TryParse(filename.Replace(".txt", "").Split("-")[1], out int number))
                {
                    if (number > highest) highest = number;
                }
            }

            Filename = $"Logs/Log-{highest + 1}.txt";

            Title();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string output = Buffer.ToString();
            Buffer.Clear();

            File.AppendAllText(Filename, output);
        }

        public void Log(string module, string message, LogSeverity severity = LogSeverity.Dbug)
        {
            string time = $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}";
            string output = $"{time}  {severity,-4}  {module,-10}  {message}\n";

            LogRaw(output);
        }

        public void LogEmpty(bool fileOnly = false)
        {
            if(!fileOnly) Console.Write('\n');
            Buffer.Append('\n');
        }

        public void LogRaw(string message)
        {
            Console.Write(message);
            Buffer.Append(message);
        }

        public void ReportError(string module, Exception exception, LogSeverity severity = LogSeverity.Errr)
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

        private void Title()
        {
            string title = @"
===============================================================================
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
===============================================================================


";

            LogRaw(title);
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
