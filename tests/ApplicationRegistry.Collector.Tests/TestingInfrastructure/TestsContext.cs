using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationRegistry.Collector.Tests.TestingInfrastructure
{
    public class TestsContext
    {
        public TestsContext()
        {
            LoggerHelper.LoggerFactory = new LoggerFactory();
            LoggerHelper.LoggerFactory.AddConsole();
        }
    }
}
