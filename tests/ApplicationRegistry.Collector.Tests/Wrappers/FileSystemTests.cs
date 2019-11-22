using ApplicationRegistry.Collector.Wrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Tests.Wrappers
{
    public class FileSystemTests
    {
        [Fact]
        public async Task File_WriteAllTextAsync_should_work()
        {
            // Arrange
            var path = Path.GetTempFileName();
            var fileContent = Guid.NewGuid().ToString();
            var encoding = System.Text.Encoding.UTF8;
            var filesystem  = new FileSystem();

            // Act
            await filesystem.File_WriteAllTextAsync(path, fileContent, encoding);

            // Assert
            var fileContentRead = File.ReadAllText(path, encoding);

            fileContentRead.Should().Be(fileContent);

            File.Delete(path);
        }

    }
}
