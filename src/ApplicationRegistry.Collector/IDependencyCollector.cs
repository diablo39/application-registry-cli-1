using System.Collections.Generic;
using ApplicationRegistry.Collector.Model;

namespace ApplicationRegistry.Collector
{
    public interface IDependencyCollector
    {
        List<ApplicationVersionDependency> GetDependencies();
    }
}
