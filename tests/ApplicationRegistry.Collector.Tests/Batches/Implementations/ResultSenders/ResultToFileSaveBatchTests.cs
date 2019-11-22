using ApplicationRegistry.Collector.Batches;
using ApplicationRegistry.Collector.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Moq;
using FluentAssertions;
using static ApplicationRegistry.Collector.BatchExecutionResult;

namespace ApplicationRegistry.Collector.Tests.Batches.Implementations.ResultSenders
{
    class ResultToFileSaveBatchTests
    {
        public async Task Saving_should_be_skipped()
        {
            // Arrange
            var context = new BatchContext(new BatchProcessArguments());
            var fileSystemMoq = new Mock<FileSystem>();
            var batch = new ResultToFileSaveBatch(fileSystemMoq.Object);

            // Act
            var taskResult = await batch.ProcessAsync(context);

            //Assert

            taskResult.Result.Should().Be(ExecutionResult.Success);
            fileSystemMoq.VerifyNoOtherCalls();
        }
    }
}
