using ApplicationRegistry.Collector.Model;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches.Implementations.Dependencies
{
    class CollectDotnetCoreVersionDependecyBatch : IBatch
    {
        public async Task<BatchExecutionResult> ProcessAsync(BatchContext context)
        {
            var solutionFilePath = context.Arguments.SolutionFilePath;
            var projectFilePath = context.Arguments.ProjectFilePath;

            var result = new List<ApplicationVersionDependency>();

            AnalyzerManager manager = new AnalyzerManager(solutionFilePath);



            AdhocWorkspace workspace = manager.GetWorkspace();

            var project = workspace.CurrentSolution.Projects.FirstOrDefault(p => p.FilePath == projectFilePath);



            var compilation = await project.GetCompilationAsync();



            return BatchExecutionResult.CreateSuccessResult();
        }
    }
}
