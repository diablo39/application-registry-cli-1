namespace ApplicationRegistry.Collector.Batches
{
    class SpecificationGeneratorBatch<T> : IBatch
        where T : ISpecificationGenerator
    {
        public BatchResult Process(BatchContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
