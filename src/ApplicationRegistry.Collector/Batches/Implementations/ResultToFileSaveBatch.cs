
using ApplicationRegistry.Collector.Wrappers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches
{
    class ResultToFileSaveBatch : IBatch
    {
        private readonly ILogger<ResultToFileSaveBatch> _logger;
        private readonly FileSystem _fileSystem;

        public ResultToFileSaveBatch(ILogger<ResultToFileSaveBatch> logger, FileSystem fileSystem)
        {
            _logger = logger;
            _fileSystem = fileSystem;
        }

        public async Task<BatchExecutionResult> ProcessAsync(BatchContext context)
        {
            if (string.IsNullOrWhiteSpace(context.Arguments.FileOutput))
            {
                "Output file path not provided. Skipping execution".LogDebug(this);

                return BatchExecutionResult.CreateSuccessResult();
            }

            try
            {
                "Will save result to: {0}".LogDebug(context.Arguments.FileOutput);

                var content = JsonConvert.SerializeObject(context.BatchResult, Formatting.Indented);

                content.LogTraceWithFormat(this, "Generated content: {0}");
                
                await _fileSystem.File_WriteAllTextAsync(context.Arguments.FileOutput, content, Encoding.UTF8);
                throw new Exception("DUPA!");
                
            }
            catch (System.Exception e)
            {
                "Error while processing batch".LogError(this, e);

                return BatchExecutionResult.CreateFailResult();

            }

            return BatchExecutionResult.CreateSuccessResult();
        }

    }
}
