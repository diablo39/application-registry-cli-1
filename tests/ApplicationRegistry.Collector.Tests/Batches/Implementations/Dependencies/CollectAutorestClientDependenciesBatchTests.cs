using ApplicationRegistry.Collector.Batches.Implementations.Dependencies;
using ApplicationRegistry.Collector.Model;
using ApplicationRegistry.Collector.Tests.TestingInfrastructure;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ApplicationRegistry.Collector.Tests.Batches
{
    public class CollectAutorestClientDependenciesBatchTests : IClassFixture<LoggingContext>
    {
        string _webApplication1Path = Path.GetFullPath("../../../../../samples/ApplicationRegistry.Sample.WebApplication1/ApplicationRegistry.Sample.WebApplication1.csproj");
        string _webApplication2Path = Path.GetFullPath("../../../../../samples/ApplicationRegistry.Sample.WebApplication2/ApplicationRegistry.Sample.WebApplication2.csproj");
        string _webApplication3Path = Path.GetFullPath("../../../../../samples/ApplicationRegistry.Sample.WebApplication3/ApplicationRegistry.Sample.WebApplication3.csproj");
        string _solutionFilePath = Path.GetFullPath("../../../../../samples/Samples.sln");

        public LoggingContext LoggingContext { get; }

        public CollectAutorestClientDependenciesBatchTests(LoggingContext context)
        {
            LoggingContext = context;
        }

        [Fact(DisplayName = "Getting_autorest_dependencies_should_ok_for_webapp1")]
        public async Task Getting_autorest_dependencies_should_ok()
        {
            LoggingContext.ResetLogger();

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

            context.BatchResult.Dependencies.ForEach(dependency => {
                dependency.DependencyType.Should().Be("AUTORESTCLIENT", "only autorest clients should be found");

                dependency.VersionExtraProperties.Keys.Count.Should().Be(1);

                var operations = (dependency.VersionExtraProperties["Operations"] as List<ApplicationVersionDependency.Operation>);

                operations.Should().NotBeNull();

                operations.ForEach(operation => {
                    operation.HttpMethod.Should().NotBeNullOrWhiteSpace();
                    operation.OperationId.Should().NotBeNullOrWhiteSpace();
                    operation.Path.Should().NotBeNullOrWhiteSpace();
                });

            });

            LoggingContext.Logger.Error.Should().Be(0);
            LoggingContext.Logger.Critical.Should().Be(0);
        }

        [Fact(DisplayName = "Getting_autorest_dependencies_should_ok_for_webapp2")]
        public async Task Getting_autorest_dependencies_should_ok_2()
        {
            LoggingContext.ResetLogger();
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

            LoggingContext.Logger.Error.Should().Be(0);
            LoggingContext.Logger.Critical.Should().Be(0);
        }

        [Fact(DisplayName = "Getting_missing_autorest_dependencies_should_ok_for_webapp3")]
        public async Task Getting_missing_autorest_dependencies_should_ok_for_webapp3()
        {
            LoggingContext.ResetLogger();
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

            LoggingContext.Logger.Error.Should().Be(0);
            LoggingContext.Logger.Critical.Should().Be(0);
        }

    }
}
