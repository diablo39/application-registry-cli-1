using ApplicationRegistry.Collector.Model;
using ApplicationRegistry.Collector.Properties;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches.Implementations
{
    class GenerateSwaggerSpecificationBatch : IBatch
    {
        private readonly ILoggerFactory _loggerFactory;

        public GenerateSwaggerSpecificationBatch(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public Task<BatchExecutionResult> ProcessAsync(BatchContext context)
        {
            try
            {
                var swagger = GetSwaggerSpecification(context.Arguments.ProjectFilePath);
                context.BatchResult.Specifications.AddRange(swagger);
            }
            catch (Exception)
            {
                return Task.FromResult(BatchExecutionResult.CreateErrorResult());
            }

            return Task.FromResult(BatchExecutionResult.CreateSuccessResult());
        }

        private List<ApplicationVersionSpecification> GetSwaggerSpecification(string projectFilePath)
        {
            using (var project = new DotNetProject(_loggerFactory.CreateLogger<DotNetProject>(), projectFilePath))
            {
                "Starting standard build of the application".LogDebug(this);
                project.Build();
                "Standard build finished".LogDebug(this);

                "Adding new main function".LogDebug(this);
                project.AddFile("ApplicationRegistryProgram.cs", Resources.ApplicationRegistryProgram_ignore, true);
                "New main function added".LogDebug(this);

                project.DisableCompilationForCshtml();

                project.Build("ApplicationRegistry.ApplicationRegistryProgram");

                var filePath = Path.GetTempFileName();

                try
                {
                    project.Run(filePath);
                }
                catch (Exception)
                {
                    "Running swagger generator failed".LogError(this);

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
