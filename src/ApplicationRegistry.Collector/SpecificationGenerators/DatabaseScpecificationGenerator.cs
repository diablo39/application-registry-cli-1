using System;
using System.Collections.Generic;
using System.Text;
using ApplicationRegistry.Collector.Model;

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
