using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches
{
    class SpecificationGeneratorBatch<T> : IBatch
        where T : ISpecificationGenerator
    {
        public Task<BatchResult> ProcessAsync(BatchContext context)
        {
            return Task.FromResult(BatchResult.CreateSuccessResult());
        }
    }
}
