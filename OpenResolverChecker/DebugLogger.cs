using System;
using System.Globalization;
using DnsClient.Internal;

namespace OpenResolverChecker
{
    public class DebugLoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger(string categoryName) => new DebugLogger();
    }

    public class DebugLogger : ILogger
    {
        public void Log(LogLevel logLevel, int eventId, Exception exception, string message, params object[] args)
        {
            var displayMsg = $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)} [DNSClient DebugLogger] [{logLevel}] ";
            if (message != null)
                displayMsg += string.Format(message, args);
            if (exception != null)
                displayMsg += Environment.NewLine + exception;

            Console.WriteLine(displayMsg);
        }

        public bool IsEnabled(LogLevel logLevel) => true;
    }
}