using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector
{
    class ApplicationArguments
    {
        public string Version { get; set; }

        public string Applicatnion { get; set; }

        public string Environment { get; set; }

        public string SolutionFilePath { get; set; }

        public string ProjectFilePath { get; set; }

        public string FileOutput { get; set; }

        public Uri Url { get; set; }
    }

    class BatchContext
    {
        public ApplicationArguments Arguments { get; } = new ApplicationArguments();
    }

    interface IBatch
    {
        Task<BatchResult> ProcessAsync(BatchContext context);
    }

    struct BatchResult
    {
        public bool Success { get; private set; }

        public static BatchResult CreateSuccessResult()
        {
            return new BatchResult { Success = true };
        }

        public static BatchResult CreateFailResult()
        {
            return new BatchResult { Success = false };
        }

        public void WriteError(string error)
        {

        }
    }


    class BatchCollection : List<IBatch>
    {

    }
}
