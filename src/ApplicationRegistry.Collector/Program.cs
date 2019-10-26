using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using ApplicationRegistry.Collector.Batches;
using ApplicationRegistry.Collector.SpecificationGenerators;
using ApplicationRegistry.Collector.DependencyCollectors;

namespace ApplicationRegistry.Collector
{
    class Program
    {
        public static int Main(string[] args)
        {
            return new HostBuilder()
                .ConfigureLogging((context, builder) =>
                {
                    builder
                        .AddConsole();
                })
                .ConfigureServices((context, services) =>
                {
                    services // Add dependency collectors
                        .AddTransient<NugetDependencyCollector>()
                        .AddTransient<AutorestClientDependencyCollector>();
         
                    services // Add specification generators
                        .AddTransient<SwaggerSpecificationGenerator>()
                        .AddTransient<DatabaseScpecificationGenerator>();
        

                    services
                        .AddSingleton(new BatchContext())
                        .AddTransient<SanitazeApplicationArgumentsBatch>()
                        .AddTransient<SpecificationGeneratorBatch<SwaggerSpecificationGenerator>>()
                        .AddTransient<DependencyCollectorBatch<AutorestClientDependencyCollector>>()
                        .AddTransient<DependencyCollectorBatch<NugetDependencyCollector>>()
                        .AddTransient<ResultToFileSaveBatch>()
                        .AddTransient<ResultToHostSendBatch>();

                    services
                        .AddSingleton(PhysicalConsole.Singleton)
                        .AddTransient<IEnumerable<IBatch>>(s => new List<IBatch> {
                            s.GetRequiredService<SanitazeApplicationArgumentsBatch>(),
                            s.GetRequiredService<DependencyCollectorBatch<NugetDependencyCollector>>(),
                            s.GetRequiredService<DependencyCollectorBatch<AutorestClientDependencyCollector>>(),
                            s.GetRequiredService<SpecificationGeneratorBatch<SwaggerSpecificationGenerator>>(),
                            s.GetRequiredService<ResultToFileSaveBatch>(),
                            s.GetRequiredService<ResultToHostSendBatch>(),
                        });
                })
                .RunCommandLineApplicationAsync<Worker>(args)
                .GetAwaiter()
                .GetResult();
        }
    }

}
