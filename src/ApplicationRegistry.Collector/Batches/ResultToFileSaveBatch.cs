using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches
{
    class ResultToFileSaveBatch : IBatch
    {
        public Task<BatchResult> ProcessAsync(BatchContext context)
        {
            return Task.FromResult(BatchResult.CreateSuccessResult());
        }
    }
}
