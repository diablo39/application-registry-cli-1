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

namespace ApplicationRegistry.Collector.DependencyCollectors
{
    class AutorestClientDependencyCollector : IDependencyCollector
    {
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
                //var dependecy = new ApplicationVersionDependency
                //{
                //    DependencyType = "AUTORESTCLIENT",
                //    Name = ""
                //}

                var projectFile = _options.Value.ProjectFilePath;

                AnalyzerManager manager = new AnalyzerManager(_options.Value.SolutionFilePath);

                var projects = manager.Projects;

                AdhocWorkspace workspace = manager.GetWorkspace();

                var project = workspace.CurrentSolution.Projects.Where(p => p.FilePath == projectFile).FirstOrDefault();

                //var graph = workspace.CurrentSolution.GetProjectDependencyGraph();
                //var dependencies = graph.GetProjectsThatThisProjectTransitivelyDependsOn(project.Id);
                //var projectsToScan = dependencies.Union(new[] { project.Id });

                var compilation = project.GetCompilationAsync().Result;

                string restClientBaseClassName = "Microsoft.Rest.ServiceClient`1";

                var restClientBaseClass = compilation.GetTypeByMetadataName(restClientBaseClassName);
                var restClients = SymbolFinder.FindDerivedClassesAsync(restClientBaseClass, workspace.CurrentSolution).Result;
                var members = restClients.First().GetMembers().OfType<IMethodSymbol>().Where(t => t.Name.EndsWith("WithHttpMessagesAsync"))
                    .Select(e => e.Name.Substring(0, e.Name.Length - "WithHttpMessagesAsync".Length)).ToList();


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing autorest dependencies");

            }

            return result;

        }
    }
}
