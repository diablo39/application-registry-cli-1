//using System;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;

//namespace ApplicationRegistry.Collector.Batches
//{
//    class SanitazeApplicationArgumentsBatch : IBatch
//    {
//        public Task<BatchExecutionResult> ProcessAsync(BatchContext context)
//        {
//            BatchProcessArguments arguments = context.Arguments;
//            arguments.ProjectFilePath = arguments.ProjectFilePath.TrimEnd('"');
//            arguments.ProjectFilePath = System.IO.Path.GetFullPath(arguments.ProjectFilePath);

//            if (!arguments.ProjectFilePath.EndsWith(".csproj", StringComparison.InvariantCultureIgnoreCase)) // directory provided
//            {
//                var files = Directory.EnumerateFiles(arguments.ProjectFilePath, "*.csproj").ToList();

//                if (files.Count == 0)
//                {
//                    "Project file not found. Please try to set full path to your *.csproj file".LogCritical(this);
//                    return Task.FromResult(BatchExecutionResult.CreateFailResult());
//                }

//                if (files.Count > 1)
//                {
//                    Console.Error.WriteLine("Error: More than one project found in directory {0}", arguments.ProjectFilePath); // TODO: try to use result as store
//                    Console.Error.WriteLine("Set full path to the project file");

//                    return Task.FromResult(BatchExecutionResult.CreateFailResult());
//                }

//                arguments.ProjectFilePath = files[0];
//            }

//            if (string.IsNullOrWhiteSpace(arguments.SolutionFilePath))
//            {
//                var success = FindSolutionFile(arguments);

//                if (!success)
//                {
//                    Console.Error.WriteLine("Solution file can't be found, try to set seolution file location explicitly"); // TODO: try to use result as store
//                    return Task.FromResult(BatchExecutionResult.CreateFailResult());
//                }
//            }
//            else
//            {
//                arguments.SolutionFilePath = Path.GetFullPath(arguments.SolutionFilePath);
//            }

//            return Task.FromResult(BatchExecutionResult.CreateSuccessResult());
//        }

//        private static bool FindSolutionFile(BatchProcessArguments arguments)
//        {
//            bool success = false;

//            var directory = new FileInfo(arguments.ProjectFilePath).Directory;
//            do
//            {
//                var files = directory.GetFiles("*.sln");

//                if (files.Length == 1)
//                {
//                    arguments.SolutionFilePath = files[0].FullName;
//                    success = true;
//                    break;
//                }

//                if (files.Length > 1)
//                {
//                    Console.Error.WriteLine("More than one solution file found.");
//                    Console.Error.WriteLine("Found:");
//                    for (int i = 0; i < files.Length; i++)
//                    {
//                        Console.Error.WriteLine($"* {files[i].FullName}");
//                    }
//                    Console.Error.WriteLine("Use parametr --solution to specify correct solution file");
//                    success = false;
//                    break;
//                }


//                directory = directory.Parent;
//            }
//            while (directory != null);
//            return success;
//        }
//    }
//}
