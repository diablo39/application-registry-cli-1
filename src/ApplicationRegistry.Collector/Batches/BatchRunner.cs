using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches
{
    class BatchRunner
    {
        private readonly ILogger<BatchRunner> _logger;
        private readonly IEnumerable<IBatch> _batches;

        public BatchRunner(IEnumerable<IBatch> batches, ILogger<BatchRunner> logger)
        {
            _logger = logger;
            _batches = batches;
        }

        public async Task RunBatchesAsync(BatchContext context)
        {
            PrintBatches();
            var activity = new Activity("Batches running").Start();
            
            foreach (var batch in _batches)
            {
                var batchName = batch.GetType().Name;
                _logger.LogInformation("Starting {0}", batchName);
                var batchActivity = new Activity(batchName + " Execution").Start();
                await batch.ProcessAsync(context);
                batchActivity.Stop();

                "Finished {0}. \tExecution took: {1}".LogDebug(this, batchName, batchActivity.Duration);
            }

            activity.Stop();

            _logger.LogInformation("Processing all batches finished after: {0}", activity.Duration);
        }

        private void PrintBatches()
        {
            var batches = string.Join(", ", _batches.Select(e => e.GetType().Name));

            _logger.LogInformation("Batches loaded: {0}", batches);
        }
    }
}
