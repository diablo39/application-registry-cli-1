using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches
{
    class SpecificationGeneratorBatch<T> : IBatch
        where T : ISpecificationGenerator
    {
        public Task<BatchExecutionResult> ProcessAsync(BatchContext context)
        {
            return Task.FromResult(BatchExecutionResult.CreateSuccessResult());
        }
    }
}
