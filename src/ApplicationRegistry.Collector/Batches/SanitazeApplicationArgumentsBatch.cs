using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Batches
{
    class SanitazeApplicationArgumentsBatch : IBatch
    {
        public Task<BatchResult> ProcessAsync(BatchContext context)
        {
            context.Arguments.ProjectFilePath = context.Arguments.ProjectFilePath.TrimEnd('"');
            context.Arguments.ProjectFilePath = System.IO.Path.GetFullPath(context.Arguments.ProjectFilePath);

            if (!context.Arguments.ProjectFilePath.EndsWith(".csproj")) // directory provided
            {
                var files = Directory.EnumerateFiles(context.Arguments.ProjectFilePath, "*.csproj").ToList();

                if (files.Count == 0)
                {
                    Console.Error.WriteLine("Project file not found"); // TODO: try to use result as store
                    return Task.FromResult(BatchResult.CreateFailResult());
                }

                if (files.Count > 1)
                {
                    Console.Error.WriteLine("Error: More than one project found in directory {0}", context.Arguments.ProjectFilePath); // TODO: try to use result as store
                    Console.Error.WriteLine("Set full path to the project file");

                    return Task.FromResult(BatchResult.CreateFailResult());
                }

                context.Arguments.ProjectFilePath = files[0];
            }

            if (string.IsNullOrWhiteSpace(context.Arguments.SolutionFilePath))
            {
                var success = FindSolutionFile(context);

                if (!success)
                {
                    Console.Error.WriteLine("Solution file can't be found, try to set seolution file location explicitly"); // TODO: try to use result as store
                    return Task.FromResult(BatchResult.CreateFailResult());
                }
            }

            return Task.FromResult(BatchResult.CreateSuccessResult());
        }

        private bool FindSolutionFile(BatchContext context)
        {
            bool success = false;

            var directory = new FileInfo(context.Arguments.ProjectFilePath).Directory;
            do
            {
                var files = directory.GetFiles("*.sln");

                if (files.Length == 1)
                {
                    context.Arguments.SolutionFilePath = files[0].FullName;
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
