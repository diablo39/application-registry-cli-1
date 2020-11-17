using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationRegistry.Collector.Model;
using System.Linq;
using Buildalyzer;
using Microsoft.CodeAnalysis;
using Buildalyzer.Workspaces;

namespace ApplicationRegistry.Collector.Batches.Implementations.Dependencies
{
    class CollectNugetDependenciesBatch : IBatch
    {
        private SortedSet<string> _processedProjects = new SortedSet<string>();

        public async Task<BatchExecutionResult> ProcessAsync(BatchContext context)
        {
            try
            {
                var dependencies = await GetDependencies(context.Arguments.ProjectFilePath, context.Arguments.SolutionFilePath);

                context.BatchResult.Dependencies.AddRange(dependencies);

                return BatchExecutionResult.CreateSuccessResult();
            }
            catch (Exception)
            {
                return BatchExecutionResult.CreateErrorResult();
            }
        }

        public async Task<List<ApplicationVersionDependency>> GetDependencies(string projectFilePath, string solutionFilePath)
        {
            var result = new List<ApplicationVersionDependency>();

            AnalyzerManager manager = new AnalyzerManager(solutionFilePath);

            AdhocWorkspace workspace = manager.GetWorkspace();

            var project = workspace.CurrentSolution.Projects.FirstOrDefault(p => p.FilePath == projectFilePath);

            var compilation = await project.GetCompilationAsync();

            foreach (var item in compilation.ReferencedAssemblyNames)
            {
                if (workspace.CurrentSolution.Projects.Any(e => e.Name == item.Name || e.AssemblyName == item.Name)) continue;

                result.Add(new ApplicationVersionDependency { DependencyType = "NUGET", Name = item.Name, Version = item.Version.ToString() });
            }

            return result;
        }

    }
}
