using System;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches
{
    class ResultToHostSendBatch : IBatch
    {
        public Task<BatchResult> ProcessAsync(BatchContext context)
        {
            return Task.FromResult(BatchResult.CreateSuccessResult());
        }
    }
}
