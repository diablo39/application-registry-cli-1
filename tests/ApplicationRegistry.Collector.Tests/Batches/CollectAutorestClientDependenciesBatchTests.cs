using ApplicationRegistry.Collector.Batches.Implementations.Dependencies;
using ApplicationRegistry.Collector.Tests.TestingInfrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using FluentAssertions.Collections;
using System.Threading.Tasks;
using System.IO;

namespace ApplicationRegistry.Collector.Tests.Batches
{
    public class CollectAutorestClientDependenciesBatchTests : IClassFixture<TestsContext>
    {
        string _webApplication1Path = Path.GetFullPath("../../../../../samples/ApplicationRegistry.Sample.WebApplication1/ApplicationRegistry.Sample.WebApplication1.csproj");
        string _webApplication2Path = Path.GetFullPath("../../../../../samples/ApplicationRegistry.Sample.WebApplication2/ApplicationRegistry.Sample.WebApplication2.csproj");
        string _webApplication3Path = Path.GetFullPath("../../../../../samples/ApplicationRegistry.Sample.WebApplication3/ApplicationRegistry.Sample.WebApplication3.csproj");
        string _solutionFilePath = Path.GetFullPath("../../../../../samples/Samples.sln");

        public CollectAutorestClientDependenciesBatchTests(TestsContext context)
        {

        }

        [Fact(DisplayName = "Getting_autorest_dependencies_should_ok_for_webapp1")]
        public async Task Getting_autorest_dependencies_should_ok()
        {
            var context = new BatchContext(new BatchProcessArguments
            {
                ProjectFilePath = _webApplication1Path,
                SolutionFilePath = _solutionFilePath
            });

            var batch = new CollectAutorestClientDependenciesBatch();

            var result = await batch.ProcessAsync(context);

            result.Should().NotBeNull();

            result.Result.Should().Be(BatchExecutionResult.ExecutionResult.Success);

            context.BatchResult.Dependencies.Should().HaveCount(3);
        }

        [Fact(DisplayName = "Getting_autorest_dependencies_should_ok_for_webapp2")]
        public async Task Getting_autorest_dependencies_should_ok_2()
        {
            var context = new BatchContext(new BatchProcessArguments
            {
                ProjectFilePath = _webApplication2Path,
                SolutionFilePath = _solutionFilePath
            });

            var batch = new CollectAutorestClientDependenciesBatch();

            var result = await batch.ProcessAsync(context);

            result.Should().NotBeNull();

            result.Result.Should().Be(BatchExecutionResult.ExecutionResult.Success);

            context.BatchResult.Dependencies.Should().HaveCount(1);
        }

        [Fact(DisplayName = "Getting_missing_autorest_dependencies_should_ok_for_webapp3")]
        public async Task Getting_missing_autorest_dependencies_should_ok_for_webapp3()
        {
            var context = new BatchContext(new BatchProcessArguments
            {
                ProjectFilePath = _webApplication3Path,
                SolutionFilePath = _solutionFilePath
            });

            var batch = new CollectAutorestClientDependenciesBatch();

            var result = await batch.ProcessAsync(context);

            result.Should().NotBeNull();

            result.Result.Should().Be(BatchExecutionResult.ExecutionResult.Success);

            context.BatchResult.Dependencies.Should().BeNullOrEmpty();
        }

    }
}
