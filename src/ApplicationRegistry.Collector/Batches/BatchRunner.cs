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
                "Starting {0}".LogInfo(this, batchName);
                var batchActivity = new Activity(batchName + " Execution").Start();
                var batchResult = await batch.ProcessAsync(context);
                batchActivity.Stop();

                "Finished {0} with result: {1}{2}Execution took: {3}".LogInfo(this, batchName, batchResult.Result, Environment.NewLine, batchActivity.Duration);
            }

            activity.Stop();

            "Processing of all batches finished after: {0}".LogInfo(this, activity.Duration);
        }

        private void PrintBatches()
        {
            var batches = string.Join("", _batches.Select(e => string.Concat(Environment.NewLine, "--> ", e.GetType().Name)));

            _logger.LogInformation("Batches loaded: {0}", batches);
        }
    }
}
