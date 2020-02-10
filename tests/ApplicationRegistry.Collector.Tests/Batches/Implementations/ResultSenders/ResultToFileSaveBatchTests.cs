using ApplicationRegistry.Collector.Batches;
using ApplicationRegistry.Collector.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Moq;
using FluentAssertions;
using static ApplicationRegistry.Collector.BatchExecutionResult;
using Xunit;
using System.IO;
using ApplicationRegistry.Collector.Batches.Implementations.ResultSenders;

namespace ApplicationRegistry.Collector.Tests.Batches.Implementations.ResultSenders
{
    public class ResultToFileSaveBatchTests
    {
        [Fact]
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

        [Fact]
        public async Task Saving_should_be_executed()
        {
            // Arrange
            var filePath = Path.GetTempFileName();
            var context = new BatchContext(new BatchProcessArguments
            {
                FileOutput = filePath
            });

            var fileSystemMoq = new Mock<FileSystem>();

            fileSystemMoq.Setup(e => e.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Encoding>()));
            var batch = new ResultToFileSaveBatch(fileSystemMoq.Object);

            // Act
            var taskResult = await batch.ProcessAsync(context);

            //Assert
            fileSystemMoq.VerifyAll();
            taskResult.Result.Should().Be(ExecutionResult.Success);
            

            // Cleanup
            File.Delete(filePath);
        }
    }
}
