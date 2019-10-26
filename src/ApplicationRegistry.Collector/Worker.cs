using ApplicationRegistry.Collector.DependencyCollectors;
using ApplicationRegistry.Collector.Model;
using ApplicationRegistry.Collector.SpecificationGenerators;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector
{
    class Worker
    {
        private readonly string _newLine = System.Environment.NewLine;
        private readonly BatchContext _batchContext;
        private readonly IServiceProvider _serviceProvider;

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
        [Option("--output-url <URL>", "Url to Application Registry", CommandOptionType.SingleValue)]
        public Uri Url { get; set; }

        public Worker(BatchContext batchContext ,IServiceProvider serviceProvider, IHostingEnvironment environment)
        {
            _batchContext = batchContext;
            _serviceProvider = serviceProvider;
        }

        public async Task<int> OnExecute()
        {
            _batchContext.Arguments.Applicatnion = Applicatnion;
            _batchContext.Arguments.Environment = Environment;
            _batchContext.Arguments.FileOutput = FileOutput;
            _batchContext.Arguments.ProjectFilePath = ProjectFilePath;
            _batchContext.Arguments.SolutionFilePath = SolutionFilePath;
            _batchContext.Arguments.Url = Url;
            _batchContext.Arguments.Version = Version;

            var batches = _serviceProvider.GetRequiredService<IEnumerable<IBatch>>();

            foreach (var batch in batches)
            {
                await batch.ProcessAsync(_batchContext);
            }
  
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

       

        private async Task RunCollectorAsync(ServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILogger<Program>>();

            logger.LogInformation($"Starting application with parameters: {_newLine}\turl: {Url}{_newLine}\tApplication: {Applicatnion}{_newLine}\tPath: {this.ProjectFilePath}");

            (List<ApplicationVersionDependency> dependencies, bool collectDependenciesFailed) = CollectDependencies(serviceProvider);


            (List<ApplicationVersionSpecification> specifications, bool specificationGenerationFailed) = GenerateSpecifications(serviceProvider, logger);


            var result = new ApplicationVersion
            {
                ApplicationCode = Applicatnion,
                IdEnvironment = Environment,
                Version = Version,
                Dependencies = dependencies,
                Specifications = specifications,
                SpecificationGenerationFailed = specificationGenerationFailed,
                DependencyFillectionFailed = collectDependenciesFailed,
                ToolsVersion = typeof(Program).Assembly.GetName().Version.ToString()
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

        private static (List<ApplicationVersionDependency> dependencies, bool collectDependenciesFailed) CollectDependencies(ServiceProvider serviceProvider)
        {
            var dependencies = new List<ApplicationVersionDependency>();
            var collectDependenciesFailed = false;
            try
            {
                var nugetDependencies = serviceProvider.GetService<NugetDependencyCollector>().GetDependencies();
                dependencies.AddRange(nugetDependencies);

                var autorestclientdependencies = serviceProvider.GetService<AutorestClientDependencyCollector>().GetDependencies();
                dependencies.AddRange(autorestclientdependencies);
            }
            catch
            {
                collectDependenciesFailed = true;
            }

            return (dependencies, collectDependenciesFailed);
        }

        private static (List<ApplicationVersionSpecification> specifications, bool specificationGenerationFailed) GenerateSpecifications(ServiceProvider serviceProvider, ILogger<Program> logger)
        {
            logger.LogTrace("Starting specification generation");

            var specifications = new List<ApplicationVersionSpecification>();
            var specificationGenerationFailed = false;
            using (var loggerScope = logger.BeginScope("SpecificationGeneration"))
            {
                var specificationGenerators = serviceProvider.GetRequiredService<ISpecificationGenerator[]>();
                for (int i = 0; i < specificationGenerators.Length; i++)
                {
                    try
                    {
                        var specificationGenerator = specificationGenerators[i];
                        logger.LogDebug("Starting {0}", specificationGenerator.GetType());
                        var specificationsGenerated = specificationGenerator.GetSpecifications();
                        logger.LogDebug("Ending {0}", specificationGenerator.GetType());
                        specifications.AddRange(specificationsGenerated);
                    }
                    catch
                    {
                        specificationGenerationFailed = true;
                    }
                }
            }

            logger.LogTrace("Ending: specifications generation");

            return (specifications, specificationGenerationFailed);
        }



    }
}
