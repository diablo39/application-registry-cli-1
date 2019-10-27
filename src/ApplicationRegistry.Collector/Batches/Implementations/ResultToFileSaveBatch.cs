
using ApplicationRegistry.Collector.Wrappers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
                _logger.LogDebug("Output file path not provided. Skipping execution");

                return BatchExecutionResult.CreateSuccessResult();
            }

            try
            {
                _logger.LogDebug("Will save result to: {0}", context.Arguments.FileOutput);

                var content = JsonConvert.SerializeObject(context.BatchResult, Formatting.Indented);

                await _fileSystem.File_WriteAllTextAsync(context.Arguments.FileOutput, content, Encoding.UTF8);
                
            }
            catch (System.Exception e)
            {
                _logger.LogError(e, "Error while processing batch");
                return BatchExecutionResult.CreateFailResult();

            }

            return BatchExecutionResult.CreateSuccessResult();
        }
    }
}
