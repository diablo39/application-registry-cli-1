using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationRegistry.Collector.Model;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ApplicationRegistry.Collector.Batches.Implementations.Dependencies
{
    class CollectNugetDependenciesBatch : IBatch
    {
        private SortedSet<string> _processedProjects = new SortedSet<string>();

        public Task<BatchExecutionResult> ProcessAsync(BatchContext context)
        {
            try
            {
                var dependencies = GetDependencies(context.Arguments.ProjectFilePath);

                context.BatchResult.Dependencies.AddRange(dependencies);

                return Task.FromResult(BatchExecutionResult.CreateSuccessResult());
            }
            catch (Exception)
            {
                return Task.FromResult(BatchExecutionResult.CreateErrorResult());
            }
        }

        public List<ApplicationVersionDependency> GetDependencies(string projectFilePath)
        {
            var result = new List<ApplicationVersionDependency>();
            var projectDirectory = new FileInfo(projectFilePath).Directory.FullName;

            var xml = XDocument.Load(projectFilePath);

            var packageReferences = xml.XPathSelectElements("//PackageReference");

            foreach (var item in packageReferences)
            {
                if (item.Attribute("Version") == null)
                    continue;

                result.Add(new ApplicationVersionDependency { DependencyType = "NUGET", Name = item.Attribute("Include").Value, Version = item.Attribute("Version").Value });
            }

            var projectReferences = xml.XPathSelectElements("//ProjectReference");

            foreach (var item in projectReferences)
            {
                var referencedProject = Path.Combine(projectDirectory, item.Attribute("Include").Value);

                if (_processedProjects.Contains(referencedProject)) continue;

                var collectorDependencies = GetDependencies(referencedProject);

                for (int i = 0; i < collectorDependencies.Count; i++)
                {
                    var d = collectorDependencies[i];
                    if (!result.Any(e => e.DependencyType == d.DependencyType && string.Equals(e.Name, d.Name, StringComparison.InvariantCultureIgnoreCase)))
                        result.Add(d);
                }

                _processedProjects.Add(referencedProject);
            }

            return result;
        }

    }
}
