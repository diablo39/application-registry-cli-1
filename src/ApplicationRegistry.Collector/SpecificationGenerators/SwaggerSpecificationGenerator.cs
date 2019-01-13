using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ApplicationRegistry.Collector.Model;
using ApplicationRegistry.Collector.Properties;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Resources = ApplicationRegistry.Collector.Properties.Resources;

namespace ApplicationRegistry.Collector.SpecificationGenerators
{
    class SwaggerSpecificationGenerator : ISpecificationGenerator
    {
        private readonly string _projectFilePath;
        private readonly string _projectDirectory;
        private readonly string _swaggerdoc;

        public SwaggerSpecificationGenerator(IOptions<ApplicationOptions> options)
        {
            _projectFilePath = options.Value.ProjectFilePath;
            _projectDirectory = new FileInfo(_projectFilePath).Directory.FullName;
            _swaggerdoc = options.Value.SwaggerDoc; 
        }

        public List<ApplicationVersionSpecification> GetSpecifications()
        {
            using (var project = new DotNetProject(_projectFilePath))
            {

                //var outputFilePath = "swagger.json";
                //project.AddDotNetCliToolReference("Swashbuckle.AspNetCore.Cli", "3.0.0-beta1");
                //var command = string.Concat("dotnet swagger tofile --output ", outputFilePath, " \"$(OutputPath)$(AssemblyName).dll\" \"", _swaggerdoc, "\"");
                //project.AddAfterBuildCommand("SwaggerGeneration", command);

                project.AddFile("ApplicationRegistryProgram.cs", Resources.ApplicationRegistryProgram, true);
                project.Build("ApplicationRegistry.ApplicationRegistryProgram");
                string swagger = project.Run(_swaggerdoc);

                return new List<ApplicationVersionSpecification>() {
                    new ApplicationVersionSpecification {
                        ContentType = "application/json",
                        SpecificationType = "Swagger",
                        Specification = swagger,
                        Code = _swaggerdoc
                    }
                };
            }
        }
    }
}
