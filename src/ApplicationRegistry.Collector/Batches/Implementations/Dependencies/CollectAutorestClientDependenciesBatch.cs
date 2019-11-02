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

        public async Task<BatchExecutionResult> ProcessAsync(BatchContext context)
        {
            try
            {
                var dependencies = await GetDependenciesAsync(context.Arguments.ProjectFilePath, context.Arguments.SolutionFilePath);

                context.BatchResult.Dependencies.AddRange(dependencies);

                return BatchExecutionResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                "Error while processing autorest dependencies".LogError(this, ex);

                return BatchExecutionResult.CreateErrorResult();
            }
        }

        private async Task<List<ApplicationVersionDependency>> GetDependenciesAsync(string projectFilePath, string solutionFilePath)
        {
            var result = new List<ApplicationVersionDependency>();

            AnalyzerManager manager = new AnalyzerManager(solutionFilePath);

            AdhocWorkspace workspace = manager.GetWorkspace();

            var project = workspace.CurrentSolution.Projects.FirstOrDefault(p => p.FilePath == projectFilePath);

            var compilation = await project.GetCompilationAsync();

            var restClientBaseClass = compilation.GetTypeByMetadataName(_restClientBaseClassName);

            if (restClientBaseClass == null) 
            {
                "No autorest type found in solution".LogInfo(this);
                return result;
            }

            var projectsToScan = GetProjectsToBeScanned(workspace, project);

            var restClients = await SymbolFinder.FindDerivedClassesAsync(restClientBaseClass, workspace.CurrentSolution, projectsToScan);

            foreach (var restClient in restClients)
            {
                var members = restClient.GetMembers().OfType<IMethodSymbol>().Where(t => t.Name.EndsWith("WithHttpMessagesAsync")).ToList();
                var operations = new List<ApplicationVersionDependency.Operation>();

                foreach (var member in members)
                {
                    string operationId = GetOperationId(member);
                    string path = GetPath(member);
                    string httpMethod = GetHttpMethod(member);

                    operations.Add(
                        new ApplicationVersionDependency.Operation
                        {
                            IsInUse = false,
                            OperationId = operationId,
                            Path = "/" + path,
                            HttpMethod = httpMethod
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

            return result;
        }

        private static string GetOperationId(IMethodSymbol member)
        {
            return member.Name.Substring(0, member.Name.Length - "WithHttpMessagesAsync".Length);
        }

        private static ImmutableHashSet<Project> GetProjectsToBeScanned(AdhocWorkspace workspace, Project project)
        {
            var graph = workspace.CurrentSolution.GetProjectDependencyGraph();
            var dependencies = graph.GetProjectsThatThisProjectTransitivelyDependsOn(project.Id);
            var projectIdsToScan = dependencies.Union(new[] { project.Id });

            var projectsToScan = workspace.CurrentSolution.Projects.Where(e => projectIdsToScan.Any(p => p.Id == e.Id.Id)).ToImmutableHashSet();
            return projectsToScan;
        }

        private string GetPath(IMethodSymbol member)
        {
            try
            {
                if (member.Locations.Length > 1 || member.Locations.Length == 0)
                {
                    "More then one location found for method. Skipping {0}".LogWarning(this, member.Name);
                    return "";
                }

                var uriDeclaration = member.Locations[0]
                                        .SourceTree.GetRoot()
                                        .FindNode(member.Locations[0].SourceSpan)
                                        .DescendantNodes()
                                        .Where(n => n.GetText().ToString().Contains("var _url ") && n is LocalDeclarationStatementSyntax)
                                        .ToList();

                if (uriDeclaration.Count != 1)
                {
                    "MORE_THEN_ONE_URL_DECLARED".LogWarning(this);

                    return "";
                }

                var literals = uriDeclaration
                    .FirstOrDefault()
                    .DescendantNodes()
                    .OfType<LiteralExpressionSyntax>()
                    .FirstOrDefault(n =>
                    {
                        var text = n.GetText().ToString();
                        return text != "\"/\"" && text != "\"\"" && text != "" && text != "\"\" ";
                    });

                var path = literals?.GetText()?.ToString()?.Trim('"') ?? "";

                if (string.IsNullOrWhiteSpace(path))
                {
                    "Path not found for member {0}".LogWarning(this, member.Name);
                }
                return path;
            }
            catch (Exception ex)
            {
                "Error while looking for url in member {0}".LogError(this, ex, member.Name);
                return "";
            }
        }

        private string GetHttpMethod(IMethodSymbol member)
        {
            try
            {
                if (member.Locations.Length > 1 || member.Locations.Length == 0)
                {
                    "More then one location found for method. Skipping {0}".LogWarning(this, member.Name);
                    return "";
                }

                var methodAssignment = member.Locations[0]
                                        .SourceTree.GetRoot()
                                        .FindNode(member.Locations[0].SourceSpan)
                                        .DescendantNodes()
                                        .OfType<AssignmentExpressionSyntax>()
                                        .SingleOrDefault(n => n.GetText().ToString().Contains("_httpRequest.Method = "));

                if (methodAssignment == null)
                {
                    "Can't find http method for member {0}".LogWarning(this, member.Name);
                    return "";
                }

                var httpMethod = methodAssignment
                    .DescendantNodes()
                    .OfType<LiteralExpressionSyntax>()
                    .SingleOrDefault()
                    .Token
                    .Value
                    .ToString();

                return httpMethod;
            }
            catch (Exception ex)
            {
                "Error while looking for url in member {0}".LogError(this, ex, member.Name);
                return "";
            }
        }

    }
}
