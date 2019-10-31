using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using ApplicationRegistry.Collector.Batches;
using ApplicationRegistry.Collector.SpecificationGenerators;
using ApplicationRegistry.Collector.DependencyCollectors;
using ApplicationRegistry.Collector.Batches.Implementations;
using ApplicationRegistry.Collector.Wrappers;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector
{
    public class Startup : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class Startup2 : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

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

                    services // Add dependency collectors
                        .AddTransient<NugetDependencyCollector>()
                        .AddTransient<AutorestClientDependencyCollector>();

                    services // Add specification generators
                        .AddTransient<SwaggerSpecificationGenerator>()
                        .AddTransient<DatabaseScpecificationGenerator>();

                    services
                        .AddTransient<BatchRunner>()
                        .AddTransient<SanitazeApplicationArgumentsBatch>()
                        .AddTransient<SpecificationGeneratorBatch<SwaggerSpecificationGenerator>>()
                        .AddTransient<DependencyCollectorBatch<AutorestClientDependencyCollector>>()
                        .AddTransient<DependencyCollectorBatch<NugetDependencyCollector>>()
                        .AddTransient<ResultToFileSaveBatch>()
                        .AddTransient<ResultToHostSendBatch>()
                        .AddTransient<CollectApplicationInfoBatch>();

                    services
                        .AddSingleton(PhysicalConsole.Singleton)
                        .AddTransient<IEnumerable<IBatch>>(s => new List<IBatch> {
                            s.GetRequiredService<SanitazeApplicationArgumentsBatch>(),
                            s.GetRequiredService<CollectApplicationInfoBatch>(),
                            //s.GetRequiredService<DependencyCollectorBatch<NugetDependencyCollector>>(),
                            //s.GetRequiredService<DependencyCollectorBatch<AutorestClientDependencyCollector>>(),
                            //s.GetRequiredService<SpecificationGeneratorBatch<SwaggerSpecificationGenerator>>(),
                            s.GetRequiredService<ResultToFileSaveBatch>(),
                            //s.GetRequiredService<ResultToHostSendBatch>(),
                        });

                    services.AddHostedService<Startup>();
                    services.AddHostedService<Startup2>();
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
