using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace ApplicationRegistry.Collector.Tools
{
    internal class CompilationProvider
    {
        private static Dictionary<string, AdhocWorkspace> _compilations = new Dictionary<string, AdhocWorkspace>();

        public AdhocWorkspace GetWorkspace(string solutionFilePath)
        {
            AdhocWorkspace result;

            if (_compilations.TryGetValue(solutionFilePath, out result))
            {
                return result;
            }

            AnalyzerManager manager = new AnalyzerManager(solutionFilePath);

            result = manager.GetWorkspace();

            _compilations.Add(solutionFilePath, result);

            return result;
        }

    }
}
