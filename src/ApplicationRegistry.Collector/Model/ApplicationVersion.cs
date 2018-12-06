using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ApplicationRegistry.Collector.Model
{
    public class ApplicationVersion
    {
        [Required]
        public string ApplicationCode { get; set; }

        [Required]
        public string IdEnvironment { get; set; }

        [Required]
        public string Version { get; set; }


        public string IdCommit { get; set; }

        // Navigation properties

        public List<ApplicationVersionSpecification> Specifications { get; set; }

        public List<ApplicationVersionDependency> Dependencies { get; set; }
    }
}
