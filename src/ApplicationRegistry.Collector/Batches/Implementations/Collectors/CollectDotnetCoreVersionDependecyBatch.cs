using ApplicationRegistry.Collector.Model;
using ApplicationRegistry.Collector.Tools;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches.Implementations.Dependencies
{
    class CollectDotnetCoreVersionDependecyBatch : IBatch
    {
        private CompilationProvider _compilationProvider;

        public CollectDotnetCoreVersionDependecyBatch(CompilationProvider compilationProvider)
        {
            _compilationProvider = compilationProvider;
        }

        public async Task<BatchExecutionResult> ProcessAsync(BatchContext context)
        {
            var solutionFilePath = context.Arguments.SolutionFilePath;
            var projectFilePath = context.Arguments.ProjectFilePath;

            var result = new List<ApplicationVersionDependency>();

            AdhocWorkspace workspace = _compilationProvider.GetWorkspace(solutionFilePath);

            var project = workspace.CurrentSolution.Projects.FirstOrDefault(p => p.FilePath == projectFilePath);

            var compilation = await project.GetCompilationAsync();

            return BatchExecutionResult.CreateSuccessResult();
        }
    }
}
