using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches
{
    class DependencyCollectorBatch<T> : IBatch
        where T : IDependencyCollector
    {
        private readonly T _dependencyCollector;

        public DependencyCollectorBatch(T dependencyCollector)
        {
            _dependencyCollector = dependencyCollector;
        }

        public Task<BatchExecutionResult> ProcessAsync(BatchContext context)
        {
            try
            {
                var dependencies = _dependencyCollector.GetDependencies();
                context.BatchResult.Dependencies.AddRange(dependencies);
            }
            catch (System.Exception ex)
            {
                // TODO: Log error
                return Task.FromResult(BatchExecutionResult.CreateErrorResult());
            }
            return Task.FromResult(BatchExecutionResult.CreateSuccessResult());
        }
    }
}
