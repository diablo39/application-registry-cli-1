using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationRegistry.Collector.Tests.TestingInfrastructure
{
    class TestingLogProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public TestingLogProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        public void Dispose()
        {
            
        }
    }

    public class TestingLogger: ILogger
    {
        public int Error { get; set; } = 0;
        public int Critical { get; set; } = 0;
        public int Warning { get; set; } = 0;
        public int Info { get; set; } = 0;
        public int Debug { get; set; } = 0;
        public int Trace { get; set; } = 0;

        public void ResetLogger()
        {
            Error = Critical = Info = Debug = Trace = Warning = 0;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    ++Trace;
                    break;
                case LogLevel.Debug:
                    ++Debug;
                    break;
                case LogLevel.Information:
                    ++Info;
                    break;
                case LogLevel.Warning:
                    ++Warning;
                    break;
                case LogLevel.Error:
                    ++Error;
                    break;
                case LogLevel.Critical:
                    ++Critical;
                    break;
                case LogLevel.None:
                default:
                    break;
            }
        }
    }

    public class TestingLogger<T>: ILogger<T>
    {

        public int Error { get; set; } = 0;
        public int Critical { get; set; } = 0;
        public int Warning { get; set; } = 0;
        public int Info { get; set; } = 0;
        public int Debug { get; set; } = 0;
        public int Trace { get; set; } = 0;

        public void ResetLogger()
        {
            Error = Critical = Info = Debug = Trace = 0;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    ++Trace;
                    break;
                case LogLevel.Debug:
                    ++Debug;
                    break;
                case LogLevel.Information:
                    ++Info;
                    break;
                case LogLevel.Warning:
                    ++Warning;
                    break;
                case LogLevel.Error:
                    ++Error;
                    break;
                case LogLevel.Critical:
                    ++Critical;
                    break;
                case LogLevel.None:
                default:
                    break;
            }
        }
    }

    public class LoggingContext
    {
        public TestingLogger Logger { get; set; } = new TestingLogger();

        public LoggingContext()
        {
            LoggerHelper.LoggerFactory = CreateLoggerFactory();
        }

        public void ResetLogger()
        {
            Logger.ResetLogger();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        private LoggerFactory CreateLoggerFactory()
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new TestingLogProvider(Logger));
            return loggerFactory;
        }
    }
}
