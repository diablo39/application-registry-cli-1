using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ApplicationRegistry.Collector.Model
{
    public class ApplicationVersionSpecification
    {
        public string ContentType { get; set; }

        public string Specification { get; set; }

        public string SpecificationType { get; set; }

        public string Code { get; set; }
    }
}
