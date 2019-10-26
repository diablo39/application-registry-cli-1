using ApplicationRegistry.Collector.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ApplicationRegistry.Collector.DependencyCollectors
{
    class NugetDependencyCollector : IDependencyCollector
    {
        private readonly string _projectFilePath;

        public NugetDependencyCollector(BatchContext context)
        {
            _projectFilePath = context.Arguments.ProjectFilePath;
        }

        private NugetDependencyCollector(string referencedProject)
        {
            _projectFilePath = referencedProject;
        }

        public List<ApplicationVersionDependency> GetDependencies()
        {
            var result = new List<ApplicationVersionDependency>();
            var projectDirectory = new FileInfo(_projectFilePath).Directory.FullName;

            var xml = XDocument.Load(_projectFilePath);

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

                var collector = new NugetDependencyCollector(referencedProject);
                var collectorDependencies = collector.GetDependencies();
                for (int i = 0; i < collectorDependencies.Count; i++)
                {
                    var d = collectorDependencies[i];
                    if (!result.Any(e => e.DependencyType == d.DependencyType && string.Equals(e.Name, d.Name, StringComparison.InvariantCultureIgnoreCase)))
                        result.Add(d);
                }
            }

            return result;
        }
    }
}
