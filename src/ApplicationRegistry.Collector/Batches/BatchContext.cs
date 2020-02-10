using ApplicationRegistry.Collector.Model;

namespace ApplicationRegistry.Collector
{
    class BatchContext
    {
        public BatchContext(BatchProcessArguments arguments)
        {
            Arguments = arguments;
        }

        public BatchProcessArguments Arguments { get; }

        public ApplicationInfo BatchResult { get; } = new ApplicationInfo();
    }
}
