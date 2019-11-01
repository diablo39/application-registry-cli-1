using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationRegistry.Collector.Tests.TestingInfrastructure
{
    class TestingLogProvider : ILoggerProvider
    {
        static TestingLogger _instance = new TestingLogger();

        public ILogger CreateLogger(string categoryName)
        {
            return _instance;
        }

        public void Dispose()
        {
            
        }
    }

    class TestingLogger : ILogger
    {


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
            
        }
    }

    public class TestsContext
    {
        public TestsContext()
        {
            //LoggerHelper.LoggerFactory = new LoggerFactory();
        }
    }
}
