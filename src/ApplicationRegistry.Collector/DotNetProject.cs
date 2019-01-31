using McMaster.Extensions.CommandLineUtils;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace ApplicationRegistry.Collector
{
    public class DotNetProject : IDisposable
    {
        private readonly ILogger<DotNetProject> _logger;
        private readonly string _projectFile;
        private readonly string _projectDirectory;
        private readonly List<(string file, string bakFile)> _filesToRollBack = new List<(string file, string bakFile)>();
        private readonly List<string> _filesToRemove = new List<string>();

        public DotNetProject(ILogger<DotNetProject> logger, string projectFile)
        {
            if (string.IsNullOrWhiteSpace(projectFile))
            {
                throw new ArgumentException("Field cant't be null or white space", nameof(projectFile));
            }

            _logger = logger;
            _projectFile = projectFile;
            var projectFileBak = BackupFile(projectFile);
            _filesToRollBack.Add((projectFile, projectFileBak));
            _projectDirectory = new FileInfo(_projectFile).Directory.FullName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Absolute or relative to csproj</param>
        /// <param name="content"></param>
        public void AddFile(string path, string content, bool undoAfterDispose = true)
        {
            var filePath = path;

            if (!Path.IsPathFullyQualified(filePath))
            {
                var projectFileDirectoryPath = _projectDirectory;
                filePath = Path.GetFullPath(filePath, projectFileDirectoryPath);
            }

            if (undoAfterDispose)
            {
                _filesToRemove.Add(filePath);
            }

            File.WriteAllText(filePath, content);
        }

        public string Run(params string[] args)
        {
            var parameters = new StringBuilder("run --no-launch-profile --framework netcoreapp2.1 -- "); // TODO: no hardcoded framework

            if(args != null)
            {
                parameters.Append(string.Join(" ", args));
            }

            var start = new ProcessStartInfo(DotNetExe.FullPathOrDefault(), parameters.ToString())
            {
                CreateNoWindow = true,
                WorkingDirectory = _projectDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                
            };
            
            using (var process = new Process())
            {
                process.StartInfo = start;
                process.Start();

                var result = new StringBuilder();

                while (!process.HasExited)
                {
                    var lines = process.StandardOutput.ReadToEnd();
                    _logger.LogDebug(lines);
                    result.Append(lines);
                }

                
                if (process.ExitCode != 0)
                {
                    var error = process.StandardError.ReadToEnd();
                    result.Insert(0, error);

                    throw new Exception("Operation failed. Error: " + result.ToString());
                }

                return result.ToString();
            }
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
                RedirectStandardError = true
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

        public void Build(string startupObject = null)
        {
            var commandBuilder = new StringBuilder("build ");
            commandBuilder.Append("\"" + _projectFile + "\"");

            if (!string.IsNullOrWhiteSpace(startupObject))
            {
                commandBuilder.AppendFormat(" /p:StartupObject={0}", startupObject);
            }

            var start = new ProcessStartInfo(DotNetExe.FullPathOrDefault(), commandBuilder.ToString())
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
                
                while(!process.HasExited)
                {
                    var output = process.StandardOutput.ReadToEnd();
                    _logger.LogDebug(output);
                }

                if (process.ExitCode != 0)
                {
                    _logger.LogError("Error while building application");
                    throw new Exception("Project can't be build. Read previous erroros ");
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

        private string BackupFile(string filePath)
        {
            if (!Path.IsPathFullyQualified(filePath))
                throw new ArgumentException("path should be absoule", nameof(filePath));

            var backFileName = filePath + ".bak";

            File.Copy(filePath, backFileName, true);

            return backFileName;
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            var exceptions = new List<Exception>();

            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var backup in _filesToRollBack)
                    {
                        try
                        {
                            File.Copy(backup.bakFile, backup.file, true);
                            File.Delete(backup.bakFile);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }

                    foreach (var remove in _filesToRemove)
                    {
                        try
                        {
                            File.Delete(remove);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }

                _disposedValue = true;

                if(exceptions.Any())
                {
                    throw new AggregateException("Couple files couldn't be retrived from backup. See inner exceptions for details.", exceptions);
                }
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

