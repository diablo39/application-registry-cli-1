using System;
using Xunit;

namespace ApplicationRegistry.Collector.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void FullPath()
        {
            var relativePath = "test/aa";
            var a = System.IO.Path.GetFullPath(relativePath);
            Console.WriteLine(a);
        }
    }
}
