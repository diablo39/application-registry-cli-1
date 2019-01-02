using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;
using System.Xml.XPath;
using System.Xml.Linq;
using ApplicationRegistry.Collector.DependencyCollectors;
using ApplicationRegistry.Collector.SpecificationGenerators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using ApplicationRegistry.Collector.Model;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;

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

        [Required]
        [Option("-sd|--swaggerdoc <SWAGGERDOC>", "Swagger Doc", CommandOptionType.SingleValue)]
        public string SwaggerDoc { get; }

        //[Required]
        [Url]
        [Option("-u|--url <URL>", "Url to Application Registry", CommandOptionType.SingleValue)]
        public Uri Url { get; set; }

        [Required]
        [Option("-s|--solution <solution>", "Path to the solution", CommandOptionType.SingleValue)]
        public string SolutionFilePath { get; }

        [Required]
        [Option("-p|--path <PATH>", "Path to the project", CommandOptionType.SingleValue)]
        public string ProjectFilePath { get; }

        [Option("--output-file <PATH>", "Path to output file", CommandOptionType.SingleValue)]
        public string FileOutput { get; set; }

        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        private static string NewLine = System.Environment.NewLine;

        private async Task OnExecute()
        {
            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger<Program>>();

            try
            {
                RunCollector(serviceProvider);
            }
            catch (Exception ex)
            {
                
                logger.LogCritical(ex, "Execution failed");
            }

        }

        private void RunCollector(ServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILogger<Program>>();

            logger.LogInformation($"Starting application with parameters: {NewLine}\turl: {Url}{NewLine}\tApplication: {Applicatnion}");

            logger.LogTrace("Starting specification generation");
            var loggerScope = logger.BeginScope("SpecificationGeneration");

            var specifications = serviceProvider.GetService<SwaggerSpecificationGenerator>().GetSpecifications();
            logger.LogTrace("Swagger generated");
            loggerScope.Dispose();


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

            //var client = new HttpClient();
            //client.BaseAddress = Url;

            //var postResult = await client.PostAsJsonAsync("/api/v1/collector", result);
            //if(!postResult.IsSuccessStatusCode)
            //{
            //    Console.WriteLine("Error occured. Response from the server:");

            //    var responseTest = await postResult.Content.ReadAsStringAsync();
            //    Console.WriteLine(responseTest);
            //}

            Console.WriteLine();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services
                .AddLogging(configure => configure.AddConsole())
                .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Trace);

            services.AddOptions<ApplicationOptions>().Configure(options =>
            {
                options.ProjectFilePath = System.IO.Path.GetFullPath(ProjectFilePath);
                options.SwaggerDoc = SwaggerDoc;
                options.SolutionFilePath = SolutionFilePath;
            });

            services.AddTransient<SwaggerSpecificationGenerator>();

            services.AddTransient<NugetDependencyCollector>();
            services.AddTransient<AutorestClientDependencyCollector>();
        }
    }

}
