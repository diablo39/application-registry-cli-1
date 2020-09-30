using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using ApplicationRegistry.Collector.Batches;
using ApplicationRegistry.Collector.Batches.Implementations;
using ApplicationRegistry.Collector.Wrappers;
using ApplicationRegistry.Collector.Batches.Implementations.Dependencies;
using ApplicationRegistry.Collector.Batches.Implementations.ResultSenders;
using ApplicationRegistry.Collector.Batches.Implementations.Specifications;
using System.ComponentModel.DataAnnotations;
using System;
using System.Threading.Tasks;
using ApplicationRegistry.BackendHttpClient;

namespace ApplicationRegistry.Collector
{
    partial class Program
    {
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


        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);


        public Task<int> OnExecute()
        {
            var arguments = new BatchProcessArgumentsFactory().Create(Applicatnion, Environment, FileOutput, ProjectFilePath, SolutionFilePath, Url, Version);

            if (arguments == null)
            {
                return Task.FromResult(0);
            }

            var batchContext = new BatchContext(arguments);

            new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging();

                    services.AddSingleton(batchContext);

                    services.AddSingleton<FileSystem>();

                    services.AddTransient<ServerClient>();

                    services
                        .AddTransient<BatchRunner>()
                        .AddTransient<ValidateArgumentsBatch>()
                        .AddTransient<CollectApplicationInfoBatch>()

                        .AddTransient<GetReadmeBatch>()
                        .AddTransient<GenerateSwaggerSpecificationBatch>()
                        .AddTransient<GenerateDatabaseSpecificationBatch>()

                        .AddTransient<CollectNugetDependenciesBatch>()
                        .AddTransient<CollectAutorestClientDependenciesBatch>()
                        .AddTransient<CollectDotnetCoreVersionDependecyBatch>()

                        .AddTransient<ResultToFileSaveBatch>()
                        .AddTransient<ResultToHttpEndpointBatch>();

                    services
                        .AddSingleton(PhysicalConsole.Singleton)
                        .AddTransient<IEnumerable<IBatch>>(s => new List<IBatch> {
                            s.GetRequiredService<ValidateArgumentsBatch>(),
                            s.GetRequiredService<CollectApplicationInfoBatch>(),

                            s.GetRequiredService<GetReadmeBatch>(),
                            s.GetRequiredService<GenerateSwaggerSpecificationBatch>(),
                            s.GetRequiredService<GenerateDatabaseSpecificationBatch>(),

                            s.GetRequiredService<CollectDotnetCoreVersionDependecyBatch>(),
                            s.GetRequiredService<CollectNugetDependenciesBatch>(),
                            s.GetRequiredService<CollectAutorestClientDependenciesBatch>(),

                            s.GetRequiredService<ResultToFileSaveBatch>(),
                            s.GetRequiredService<ResultToHttpEndpointBatch>(),
                        });
                    services.AddTransient<ServerClient>((s) => new ServerClient(new System.Net.Http.HttpClient { BaseAddress = Url }));

                })
                .ConfigureServices((host, services) =>
                {
                    services.AddHostedService<WorkerService>();
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddConsole(options => { options.IncludeScopes = false; });

                    builder.SetMinimumLevel(LogLevel.Trace);
                })
                .Build()
                .Run();

            Console.WriteLine("Application is closed");

            return Task.FromResult(0);
        }

    }
}
