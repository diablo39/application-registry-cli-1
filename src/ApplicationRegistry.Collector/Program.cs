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
                    services.AddTransient<NugetDependencyCollector>();
                    services.AddTransient<DependencyCollectorBatch<NugetDependencyCollector>>();

                    services.AddTransient<AutorestClientDependencyCollector>();
                    services.AddTransient<DependencyCollectorBatch<AutorestClientDependencyCollector>>();

                    services.AddTransient<SwaggerSpecificationGenerator>();
                    services.AddTransient<SpecificationGeneratorBatch<SwaggerSpecificationGenerator>>();

                    services
                        .AddSingleton(PhysicalConsole.Singleton)
                        .AddSingleton(new BatchContext())
                        .AddTransient<SanitazeApplicationArgumentsBatch>()
                        .AddTransient<IEnumerable<IBatch>>(s => new List<IBatch> {
                            s.GetRequiredService<SanitazeApplicationArgumentsBatch>(),
                            s.GetRequiredService<DependencyCollectorBatch<NugetDependencyCollector>>(),
                            s.GetRequiredService<DependencyCollectorBatch<AutorestClientDependencyCollector>>(),
                            s.GetRequiredService<SpecificationGeneratorBatch<SwaggerSpecificationGenerator>>(),
                        });
                })
                .RunCommandLineApplicationAsync<Worker>(args)
                .GetAwaiter()
                .GetResult();
        }
    }

}
