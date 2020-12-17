using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UtiliSite
{
    public class ErrorLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                string requestId = Activity.Current?.Id ?? context.TraceIdentifier;
                SaveErrorMessage(requestId, exception.Message);

                if (!Directory.Exists("Errors")) Directory.CreateDirectory("Errors");
                string errorReportFilename = $"Errors/{requestId}.txt";
                StreamWriter errorReport = File.CreateText(errorReportFilename);
                await errorReport.WriteLineAsync($"Error report for error at time: {DateTime.Now}\n");
                int errorNumber = 0;
                while (exception != null)
                {
                    await errorReport.WriteLineAsync($"Error {errorNumber}:\n{exception.Message}\n{exception.StackTrace}\n");
                    exception = exception.InnerException;
                    errorNumber++;
                }
                errorReport.Close();
                throw;
            }
        }

        private static Dictionary<string, string> _errors = new Dictionary<string, string>();
        public static void SaveErrorMessage(string requestId, string error)
        {
            _errors.TryAdd(requestId, error);
        }

        public static string GetErrorMessage(string requestId)
        {
            if(_errors.TryGetValue(requestId, out string error))
            {
                _errors.Remove(requestId);
                return error;
            }

            return "null";
        }
    }
}
