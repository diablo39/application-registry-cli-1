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

    public class TestingLogger : ILogger
    {
        public int Critical = 0;

        public int Error = 0;

        public int Warning = 0;
        
        public int Info = 0;

        public int Debug = 0;

        public int Trace = 0;

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
        public TestingLogger Logger = new TestingLogger();

        public LoggingContext()
        {
            LoggerHelper.LoggerFactory = CreateLoggerFactory();
        }

        public void ResetLogger()
        {
            Logger.ResetLogger();
        }

        private LoggerFactory CreateLoggerFactory()
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new TestingLogProvider(Logger));
            return loggerFactory;
        }
    }
}
