using ApplicationRegistry.Collector.Batches;
using ApplicationRegistry.Collector.Model;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector
{
    class WorkerService: IHostedService
    {

        private readonly BatchRunner _batchRunner;
        private readonly BatchContext _batchContext;

        public WorkerService(BatchContext batchContext, BatchRunner batchRunner, ILoggerFactory loggerFactory)
        {
            LoggerHelper.LoggerFactory = loggerFactory;

            _batchRunner = batchRunner;
            _batchContext = batchContext;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _batchRunner.RunBatchesAsync(_batchContext);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

    }
}
