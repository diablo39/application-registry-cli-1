using ApplicationRegistry.Collector.Model;
using System;
using System.Collections.Generic;

namespace ApplicationRegistry.Collector.SpecificationGenerators
{
    class DatabaseScpecificationGenerator : ISpecificationGenerator
    {
        public List<ApplicationVersionSpecification> GetSpecifications()
        {
            return new List<ApplicationVersionSpecification>();
        }
    }
}
