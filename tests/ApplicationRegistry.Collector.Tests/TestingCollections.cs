using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ApplicationRegistry.Collector.Tests
{
    [CollectionDefinition("NoParallelization", DisableParallelization = true)]
    public class NoParallelizationCollection { }
}
