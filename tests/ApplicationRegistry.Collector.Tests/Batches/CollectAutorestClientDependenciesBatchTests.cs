using ApplicationRegistry.Collector.Batches.Implementations.Dependencies;
using ApplicationRegistry.Collector.Tests.TestingInfrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Tests.Batches
{
    class CollectAutorestClientDependenciesBatchTests : IClassFixture<TestsContext>
    {
        public async Task aaa()
        {
            var context = new BatchContext(new BatchProcessArguments
            {
                ProjectFilePath = "ProjectFilePath",
                SolutionFilePath = "SolutionFilePath"
            });

            var batch = new CollectAutorestClientDependenciesBatch();

            var result = await batch.ProcessAsync(context);

            result.Should().NotBeNull();

            result.Result.Should().Be(BatchExecutionResult.ExecutionResult.Success);
        }

    }
}
