using System;
using System.IO;

namespace Utili.Services
{
    static class LogWriter
    {
        static readonly object LockObj = new();

        public static void Write(params (string, ConsoleColor)[] message)
        {
            lock (LockObj)
            {
                string rawMessage = "";
                foreach ((string, ConsoleColor) messagePart in message)
                {
                    rawMessage += messagePart.Item1;
                    Console.ForegroundColor = messagePart.Item2;
                    Console.Write(messagePart.Item1);
                }

                WriteToLogFile(rawMessage);
            }
        }

        public static void CreateErrorReport(Exception exception)
        {
            if (!Directory.Exists("Exceptions")) Directory.CreateDirectory("Exceptions");
            string filename = $"Exceptions/Exception-{DateTime.UtcNow.Year:0000}-{DateTime.UtcNow.Month:00}-{DateTime.UtcNow.Day:00} {DateTime.UtcNow.Hour:00}-{DateTime.UtcNow.Minute:00}-{DateTime.UtcNow.Second:00}-{DateTime.UtcNow.Millisecond:0000}.txt";
            StreamWriter errorReport = File.CreateText(filename);

            errorReport.WriteLine($"Exception thrown at {DateTime.UtcNow} UTC\n");

            while (exception is not null)
            {
                errorReport.WriteLine($"{exception.Message}\n{exception.StackTrace}\n\n");
                exception = exception.InnerException;
            }

            errorReport.Close();
        }

        static void WriteToLogFile(string message)
        {
            message = message.Replace("»", ">");
            if (!Directory.Exists("Logs")) Directory.CreateDirectory("Logs");
            File.AppendAllText($"Logs/{DateTime.UtcNow:yyyy-MM-dd}.txt", message);
        }
    }
}