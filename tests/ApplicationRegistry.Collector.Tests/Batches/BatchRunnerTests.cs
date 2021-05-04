using ApplicationRegistry.Collector.Batches;
using ApplicationRegistry.Collector.Tests.TestingInfrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace ApplicationRegistry.Collector.Tests.Batches
{
    public class BatchRunnerTests : IClassFixture<LoggingContext>
    {
        private class WorkingBatch : IBatch
        {
            public Task<BatchExecutionResult> ProcessAsync(BatchContext context)
            {
                return Task.FromResult(BatchExecutionResult.CreateSuccessResult());
            }
        }

        private class FailingBatch : IBatch
        {
            public Task<BatchExecutionResult> ProcessAsync(BatchContext context)
            {
                return Task.FromResult(BatchExecutionResult.CreateFailResult());
            }
        }

        private class ErrorBatch : IBatch
        {
            public Task<BatchExecutionResult> ProcessAsync(BatchContext context)
            {
                return Task.FromResult(BatchExecutionResult.CreateErrorResult());
            }
        }
        private class ExceptionBatch : IBatch
        {
            public Task<BatchExecutionResult> ProcessAsync(BatchContext context)
            {
                throw new Exception("Failed!");
            }
        }

        private LoggingContext _context;

        public BatchRunnerTests(LoggingContext context)
        {
            _context = context;
        }

        [Fact]
        public async Task Empty_batch_collection()
        {
            // Arrange
            _context.ResetLogger();
            var batches = new List<IBatch>();
            var batchRunner = new BatchRunner(batches);
            var batchCantext = new BatchContext(new BatchProcessArguments());
            
            // Act
            await batchRunner.RunBatchesAsync(batchCantext);

            //Assert
            _context.Logger.Error.Should().Be(0);
            _context.Logger.Warning.Should().Be(0);
            _context.Logger.Critical.Should().Be(0);
        }

        [Fact]
        public async Task Non_empty_batch_collection()
        {
            // Arrange
            _context.ResetLogger();
            var batches = new List<IBatch>
            {
                new WorkingBatch()
            };
            var batchRunner = new BatchRunner(batches);
            var batchCantext = new BatchContext(new BatchProcessArguments());

            // Act
            await batchRunner.RunBatchesAsync(batchCantext);

            //Assert
            _context.Logger.Error.Should().Be(0);
            _context.Logger.Warning.Should().Be(0);
            _context.Logger.Critical.Should().Be(0);
        }

        [Fact]
        public async Task Error_batch_collection()
        {
            // Arrange
            _context.ResetLogger();
            var batches = new List<IBatch>
            {
                new ErrorBatch()
            };
            var batchRunner = new BatchRunner(batches);
            var batchCantext = new BatchContext(new BatchProcessArguments());

            // Act
            await batchRunner.RunBatchesAsync(batchCantext);

            //Assert
            _context.Logger.Warning.Should().NotBe(0); 
            _context.Logger.Error.Should().Be(0);
            _context.Logger.Critical.Should().Be(0);
        }

        [Fact]
        public async Task Fail_batch_collection()
        {
            // Arrange
            _context.ResetLogger();
            var batches = new List<IBatch>
            {
                new FailingBatch()
            };
            var batchRunner = new BatchRunner(batches);
            var batchCantext = new BatchContext(new BatchProcessArguments());

            // Act
            await batchRunner.RunBatchesAsync(batchCantext);

            //Assert
            _context.Logger.Warning.Should().Be(0); 
            _context.Logger.Error.Should().NotBe(0);
            _context.Logger.Critical.Should().Be(0);
        }

        [Fact]
        public async Task Exception_batch_collection()
        {
            // Arrange
            _context.ResetLogger();
            var batches = new List<IBatch>
            {
                new ExceptionBatch()
            };
            var batchRunner = new BatchRunner(batches);
            var batchCantext = new BatchContext(new BatchProcessArguments());

            // Act
            await batchRunner.RunBatchesAsync(batchCantext);

            //Assert
            _context.Logger.Warning.Should().Be(0);
            _context.Logger.Error.Should().NotBe(0);
            _context.Logger.Critical.Should().Be(0);
        }

        [Fact]
        public async Task Failed_batch_should_not_stop_processing()
        {
            // Arrange
            var workingBatchMock = new Mock<IBatch>();
            
            _context.ResetLogger();
            var batches = new List<IBatch>
            {
                new FailingBatch(),
                workingBatchMock.Object
            };
            var batchRunner = new BatchRunner(batches);
            var batchCantext = new BatchContext(new BatchProcessArguments());

            // Act
            await batchRunner.RunBatchesAsync(batchCantext);

            //Assert
            workingBatchMock.Verify(e=> e.ProcessAsync(It.IsAny<BatchContext>()));
        }

        [Fact]
        public async Task Exception_batch_should_not_stop_processing()
        {
            // Arrange
            var workingBatchMock = new Mock<IBatch>();

            _context.ResetLogger();
            var batches = new List<IBatch>
            {
                new ExceptionBatch(),
                workingBatchMock.Object
            };
            var batchRunner = new BatchRunner(batches);
            var batchCantext = new BatchContext(new BatchProcessArguments());

            // Act
            await batchRunner.RunBatchesAsync(batchCantext);

            //Assert
            workingBatchMock.Verify(e => e.ProcessAsync(It.IsAny<BatchContext>()));
        }
    }
}
