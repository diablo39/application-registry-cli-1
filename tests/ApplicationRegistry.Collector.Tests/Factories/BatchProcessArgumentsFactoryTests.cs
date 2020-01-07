using ApplicationRegistry.Collector.Tests.TestingInfrastructure;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ApplicationRegistry.Collector.Tests.Factories
{
    public class BatchProcessArgumentsFactoryTests : IClassFixture<LoggingContext>
    {
        public BatchProcessArgumentsFactoryTests(LoggingContext context) { }
        
        [Theory]
        [MemberData(nameof(DataForHappyPath))]
        public void Factory_Schould_Sanitaze_Path_With_Correct_Result(string projectFilePath, string solutionFilePath, string expectedProjectFile, string expectedSolutionFile)
        {
            var factory = new BatchProcessArgumentsFactory();
            var arguments = factory.Create("application", "env", "", projectFilePath, solutionFilePath, null, "");

            var currentProjectFile = arguments.ProjectFilePath;
            var currentSolutionFile = arguments.SolutionFilePath;

            currentProjectFile.Should().Be(expectedProjectFile);
            currentSolutionFile.Should().Be(expectedSolutionFile);
        }


        //[Fact]
        //public void Factory_Nonexisting_ProjectFile_Path_ShouldFail()
        //{
        //    var batch = new SanitazeApplicationArgumentsBatch();
        //    var context = new BatchContext(new BatchProcessArguments
        //    {
        //        ProjectFilePath = Path.GetFullPath(".")
        //    });

        //    var result = await batch.ProcessAsync(context);

        //    var currentProjectFile = context.Arguments.ProjectFilePath;
        //    var currentSolutionFile = context.Arguments.SolutionFilePath;


        //    result.Should().NotBeNull();
        //    result.Result.Should().Be(BatchExecutionResult.ExecutionResult.Fail);
        //    //currentProjectFile.Should().Be(expectedProjectFile);
        //    //currentSolutionFile.Should().Be(expectedSolutionFile);
        //}

        public static IEnumerable<object[]> DataForHappyPath
        {
            get
            {
                var projectFileRelativePath = "../../../ApplicationRegistry.Collector.Tests.csproj";
                var projectDirectoryRelativePath = "../../../";
                var expectedProjectFile = Path.GetFullPath("../../../ApplicationRegistry.Collector.Tests.csproj");
                var solutionFilePathRelative = "../../../../../ApplicationRegistry.Collector.sln";
                var expectedSolutionFilePath = Path.GetFullPath("../../../../../ApplicationRegistry.Collector.sln");

                return new List<object[]>
                    {
                        new object[] { projectFileRelativePath, null, expectedProjectFile, expectedSolutionFilePath },
                        new object[] { projectDirectoryRelativePath, null, expectedProjectFile, expectedSolutionFilePath },
                        new object[] { projectFileRelativePath, solutionFilePathRelative, expectedProjectFile, expectedSolutionFilePath },
                    };
            }
        }
    }
}
