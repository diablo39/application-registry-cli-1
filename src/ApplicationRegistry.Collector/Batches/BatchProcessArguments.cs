using System;

namespace ApplicationRegistry.Collector
{
    class BatchProcessArguments
    {
        public string Version { get; set; }

        public string Applicatnion { get; set; }

        public string Environment { get; set; }

        public string SolutionFilePath { get; set; }

        public string ProjectFilePath { get; set; }

        public string FileOutput { get; set; }

        public Uri Url { get; set; }
    }
}
