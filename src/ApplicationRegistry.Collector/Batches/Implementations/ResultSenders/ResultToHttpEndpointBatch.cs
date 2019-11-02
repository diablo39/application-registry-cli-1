using System;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches
{
    class ResultToHttpEndpointBatch : IBatch
    {
        public Task<BatchExecutionResult> ProcessAsync(BatchContext context)
        {
            return Task.FromResult(BatchExecutionResult.CreateSuccessResult());
        }
    }
}
