using ApplicationRegistry.Collector.Batches.Implementations;
using ApplicationRegistry.Collector.Tests.TestingInfrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace ApplicationRegistry.Collector.Tests.Batches
{
    public class CollectApplicationInfoBatchTests : IClassFixture<LoggingContext>
    {
        public CollectApplicationInfoBatchTests(LoggingContext ctx)
        {

        }

        [Fact]
        public async Task Collecting_info_should_ok()
        {
            var context = new BatchContext(new BatchProcessArguments
            {
                ProjectFilePath = "ProjectFilePath",
                SolutionFilePath = "SolutionFilePath"
            });

            var batch = new CollectApplicationInfoBatch();

            var result = await batch.ProcessAsync(context);

            result.Should().NotBeNull();

            result.Result.Should().Be(BatchExecutionResult.ExecutionResult.Success);
        }
    }
}
