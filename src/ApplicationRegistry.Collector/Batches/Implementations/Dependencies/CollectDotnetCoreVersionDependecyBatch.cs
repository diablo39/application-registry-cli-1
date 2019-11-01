using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches.Implementations.Dependencies
{
    class CollectDotnetCoreVersionDependecyBatch : IBatch
    {
        public Task<BatchExecutionResult> ProcessAsync(BatchContext context)
        {
            return Task.FromResult(BatchExecutionResult.CreateSuccessResult());
        }
    }
}
