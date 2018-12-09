using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ApplicationRegistry.UnitTests
{
    public class RoslynFun
    {
        [Fact]
        public void RunRoslyn()
        {
            string solutionPath = @"C:\Users\Roman\source\repos\ApplicationRegistry.Collector\ApplicationRegistry.Collector.sln";

            string projectFile = @"C:\Users\Roman\source\repos\ApplicationRegistry.Collector\samples\ApplicationRegistry.SampleWebApplication\ApplicationRegistry.SampleWebApplication.csproj";

            AnalyzerManager manager = new AnalyzerManager(solutionPath);

            var projects = manager.Projects;
            //manager.bui


            AdhocWorkspace workspace = manager.GetWorkspace();


            foreach (var project in workspace.CurrentSolution.Projects)
            {
                if (string.Equals(project.FilePath, projectFile, StringComparison.InvariantCultureIgnoreCase))
                {
                    var graph = workspace.CurrentSolution.GetProjectDependencyGraph();
                    var dependencies = graph.GetProjectsThatThisProjectTransitivelyDependsOn(project.Id);

                    var compilation = project.GetCompilationAsync().Result;

                    string TEST_ATTRIBUTE_METADATA_NAME = "Microsoft.Rest.ServiceClient`1";

                    var testAttributeType = compilation.GetTypeByMetadataName(TEST_ATTRIBUTE_METADATA_NAME);
                    var aa = SymbolFinder.FindDerivedClassesAsync(testAttributeType, workspace.CurrentSolution).Result;
                    break;
                }
            }


            Console.WriteLine();

            //var msWorkspace = MSBuildWorkspace.Create();

            //var solution = msWorkspace.OpenSolutionAsync(solutionPath).Result;
            //foreach (var project in solution.Projects)
            //{
            //    foreach (var document in project.Documents)
            //    {
            //        Console.WriteLine(project.Name + "\t\t\t" + document.Name);
            //    }
            //}

            //var workspace = new AdhocWorkspace();
            //workspace.AddSolution(SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Create(), solutionPath));

            //var solution = workspace.CurrentSolution;

        }
    }
}
