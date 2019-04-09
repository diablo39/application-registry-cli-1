using System;
using System.IO;
using Xunit;
using System.Diagnostics;
using System.Linq;

namespace ApplicationRegistry.Collector.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void FullPath()
        {
            var relativePath = "test/aa";
            var a = System.IO.Path.GetFullPath(relativePath);
            Console.WriteLine(a);
        }

        [Fact]
        public void MapRelativePathToFullPath()
        {
            var _projectFile = @"C:\test\test.csproj";
            var filePath = "test\tt.cs";
            if (!Path.IsPathRooted(filePath))
            {
                var file = new FileInfo(_projectFile);
                var projectFileDirectory = file.Directory;
                var projectFileDirectoryPath = projectFileDirectory.FullName;
                filePath = Path.GetFullPath(filePath, projectFileDirectoryPath);

                Path.GetRelativePath("C:", filePath);
            }

            Console.WriteLine("test");
        }

        [Fact]
        public void CshtmlPaths()
        {
            var csproj = @"C:\SDS\BM.Domain.Timeline\BM.Domain.Timeline\src\BM.Domain.Timeline.WebApi\BM.Domain.Timeline.WebApi.csproj";

            var directory = Path.GetDirectoryName(csproj);

            var views = Directory.EnumerateFiles(directory, "*.cshtml", SearchOption.AllDirectories);
            var notCompilePattern = "<None Include=\"{0}\" />";
            var viewsRelativePath = views.Select(e => e.Substring(directory.Length + 1)).Select(e=> string.Format(notCompilePattern, e)).ToList();
        }
    }
}
