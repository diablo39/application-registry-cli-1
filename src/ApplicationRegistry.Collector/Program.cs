using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;
using ApplicationRegistry.Collector.DependencyCollectors;
using ApplicationRegistry.Collector.SpecificationGenerators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using ApplicationRegistry.Collector.Model;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationRegistry.Collector
{
    class Program
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
        [Option("-p|--path <PATH>", "Path to the project file, or directory where single csproj file is located", CommandOptionType.SingleValue)]
        public string ProjectFilePath { get; set; }

        [Option("--output-file <PATH>", "Path to output file", CommandOptionType.SingleValue)]
        public string FileOutput { get; set; }

        [Url]
        [Option("--output-url <URL>", "Url to Application Registry", CommandOptionType.SingleValue)]
        public Uri Url { get; set; }

        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        private static string _newLine = System.Environment.NewLine;

        public async Task<int> OnExecute()
        {
            ProjectFilePath = ProjectFilePath.TrimEnd('"');
            ProjectFilePath = System.IO.Path.GetFullPath(ProjectFilePath);

            if(!ProjectFilePath.EndsWith(".csproj")) // directory provided
            {
                var files = Directory.EnumerateFiles(ProjectFilePath, "*.csproj").ToList();

                if(files.Count == 0)
                {
                    Console.Error.WriteLine("Project file not found");
                    return -1;
                }

                if(files.Count > 1)
                {
                    Console.Error.WriteLine("Error: More than one project found in directory {0}", ProjectFilePath);
                    Console.Error.WriteLine("Set full path to the prject file");

                    return -1;
                }

                ProjectFilePath = files[0];
            }

            if (string.IsNullOrWhiteSpace(SolutionFilePath))
            {
                var success = false;
                success = FindSolutionFile(success);

                if (!success)
                {
                    return -1;
                }
            }

            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger<Program>>();

            try
            {
                await RunCollectorAsync(serviceProvider);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Execution failed");

                if (Url != null)
                {
                    var client = new HttpClient();
                    client.BaseAddress = Url;

                    await client.PostAsJsonAsync("/api/v1/Collector/Error", new CollectorError
                    {
                        ApplicationCode = Applicatnion,
                        ErrorMessage = ex.Message + System.Environment.NewLine + ex.StackTrace,
                        IdEnvironment = Environment,
                        Version = Version
                    });
                }
            }

            return 0;
        }

        private bool FindSolutionFile(bool success)
        {
            var directory = new FileInfo(ProjectFilePath).Directory;
            do
            {
                var files = directory.GetFiles("*.sln");

                if (files.Length == 1)
                {
                    SolutionFilePath = files[0].FullName;
                    success = true;
                    break;
                }

                if (files.Length > 1)
                {
                    Console.Error.WriteLine("More than one solution file found.");
                    Console.Error.WriteLine("Found:");
                    for (int i = 0; i < files.Length; i++)
                    {
                        Console.Error.WriteLine($"* {files[i].FullName}");
                    }
                    Console.Error.WriteLine("Use parametr --solution to specify correct solution file");
                    success = false;
                    break;
                }


                directory = directory.Parent;
            }
            while (directory != null);
            return success;
        }

        private async Task RunCollectorAsync(ServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILogger<Program>>();

            logger.LogInformation($"Starting application with parameters: {_newLine}\turl: {Url}{_newLine}\tApplication: {Applicatnion}{_newLine}\tPath: {this.ProjectFilePath}");

            logger.LogTrace("Starting specification generation");

            var specifications = new List<ApplicationVersionSpecification>();

            using (var loggerScope = logger.BeginScope("SpecificationGeneration"))
            {
                var specificationGenerators = serviceProvider.GetRequiredService<ISpecificationGenerator[]>();
                for (int i = 0; i < specificationGenerators.Length; i++)
                {
                    var specificationGenerator = specificationGenerators[i];
                    logger.LogDebug("Starting {0}", specificationGenerator.GetType());
                    var specificationsGenerated = specificationGenerator.GetSpecifications();
                    logger.LogDebug("Ending {0}", specificationGenerator.GetType());
                    specifications.AddRange(specificationsGenerated);
                }
            }


            logger.LogTrace("Ending: specifications generation");

            var dependencies = serviceProvider.GetService<NugetDependencyCollector>().GetDependencies();
            var autorestclientdependencies = serviceProvider.GetService<AutorestClientDependencyCollector>().GetDependencies();
            dependencies.AddRange(autorestclientdependencies);

            var result = new ApplicationVersion
            {
                ApplicationCode = Applicatnion,
                IdEnvironment = Environment,
                Version = Version,
                Dependencies = dependencies,
                Specifications = specifications
            };

            if (!string.IsNullOrWhiteSpace(FileOutput))
            {
                File.WriteAllText(FileOutput, JsonConvert.SerializeObject(result, Formatting.Indented));
            }

            if (Url != null)
            {
                var client = new HttpClient();
                client.BaseAddress = Url;

                var postResult = await client.PostAsJsonAsync("/api/v1/collector", result);
                if (!postResult.IsSuccessStatusCode)
                {
                    Console.WriteLine("Error occured. Response from the server:");

                    var responseTest = await postResult.Content.ReadAsStringAsync();
                    Console.WriteLine(responseTest);
                }
            }

            Console.WriteLine();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services
                .AddLogging(configure => configure.AddConsole())
                .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Trace);

            services.AddOptions<ApplicationOptions>().Configure(options =>
            {
                options.ProjectFilePath = ProjectFilePath;
                options.SolutionFilePath = SolutionFilePath;
            });


            services.AddTransient<SwaggerSpecificationGenerator>();
            services.AddTransient<NugetDependencyCollector>();
            services.AddTransient<AutorestClientDependencyCollector>();

            services.AddTransient(s =>
            {
                return new ISpecificationGenerator[]
                {
                    s.GetRequiredService<SwaggerSpecificationGenerator>()
                };
            });

            services.AddTransient(s => new IDependencyCollector[]
            {
                s.GetRequiredService<NugetDependencyCollector>(),
                s.GetRequiredService<AutorestClientDependencyCollector>()
            });
        }
    }

}
