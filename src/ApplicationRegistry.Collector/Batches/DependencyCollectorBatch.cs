using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches
{
    class DependencyCollectorBatch<T> : IBatch
        where T : IDependencyCollector
    {
        public Task<BatchResult> ProcessAsync(BatchContext context)
        {
            return Task.FromResult(BatchResult.CreateSuccessResult());
        }
    }
}
