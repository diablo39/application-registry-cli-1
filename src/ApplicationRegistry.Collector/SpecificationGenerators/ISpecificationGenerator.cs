using System.Collections.Generic;
using ApplicationRegistry.Collector.Model;

namespace ApplicationRegistry.Collector
{
    public interface ISpecificationGenerator
    {
        List<ApplicationVersionSpecification> GetSpecifications();
    }
}
