using System;
using System.Text;

namespace tapi
{
    public enum LogLevel { Verbose, Normal, Silent }
    public static class Logger
    {
        public static LogLevel LogLevel {get;set;} = LogLevel.Normal;
        public static void Log(string message,bool verbose = false, bool skipTime = false)
        {
            if(LogLevel == LogLevel.Silent || LogLevel == LogLevel.Normal && verbose)
                return;

            message = skipTime ? message : $"{DateTime.Now.ToString("HH:mm:ss.fff")} {message}"
                        .Replace("å","")
                        .Replace("ä","")
                        .Replace("ö","")
                        .Replace("Å","")
                        .Replace("Ä","")
                        .Replace("Ö","");
            Console.WriteLine(message);
        }
        public static void LogError(Exception ex)
        {
            var message =  ex.GetBaseException().Message;
            
             var sb = new StringBuilder();

            var e = ex;
            var counter = 1;
            while (e != null && counter < 8)
            {
                sb.AppendLine();
                sb.AppendLine("***** Exception " + counter + " *****************");
                sb.AppendLine("Type: " + e.GetType().Name);
                sb.AppendLine("Message: " + e.Message);
                sb.AppendLine("StackTrace:");
                sb.AppendLine(e.StackTrace);

                counter++;
                e = e.InnerException;
            }

            var stacktrace = sb.ToString();

            var m = LogLevel == LogLevel.Verbose
                ? $"{message}\r\n\r\n"
                : message;            

            var c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = c;

        }
    }

}
