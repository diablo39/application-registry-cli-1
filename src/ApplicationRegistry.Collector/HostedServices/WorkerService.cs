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
        readonly IHost _host;

        public WorkerService(BatchContext batchContext, BatchRunner batchRunner, ILoggerFactory loggerFactory, IHost host)
        {
            this._host = host;
            LoggerHelper.LoggerFactory = loggerFactory;

            _batchRunner = batchRunner;
            _batchContext = batchContext;
        }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _batchRunner.RunBatchesAsync(_batchContext);
        }
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

    }
}
