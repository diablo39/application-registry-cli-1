using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector
{
    interface IBatch
    {
        Task<BatchExecutionResult> ProcessAsync(BatchContext context);
    }

    abstract class Batch
    {
        private ILogger _logger;

        public Batch(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("Dupa :)");
        }
    }
}
