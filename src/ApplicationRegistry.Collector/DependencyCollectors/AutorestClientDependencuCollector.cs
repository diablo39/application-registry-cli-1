using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ApplicationRegistry.Collector.Model;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;

namespace ApplicationRegistry.Collector.DependencyCollectors
{
    class AutorestClientDependencyCollector : IDependencyCollector
    {
        private const string _restClientBaseClassName = "Microsoft.Rest.ServiceClient`1";

        private readonly IOptions<ApplicationOptions> _options;
        private readonly ILogger<AutorestClientDependencyCollector> _logger;

        public AutorestClientDependencyCollector(IOptions<ApplicationOptions> options, ILogger<AutorestClientDependencyCollector> logger)
        {
            _options = options;
            _logger = logger;
        }

        public List<ApplicationVersionDependency> GetDependencies()
        {
            var result = new List<ApplicationVersionDependency>();

            try
            {
                var projectFile = _options.Value.ProjectFilePath;

                AnalyzerManager manager = new AnalyzerManager(_options.Value.SolutionFilePath);

                var projects = manager.Projects;

                AdhocWorkspace workspace = manager.GetWorkspace();

                var project = workspace.CurrentSolution.Projects.Where(p => p.FilePath == projectFile).FirstOrDefault();

                var graph = workspace.CurrentSolution.GetProjectDependencyGraph();
                var dependencies = graph.GetProjectsThatThisProjectTransitivelyDependsOn(project.Id);
                var projectIdsToScan = dependencies.Union(new[] { project.Id });

                var projectsToScan = workspace.CurrentSolution.Projects.Where(e => projectIdsToScan.Any(p => p.Id == e.Id.Id));

                var compilation = project.GetCompilationAsync().Result;

                var restClientBaseClass = compilation.GetTypeByMetadataName(_restClientBaseClassName);

                var restClients = SymbolFinder.FindDerivedClassesAsync(restClientBaseClass, workspace.CurrentSolution, projectsToScan.ToImmutableHashSet()).Result;

                foreach (var restClient in restClients)
                {
                    var members = restClient.GetMembers().OfType<IMethodSymbol>().Where(t => t.Name.EndsWith("WithHttpMessagesAsync")).ToList();
                    var operations = new List<ApplicationVersionDependency.Operation>();

                    foreach (var member in members)
                    {
                        var methodName = member.Name.Substring(0, member.Name.Length - "WithHttpMessagesAsync".Length);


                        operations.Add(
                            new ApplicationVersionDependency.Operation
                            {
                                IsInUse = false,
                                OperationId = methodName,
                                Path = "/value"
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
                _logger.LogError(ex, "Error while processing autorest dependencies");

            }

            return result;

        }
    }
}
