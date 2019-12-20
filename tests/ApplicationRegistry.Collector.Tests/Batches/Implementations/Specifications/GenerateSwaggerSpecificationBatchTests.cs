using ApplicationRegistry.Collector.Batches.Implementations.Specifications;
using ApplicationRegistry.Collector.Tests.TestingInfrastructure;
using ApplicationRegistry.Collector.Wrappers;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ApplicationRegistry.Collector.Tests.Batches.Implementations.Specifications
{
    public class GenerateSwaggerSpecificationBatchTests : IClassFixture<LoggingContext>
    {
        string _webApplication1Path = Path.GetFullPath("../../../../../samples/ApplicationRegistry.Sample.WebApplication1/ApplicationRegistry.Sample.WebApplication1.csproj");
        string _webApplication2Path = Path.GetFullPath("../../../../../samples/ApplicationRegistry.Sample.WebApplication2/ApplicationRegistry.Sample.WebApplication2.csproj");
        string _webApplication3Path = Path.GetFullPath("../../../../../samples/ApplicationRegistry.Sample.WebApplication3/ApplicationRegistry.Sample.WebApplication3.csproj");
        string _webApplication4Path = Path.GetFullPath("../../../../../samples/ApplicationRegistry.Sample.WebApplication4_Non_Compile/ApplicationRegistry.Sample.WebApplication4_Non_Compile.csproj");
        string _solutionFilePath = Path.GetFullPath("../../../../../samples/Samples.sln");

        public LoggingContext LoggingContext { get; }

        public GenerateSwaggerSpecificationBatchTests(LoggingContext context)
        {
            LoggingContext = context;
        }

        [Fact(DisplayName = "Getting_swagger_specification_should_ok_for_webapp1")]
        public async Task Getting_autorest_dependencies_should_ok()
        {
            LoggingContext.ResetLogger();

            var context = new BatchContext(new BatchProcessArguments
            {
                ProjectFilePath = _webApplication1Path,
                SolutionFilePath = _solutionFilePath
            });

            var fileSystem = new FileSystem();

            var batch = new GenerateSwaggerSpecificationBatch(fileSystem);

            var result = await batch.ProcessAsync(context);

            result.Should().NotBeNull();

            result.Result.Should().Be(BatchExecutionResult.ExecutionResult.Success);

            context.BatchResult.Specifications.Should().HaveCount(1);

            LoggingContext.Logger.Error.Should().Be(0);
            LoggingContext.Logger.Critical.Should().Be(0);
        }

        [Fact(DisplayName = "should_fail")]
        public async Task ddsadas()
        {
            LoggingContext.ResetLogger();

            var context = new BatchContext(new BatchProcessArguments
            {
                ProjectFilePath = _webApplication4Path,
                SolutionFilePath = _solutionFilePath
            });

            var fileSystem = new FileSystem();

            var batch = new GenerateSwaggerSpecificationBatch(fileSystem);

            var result = await batch.ProcessAsync(context);

            result.Should().NotBeNull();

            result.Result.Should().Be(BatchExecutionResult.ExecutionResult.Error);

            context.BatchResult.Specifications.Should().BeEmpty();

            LoggingContext.Logger.Error.Should().BeGreaterThan(0);
            LoggingContext.Logger.Critical.Should().Be(0);
        }
    }
}
