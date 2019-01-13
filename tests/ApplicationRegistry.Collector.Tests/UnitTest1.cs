using System;
using System.IO;
using Xunit;

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
            }

            Console.WriteLine("test");
        }
    }
}
