using ApplicationRegistry.Collector.Model;
using ApplicationRegistry.Collector.Properties;
using ApplicationRegistry.Collector.Wrappers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches.Implementations.Specifications
{
    class GenerateSwaggerSpecificationBatch : IBatch
    {
        readonly FileSystem _fileSystem;
        public GenerateSwaggerSpecificationBatch(FileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public Task<BatchExecutionResult> ProcessAsync(BatchContext context)
        {
            try
            {
                var swagger = GetSwaggerSpecification(context.Arguments.ProjectFilePath);
                context.BatchResult.Specifications.AddRange(swagger);
            }
            catch (Exception ex)
            {
                "Running swagger generator failed".LogError(this, ex);
                return Task.FromResult(BatchExecutionResult.CreateErrorResult());
            }

            return Task.FromResult(BatchExecutionResult.CreateSuccessResult());
        }

        private List<ApplicationVersionSpecification> GetSwaggerSpecification(string projectFilePath)
        {
            using (var project = new DotNetProject(projectFilePath))
            {
                var frameworkVersion = project.FrameworkVersion;
                $"Framework version: {frameworkVersion}".LogDebug(this);

                "Starting standard build of the application".LogDebug(this);
                project.Build();

                if(frameworkVersion.StartsWith("netcoreapp")) {
                    "Adding new main function (.net core 2.1)".LogDebug(this);
                    project.AddFile("ApplicationRegistryProgram.cs", Resources.ApplicationRegistryProgram_ignore, true);
                }
                else {
                    "Adding new main function (.net 5.0)".LogDebug(this);
                    project.AddFile("ApplicationRegistryProgram.cs", Resources.ApplicationRegistryProgram_ignore_net5_0, true);
                }
                
                project.DisableCompilationForCshtml();
                "Starting build with custom Program class".LogDebug(this);
                project.Build("ApplicationRegistry.ApplicationRegistryProgram");
                
                var filePath = Path.GetTempFileName();

                project.Run(filePath);

                if (!_fileSystem.FileExists(filePath)) return new List<ApplicationVersionSpecification>();

                var swagger = _fileSystem.ReadAllText(filePath);

                _fileSystem.DeleteFile(filePath);

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
