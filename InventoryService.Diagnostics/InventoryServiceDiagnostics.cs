using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace InventoryService.Diagnostics
{
    public class InventoryServiceDiagnostics
    {
#if DEBUG

        public static void Debug(
           Action operation
           , string description = null
           , [CallerMemberName] string memberName = ""
           , [CallerFilePath] string sourceFilePath = ""
           , [CallerLineNumber] int sourceLineNumber = 0)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Exception exception = null;

            try
            {
                operation();
            }
            catch (Exception e)
            {
                exception = e;
                throw;
            }
            finally
            {
                watch.Stop();
                ReportDiagnostics(description, memberName, sourceFilePath, sourceLineNumber, watch, exception);
            }
        }

#else
        public static void Debug(Action operation)
        {
            operation();
        }
#endif

        private static void ReportDiagnostics(string description, string memberName, string sourceFilePath, int sourceLineNumber,
            Stopwatch watch, Exception exception)
        {
            description = description ?? "InventoryService.Diagnostics";

            var fullDescription = Environment.NewLine + "description: " + description +
                                  Environment.NewLine + "member name: " + memberName +
                                  Environment.NewLine + "source file path: " + sourceFilePath +
                                  Environment.NewLine + "source line number: " + sourceLineNumber +
                                  Environment.NewLine;

            System.Diagnostics.Debug.WriteLine("EXECUTION TIME : " + watch.ElapsedMilliseconds + " (ms) " + Environment.NewLine +
                                               fullDescription + Environment.NewLine + " Exception Thrown : " +
                                               ParseException(exception));
        }

        public static string ParseException(Exception exception)
        {
            return ParseException(exception, (e) => e.Message) + ParseException(exception, (e) => "Source : " + e.Source + " - StackTrace : " + e.StackTrace);
        }

        public static string ParseException(List<Exception> Exceptions, Func<Exception, string> op, string separator = ",\r\n<br />")
        {
            return string.Join(separator, (Exceptions?.Select(x => ParseException(x, op, separator)) ?? new List<string>()).ToList());
        }

        public static string ParseException(Exception e, Func<Exception, string> op, string separator = ",\r\n<br />", List<string> pre = null, int currentDepth = 0, int stopAtDepth = 10)
        {
            if (op == null)
            {
                op = (x) => "Message :" + e.Message + "Source : " + x.Source + " - StackTrace : " + x.StackTrace;
            }

            currentDepth++;
            pre = pre ?? new List<string>();
            if (e == null || currentDepth >= stopAtDepth)
            {
                return string.Join(separator, pre);
            }
            pre.Add(op(e));
            return ParseException(e.InnerException, op, separator, pre, currentDepth, stopAtDepth);
        }
    }
}