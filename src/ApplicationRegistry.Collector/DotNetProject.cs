using McMaster.Extensions.CommandLineUtils;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace ApplicationRegistry.Collector
{
    public class DotNetProject : IDisposable
    {
        private readonly string _projectFile;
        private readonly string _projectFileBackup;
        private readonly string _projectDirectory;

        public DotNetProject(string projectFile)
        {
            if (string.IsNullOrWhiteSpace(projectFile))
            {
                throw new ArgumentException("Field cant't be null or white space", nameof(projectFile));
            }

            _projectFile = projectFile;
            _projectFileBackup = projectFile + ".bak";
            _projectDirectory = new FileInfo(_projectFileBackup).Directory.FullName;

            File.Copy(projectFile, _projectFileBackup, true);
        }

        public void AddPackage(string packageName, string version = null)
        {
            var arguments = "add package " + packageName;

            if (!string.IsNullOrWhiteSpace(version))
                arguments += " --version " + version;

            var start = new ProcessStartInfo(DotNetExe.FullPathOrDefault(), arguments)
            {
                CreateNoWindow = true,
                WorkingDirectory = _projectDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true
            };

            using (var process = new Process())
            {
                process.StartInfo = start;
                process.Start();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    var errorOutput = process.StandardError.ReadToEnd();
                    throw new Exception("Operation failed. Error: " + errorOutput);
                }
            }
        }

        public void Build()
        {
            var start = new ProcessStartInfo(DotNetExe.FullPathOrDefault(), "build")
            {
                CreateNoWindow = true,
                WorkingDirectory = _projectDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true
            };

            using (var process = new Process())
            {
                
                process.StartInfo = start;
                process.Start();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    var errorOutput = process.StandardError.ReadToEnd();
                    throw new Exception("Operation failed. Error: " + errorOutput);
                }
            }
        }

        public void AddDotNetCliToolReference(string name, string version)
        {
            var document = XDocument.Load(_projectFile);

            var root = document.Root;

            root.Add(
                new XElement("ItemGroup", 
                    new XElement("DotNetCliToolReference", new XAttribute("Include", name), new XAttribute("Version", version))));

            document.Save(_projectFile);
        }

        public void AddAfterBuildCommand(string name, string command)
        {
            var document = XDocument.Load(_projectFile);

            var root = document.Root;

            root.Add(
                new XElement("Target", new XAttribute("Name", name), new XAttribute("AfterTargets", "AfterBuild"),
                    new XElement("Exec", new XAttribute("Command", command))));

            document.Save(_projectFile);
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    File.Copy(_projectFileBackup, _projectFile, true);
                    File.Delete(_projectFileBackup);
                }

                _disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}

