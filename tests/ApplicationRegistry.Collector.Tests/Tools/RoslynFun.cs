//using Buildalyzer;
//using Buildalyzer.Workspaces;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.FindSymbols;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using Xunit;

//namespace ApplicationRegistry.UnitTests
//{
//    public class RoslynFun
//    {
//        [Fact]
//        public void RunRoslyn()
//        {
//            string solutionPath = @"C:\Users\Roman\source\repos\ApplicationRegistry.Collector\ApplicationRegistry.Collector.sln";

//            string projectFile = @"ApplicationRegistry.SampleWebApplication.csproj";

//            AnalyzerManager manager = new AnalyzerManager(solutionPath);

//            var projects = manager.Projects;

//            AdhocWorkspace workspace = manager.GetWorkspace();

//            var project = workspace.CurrentSolution.Projects.Where(p => p.FilePath.Split(Path.DirectorySeparatorChar).Any(e => e == projectFile)).FirstOrDefault();

//            //var graph = workspace.CurrentSolution.GetProjectDependencyGraph();
//            //var dependencies = graph.GetProjectsThatThisProjectTransitivelyDependsOn(project.Id);
//            //var projectsToScan = dependencies.Union(new[] { project.Id });

//            var compilation = project.GetCompilationAsync().Result;

//            string restClientBaseClassName = "Microsoft.Rest.ServiceClient`1";

//            var restClientBaseClass = compilation.GetTypeByMetadataName(restClientBaseClassName);
//            var restClients = SymbolFinder.FindDerivedClassesAsync(restClientBaseClass, workspace.CurrentSolution).Result;
//            var members = restClients.First().GetMembers().OfType<IMethodSymbol>().Where(t => t.Name.EndsWith("WithHttpMessagesAsync"))
//                .Select(e => e.Name.Substring(0, e.Name.Length - "WithHttpMessagesAsync".Length)).ToList();

//            //var a = Microsoft.CodeAnalysis.CSharp.Symbols.SourceOrdinaryMethodSymbol;

//            Console.WriteLine();

//            //var msWorkspace = MSBuildWorkspace.Create();

//            //var solution = msWorkspace.OpenSolutionAsync(solutionPath).Result;
//            //foreach (var project in solution.Projects)
//            //{
//            //    foreach (var document in project.Documents)
//            //    {
//            //        Console.WriteLine(project.Name + "\t\t\t" + document.Name);
//            //    }
//            //}

//            //var workspace = new AdhocWorkspace();
//            //workspace.AddSolution(SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Create(), solutionPath));

//            //var solution = workspace.CurrentSolution;

//        }
//    }
//}
