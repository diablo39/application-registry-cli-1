using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ApplicationRegistry.Collector.Model
{
    public class ApplicationInfo
    {
        [Required]
        public string ApplicationCode { get; set; }

        [Required]
        public string IdEnvironment { get; set; }

        [Required]
        public string Version { get; set; }

        public string IdCommit { get; set; }

        public bool SpecificationGenerationFailed { get; set; }

        public bool DependencyFillectionFailed { get; set; }

        public string ToolsVersion { get; set; }

        // Navigation properties
        public List<ApplicationVersionSpecification> Specifications { get; } = new List<ApplicationVersionSpecification>();

        public List<ApplicationVersionDependency> Dependencies { get; } = new List<ApplicationVersionDependency>();
        
    }
}
