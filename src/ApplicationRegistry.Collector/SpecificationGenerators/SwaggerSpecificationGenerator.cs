using ApplicationRegistry.Collector.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using Resources = ApplicationRegistry.Collector.Properties.Resources;

namespace ApplicationRegistry.Collector.SpecificationGenerators
{
    class SwaggerSpecificationGenerator : ISpecificationGenerator
    {
        private readonly string _projectFilePath;
        private readonly string _projectDirectory;
        private readonly ILogger<SwaggerSpecificationGenerator> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public SwaggerSpecificationGenerator(BatchContext context, ILoggerFactory loggerFactory)
        {
            _projectFilePath = context.Arguments.ProjectFilePath;
            _projectDirectory = new FileInfo(_projectFilePath).Directory.FullName;
            _logger = loggerFactory.CreateLogger<SwaggerSpecificationGenerator>();
            _loggerFactory = loggerFactory;
        }

        public List<ApplicationVersionSpecification> GetSpecifications()
        {
            _logger.LogDebug("Starting swagger generation process");
            using (var project = new DotNetProject(_loggerFactory.CreateLogger<DotNetProject>(), _projectFilePath))
            {
                _logger.LogDebug("Starting standard build of the application");
                project.Build();
                _logger.LogDebug("Standard build finished");

                _logger.LogDebug("Adding new main function");
                project.AddFile("ApplicationRegistryProgram.cs", Resources.ApplicationRegistryProgram_ignore, true);
                _logger.LogDebug("New main function added");

                project.DisableCompilationForCshtml();

                project.Build("ApplicationRegistry.ApplicationRegistryProgram");
                var filePath = Path.GetTempFileName();

                try
                {
                    project.Run(filePath);
                }
                catch (Exception)
                {
                    throw;
                }

                if (!File.Exists(filePath)) return new List<ApplicationVersionSpecification>();

                var swagger = File.ReadAllText(filePath);

                return new List<ApplicationVersionSpecification>() {
                    new ApplicationVersionSpecification {
                        ContentType = "application/json",
                        SpecificationType = "Swagger",
                        Specification = swagger,
                        Code = "Swagger"
                    }
                };
            }
        }
    }
}
