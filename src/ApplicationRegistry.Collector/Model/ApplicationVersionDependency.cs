using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApplicationRegistry.Collector.Model
{
    public class ApplicationVersionDependency
    {
        internal class Operation
        {
            public string OperationId { get; set; }

            public string Path { get; set; }

            public bool IsInUse { get; set; }

            public string HttpMethod { get; internal set; }
        }

        [Required]
        public string Name { get; set; }

        [Required]
        public string DependencyType { get; set; }

        [Required]
        public string Version { get; set; }

        public Dictionary<string, object> ExtraProperties { get; set; }

        public Dictionary<string, object> VersionExtraProperties { get; set; }
    }
}
