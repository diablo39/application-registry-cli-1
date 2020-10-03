using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace ApplicationRegistry.Collector
{
    internal class BatchProcessArgumentsFactory
    {
        public BatchProcessArguments Create(string application, string environment, string fileOutput, string projectFilePath, string solutionFilePath, Uri url, string version, string repositoryUrl)
        {
            var arguments = new BatchProcessArguments
            {
                Application = application,
                Environment = environment,
                FileOutput = fileOutput,
                ProjectFilePath = projectFilePath,
                SolutionFilePath = solutionFilePath,
                Url = url,
                Version = version,
                RepositoryUrl = repositoryUrl,
            };

            try
            {
                ProcessProjectFile(arguments);

                ProcessSolutionFile(arguments);
            }
            catch (Exception ex)
            {
                ex.Message.LogCritical(this, ex);
                return null;
            }

            return arguments;
        }

        private void ProcessProjectFile(BatchProcessArguments arguments)
        {
            arguments.ProjectFilePath = arguments.ProjectFilePath.TrimEnd('"');
            arguments.ProjectFilePath = System.IO.Path.GetFullPath(arguments.ProjectFilePath);

            if (!arguments.ProjectFilePath.EndsWith(".csproj", StringComparison.InvariantCultureIgnoreCase)) // directory provided
            {
                var files = Directory.EnumerateFiles(arguments.ProjectFilePath, "*.csproj").ToList();

                switch (files.Count)
                {
                    case 0:
                        throw new ArgumentException("Project file not found. Please try to set full path to your *.csproj file");
                    case 1:
                        arguments.ProjectFilePath = files[0];
                        break;
                    default:
                        throw new ArgumentException(string.Format("Error: More than one project found in directory {0}. Set full path to the project file", arguments.ProjectFilePath));
                }
            }
        }

        private void ProcessSolutionFile(BatchProcessArguments arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments.SolutionFilePath))
            {
                var success = FindSolutionFile(arguments);

                if (!success)
                {
                    throw new ArgumentException("Solution file can't be found, try to set seolution file location explicitly");
                }
            }
            else
            {
                arguments.SolutionFilePath = Path.GetFullPath(arguments.SolutionFilePath);
            }
        }

        private bool FindSolutionFile(BatchProcessArguments arguments)
        {
            bool success = false;

            var directory = new FileInfo(arguments.ProjectFilePath).Directory;
            do
            {
                var files = directory.GetFiles("*.sln");

                if (files.Length == 1)
                {
                    arguments.SolutionFilePath = files[0].FullName;
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
    }

}

