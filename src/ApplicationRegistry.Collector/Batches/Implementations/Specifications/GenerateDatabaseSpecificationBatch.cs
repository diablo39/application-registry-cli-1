using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches.Implementations.Specifications
{
    class GenerateDatabaseSpecificationBatch : IBatch
    {
        public Task<BatchExecutionResult> ProcessAsync(BatchContext context)
        {
            return Task.FromResult(BatchExecutionResult.CreateSuccessResult());
        }
    }
}
