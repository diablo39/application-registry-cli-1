using ApplicationRegistry.Collector.Batches;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.IO;

namespace ApplicationRegistry.Collector.Tests.Batches
{
    
    public class SanitazeApplicationArgumentsBatchTests
    {
        [Theory]
        [MemberData(nameof(DataForHappyPath))]
        public async Task Batch_Schould_Sanitaze_Path(string projectFilePath, string solutionFilePath, string expectedProjectFile, string expectedSolutionFile)
        {
            var batch = new SanitazeApplicationArgumentsBatch();
            var context = new BatchContext();
            context.Arguments.ProjectFilePath = projectFilePath;
            context.Arguments.SolutionFilePath = solutionFilePath;

            var result = await batch.ProcessAsync(context);

            var currentProjectFile = context.Arguments.ProjectFilePath;
            var currentSolutionFile = context.Arguments.SolutionFilePath;


            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            currentProjectFile.Should().Be(expectedProjectFile);
            currentSolutionFile.Should().Be(expectedSolutionFile);
        }

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
                    //new object[] { projectFileRelativePath, null, expectedProjectFile, expectedSolutionFilePath },
                };
            }
        }
    }
}
