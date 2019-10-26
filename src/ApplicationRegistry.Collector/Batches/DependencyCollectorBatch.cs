namespace ApplicationRegistry.Collector.Batches
{
    class DependencyCollectorBatch<T> : IBatch
        where T : IDependencyCollector
    {
        public BatchResult Process(BatchContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
