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

namespace ApplicationRegistry.Collector
{

    class Program
    {
        public static int Main(string[] args)
        {
            return new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddLogging();

                    services.AddSingleton<FileSystem>();

                    services
                        .AddTransient<BatchRunner>()
                        .AddTransient<SanitazeApplicationArgumentsBatch>()
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
                            s.GetRequiredService<SanitazeApplicationArgumentsBatch>(),
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
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddConsole(options => { options.IncludeScopes = false; });

                    builder.SetMinimumLevel(LogLevel.Trace);
                })
                .RunCommandLineApplicationAsync<Worker>(args)
                .GetAwaiter()
                .GetResult();

        }
    }

}
