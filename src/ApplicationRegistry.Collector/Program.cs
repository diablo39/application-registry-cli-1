using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using ApplicationRegistry.Collector.Batches;
using ApplicationRegistry.Collector.Batches.Implementations;
using ApplicationRegistry.Collector.Wrappers;
using ApplicationRegistry.Collector.Batches.Implementations.Dependencies;

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
                        .AddTransient<ResultToFileSaveBatch>()
                        .AddTransient<ResultToHttpEndpointBatch>()
                        .AddTransient<CollectApplicationInfoBatch>()
                        .AddTransient<GenerateSwaggerSpecificationBatch>()
                        .AddTransient<CollectNugetDependenciesBatch>()
                        .AddTransient<CollectAutorestClientDependenciesBatch>();

                    services
                        .AddSingleton(PhysicalConsole.Singleton)
                        .AddTransient<IEnumerable<IBatch>>(s => new List<IBatch> {
                            s.GetRequiredService<SanitazeApplicationArgumentsBatch>(),
                            s.GetRequiredService<CollectApplicationInfoBatch>(),
                            //s.GetRequiredService<GenerateSwaggerSpecificationBatch>(),
                            //s.GetRequiredService<CollectNugetDependenciesBatch>(),
                            s.GetRequiredService<CollectAutorestClientDependenciesBatch>(),
                            //s.GetRequiredService<DependencyCollectorBatch<AutorestClientDependencyCollector>>(),
                            //s.GetRequiredService<SpecificationGeneratorBatch<SwaggerSpecificationGenerator>>(),
                            s.GetRequiredService<ResultToFileSaveBatch>(),
                            //s.GetRequiredService<ResultToHostSendBatch>(),
                        });
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder
                        .AddConsole(options => { options.IncludeScopes = true; });

                    builder.SetMinimumLevel(LogLevel.Trace);
                })
                .RunCommandLineApplicationAsync<Worker>(args)
                .GetAwaiter()
                .GetResult();

        }
    }

}
