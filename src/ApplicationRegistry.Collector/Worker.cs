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
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector
{
    class Worker
    {

        private readonly BatchRunner _batchRunner;

        [Required]
        [Option("-v|--version <VERSION>", "Version of the application", CommandOptionType.SingleValue)]
        public string Version { get; }

        [Required]
        [Option("-a|--application <APPLICATION>", "Code of the application", CommandOptionType.SingleValue)]
        public string Applicatnion { get; }

        [Required]
        [Option("-e|--env <ENV>", "Environment", CommandOptionType.SingleValue)]
        public string Environment { get; }

        [Option("-s|--solution <solution>", "Path to the solution", CommandOptionType.SingleValue)]
        public string SolutionFilePath { get; set; }

        [Required]
        [FileOrDirectoryExists]
        [Option("-p|--path <PATH>", "Path to the project file, or directory where single csproj file is located", CommandOptionType.SingleValue)]
        public string ProjectFilePath { get; set; }

        [Option("--output-file <PATH>", "Path to output file", CommandOptionType.SingleValue)]
        public string FileOutput { get; set; }

        [Url]
        [Option("-u|--output-url <URL>", "Url to Application Registry", CommandOptionType.SingleValue)]
        public Uri Url { get; set; }

        public Worker(BatchRunner batchRunner, ILoggerFactory loggerFactory)
        {
            LoggerHelper.LoggerFactory = loggerFactory;

            _batchRunner = batchRunner;
        }

        public async Task<int> OnExecute()
        {
            var arguments = new BatchProcessArguments
            {
                Applicatnion = Applicatnion,
                Environment = Environment,
                FileOutput = FileOutput,
                ProjectFilePath = ProjectFilePath,
                SolutionFilePath = SolutionFilePath,
                Url = Url,
                Version = Version
            };

            var batchContext = new BatchContext(arguments);

            await _batchRunner.RunBatchesAsync(batchContext);

            //try
            //{
            //    await RunCollectorAsync(serviceProvider);
            //}
            //catch (Exception ex)
            //{
            //    logger.LogCritical(ex, "Execution failed");

            //    if (Url != null)
            //    {
            //        var client = new HttpClient();
            //        client.BaseAddress = Url;

            //        await client.PostAsJsonAsync("/api/v1/Collector/Error", new CollectorError
            //        {
            //            ApplicationCode = Applicatnion,
            //            ErrorMessage = ex.Message + System.Environment.NewLine + ex.StackTrace,
            //            IdEnvironment = Environment,
            //            Version = Version
            //        });
            //    }
            //}

            return 0;
        }
    }
}
