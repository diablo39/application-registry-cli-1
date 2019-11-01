using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationRegistry.Collector.Model;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Linq;


namespace ApplicationRegistry.Collector.Batches.Implementations.Dependencies
{
    class CollectAutorestClientDependenciesBatch : IBatch
    {
        private const string _restClientBaseClassName = "Microsoft.Rest.ServiceClient`1";


        public Task<BatchExecutionResult> ProcessAsync(BatchContext context)
        {
            try
            {
                var dependencies = GetDependencies(context.Arguments.ProjectFilePath, context.Arguments.SolutionFilePath);

                context.BatchResult.Dependencies.AddRange(dependencies);

                return Task.FromResult(BatchExecutionResult.CreateSuccessResult());
            }
            catch (Exception)
            {
                return Task.FromResult(BatchExecutionResult.CreateErrorResult());
            }
        }

        

        private List<ApplicationVersionDependency> GetDependencies(string projectFilePath, string solutionFilePath)
        {
            var result = new List<ApplicationVersionDependency>();

            try
            {
                var projectFile = projectFilePath;

                AnalyzerManager manager = new AnalyzerManager(solutionFilePath);

                var projects = manager.Projects;

                AdhocWorkspace workspace = manager.GetWorkspace();

                var project = workspace.CurrentSolution.Projects.Where(p => p.FilePath == projectFile).FirstOrDefault();

                var graph = workspace.CurrentSolution.GetProjectDependencyGraph();
                var dependencies = graph.GetProjectsThatThisProjectTransitivelyDependsOn(project.Id);
                var projectIdsToScan = dependencies.Union(new[] { project.Id });

                var projectsToScan = workspace.CurrentSolution.Projects.Where(e => projectIdsToScan.Any(p => p.Id == e.Id.Id));

                var compilation = project.GetCompilationAsync().Result;

                var restClientBaseClass = compilation.GetTypeByMetadataName(_restClientBaseClassName);

                if (restClientBaseClass == null)
                    return result;

                var restClients = SymbolFinder.FindDerivedClassesAsync(restClientBaseClass, workspace.CurrentSolution, projectsToScan.ToImmutableHashSet()).Result;

                foreach (var restClient in restClients)
                {
                    var members = restClient.GetMembers().OfType<IMethodSymbol>().Where(t => t.Name.EndsWith("WithHttpMessagesAsync")).ToList();
                    var operations = new List<ApplicationVersionDependency.Operation>();

                    foreach (var member in members)
                    {
                        var methodName = member.Name.Substring(0, member.Name.Length - "WithHttpMessagesAsync".Length);

                        string path = GetPath(member);

                        operations.Add(
                            new ApplicationVersionDependency.Operation
                            {
                                IsInUse = false,
                                OperationId = methodName,
                                Path = "/" + path
                            });
                    }

                    result.Add(new ApplicationVersionDependency
                    {
                        DependencyType = "AUTORESTCLIENT",
                        Name = restClient.ToString(),
                        Version = "-",
                        VersionExtraProperties = new Dictionary<string, object>
                            {
                                { "Operations",operations }
                            }
                    });
                }
            }
            catch (Exception ex)
            {
                "Error while processing autorest dependencies".LogError(this, ex);
            }

            return result;
        }

        private static string GetPath(IMethodSymbol member)
        {
            try
            {
                if (member.Locations.Length > 1 || member.Locations.Length == 0) return "";


                var uriDeclaration = member.Locations[0]
                                        .SourceTree.GetRoot()
                                        .FindNode(member.Locations[0].SourceSpan)
                                        .DescendantNodes()
                                        .Where(
                                            n => n.GetText().ToString().Contains("var _url")
                                            && n is LocalDeclarationStatementSyntax).ToList();

                if (uriDeclaration.Count != 1) return "";

                var literals = uriDeclaration
                    .FirstOrDefault()
                    .DescendantNodes()
                    .Where(n => n is LiteralExpressionSyntax && n.GetText().ToString() != "\"/\"" && n.GetText().ToString() != "\"\"" && n.GetText().ToString() != "" && n.GetText().ToString() != "\"\" ");

                var path = literals.FirstOrDefault()?.GetText()?.ToString()?.Trim('"') ?? "";
                return path;
            }
            catch (Exception)
            {
                return "PROCESSING_ERROR";
            }
        }

    }
}
