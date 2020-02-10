
using ApplicationRegistry.Collector.Wrappers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches.Implementations.ResultSenders
{
    internal class ResultToFileSaveBatch : IBatch
    {
        private readonly FileSystem _fileSystem;

        public ResultToFileSaveBatch(FileSystem fileSystem)
        {
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
                var content = JsonConvert.SerializeObject(context.BatchResult, Formatting.Indented);

                await _fileSystem.WriteAllTextAsync(context.Arguments.FileOutput, content, Encoding.UTF8);

                "Content saved to: {0}{1}Generated content:{2}{3}".LogDebug(this,
                    context.Arguments.FileOutput,
                    Environment.NewLine,
                    Environment.NewLine,
                    content);
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
