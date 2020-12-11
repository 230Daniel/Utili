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
    }
}
