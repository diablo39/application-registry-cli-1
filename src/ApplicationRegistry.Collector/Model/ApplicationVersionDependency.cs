using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ApplicationRegistry.Collector.Model
{
    public class ApplicationVersionDependency
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string DependencyType { get; set; }

        [Required]
        public string Version { get; set; }

        public Dictionary<string, object> ExtraProperties { get; set; }
    }
}
