using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches.Implementations
{
    class CollectApplicationInfoBatch : IBatch
    {
        public Task<BatchExecutionResult> ProcessAsync(BatchContext context)
        {
            context.BatchResult.ApplicationCode = context.Arguments.Applicatnion;
            context.BatchResult.IdEnvironment = context.Arguments.Environment;
            context.BatchResult.Version = context.Arguments.Version;

            context.BatchResult.ToolsVersion = typeof(Program).Assembly.GetName().Version.ToString();

            return Task.FromResult(BatchExecutionResult.CreateSuccessResult());
        }
    }
}
