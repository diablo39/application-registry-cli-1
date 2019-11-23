using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector
{
    interface IBatch
    {
        Task<BatchExecutionResult> ProcessAsync(BatchContext context);
    }
}
