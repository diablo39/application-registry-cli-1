using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static ApplicationRegistry.Collector.BatchExecutionResult;

namespace ApplicationRegistry.Collector.Batches
{
    class BatchRunner
    {
        private readonly IEnumerable<IBatch> _batches;
        readonly IHost _host;

        public BatchRunner(IEnumerable<IBatch> batches, IHost host = null)
        {
            this._host = host;
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
                Exception exceptionThrown = null;
                BatchExecutionResult batchResult;
                try
                {
                    batchResult = await batch.ProcessAsync(context);
                }
                catch (Exception ex)
                {
                    exceptionThrown = ex;
                    batchResult = BatchExecutionResult.CreateFailResult();
                    
                }

                batchActivity.Stop();

                context.BatchResult.ExecutionDuration = ((long)(DateTime.UtcNow - activity.StartTimeUtc).TotalSeconds);

                switch (batchResult.Result)
                {
                    case ExecutionResult.Success:
                        context.BatchResult.BatchStatuses.Add(batch.GetType().Name, true);
                        break;
                    case ExecutionResult.Error:
                        context.BatchResult.BatchStatuses.Add(batch.GetType().Name, false);
                        context.BatchResult.ExecutionSucceeded = false;
                        "Batch: {0} exited with error. Spikking batch and moving further".LogWarning(this, batch.GetType().ToString());
                        break;
                    case ExecutionResult.Fail:
                        context.BatchResult.BatchStatuses.Add(batch.GetType().Name, false);
                        context.BatchResult.ExecutionSucceeded = false;
                        if (exceptionThrown != null)
                        {
                            "Batch processing failed. Batch: {0} thrown exception. Stop processing.".LogError(this, exceptionThrown, batch.GetType().ToString());
                        }
                        else
                        {
                            "Batch processing failed. Batch: {0} failed and further processing can't be continued".LogError(this, batch.GetType().ToString());
                        }

                        break;
                }

                "Finished {0} with result: {1}{2}Execution took: {3}".LogInfo(this, batchName, batchResult.Result, Environment.NewLine, batchActivity.Duration);
            }

            activity.Stop();

            "Processing of all batches finished after: {0}".LogInfo(this, activity.Duration);
            if (_host != null)
            {
                await _host.StopAsync();
            }
        }

        private void PrintBatches()
        {
            var batches = string.Join("", _batches.Select(e => string.Concat(Environment.NewLine, "--> ", e.GetType().Name)));

            "Batches loaded: {0}".LogInfo(this, batches);
        }
    }
}
