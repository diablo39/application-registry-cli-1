using McMaster.Extensions.CommandLineUtils;
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
    public class DotNetProjectFactory
    {
        public DotNetProject CreateProject(string path)
        {
            var instance = new DotNetProject(path);
            return instance;
        }
    }

    public class DotNetProject : IDisposable
    {
        private readonly string _projectFile;
        private readonly string _projectDirectory;
        private readonly List<(string file, string bakFile)> _filesToRollBack = new List<(string file, string bakFile)>();
        private readonly List<string> _filesToRemove = new List<string>();

        public DotNetProject(string projectFile)
        {
            if (string.IsNullOrWhiteSpace(projectFile))
            {
                throw new ArgumentException("Field cant't be null or white space", nameof(projectFile));
            }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public string Run(params string[] args)
        {
            var parameters = new StringBuilder("run --no-launch-profile --framework netcoreapp2.1 -- "); // TODO: no hardcoded framework

            if (args != null)
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
            var result = new StringBuilder();
            var exitedCode = 0;
            using (var process = new Process())
            {
                process.StartInfo = start;
                process.Start();

                while (!process.HasExited)
                {
                    var lines = process.StandardOutput.ReadToEnd();

                    lines.LogTrace(this);

                    result.Append(lines);
                }

                exitedCode = process.ExitCode;

                if (process.ExitCode != 0)
                {
                    var error = process.StandardError.ReadToEnd();
                    result.Insert(0, error);
                }
            }

            if (exitedCode != 0)
            {
                Thread.Sleep(5000); // TODO: this is wrong!!!

                throw new Exception(string.Concat("Operation failed. Error: ", Environment.NewLine, result.ToString()));
            }

            return result.ToString();
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

            using (var process = new Process { StartInfo = start })
            {
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
            var commandBuilder = new StringBuilder($"build -v q \"{_projectFile}\"");

            if (!string.IsNullOrWhiteSpace(startupObject))
                SetStartupObject(startupObject);

            var start = new ProcessStartInfo(DotNetExe.FullPathOrDefault(), commandBuilder.ToString())
            {
                CreateNoWindow = true,
                WorkingDirectory = _projectDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true
            };

            using (var process = new Process { StartInfo = start })
            {
                process.Start();

                while (!process.HasExited)
                {
                    var output = process.StandardOutput.ReadToEnd();
                    output.LogTrace(this);
                }

                if (process.ExitCode != 0)
                {
                    "Error while building application".LogError(this);
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

        public void DisableCompilationForCshtml()
        {
            var directory = Path.GetDirectoryName(_projectFile);

            var views = Directory.EnumerateFiles(directory, "*.cshtml", SearchOption.AllDirectories);

            var viewsRelativePath = views.Select(e => e.Substring(directory.Length + 1)).Select(e => new XElement("Content", new XAttribute("Remove", e))).ToList();

            var document = XDocument.Load(_projectFile);

            var root = document.Root;

            var ignoreElement = new XElement("ItemGroup", viewsRelativePath);

            root.Add(ignoreElement);

            document.Save(_projectFile);
        }

        public void SetStartupObject(string startupObject)
        {
            var document = XDocument.Load(_projectFile);

            var root = document.Root;

            var existingStartupObjects = root.Descendants("StartupObject").ToList();

            if (existingStartupObjects.Count != 0)
            {
                for (int i = 0; i < existingStartupObjects.Count; i++)
                {
                    existingStartupObjects[i].Value = startupObject;
                }
            }
            else
            {
                var e = new XElement("PropertyGroup", new XElement("StartupObject", startupObject));
                root.Add(e);
            }

            document.Save(_projectFile);

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
                    foreach (var (file, bakFile) in _filesToRollBack)
                    {
                        try
                        {
                            File.Copy(bakFile, file, true);
                            File.Delete(bakFile);
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

                if (exceptions.Any())
                {
                    throw new AggregateException("Couple files couldn't be retrived from backup. See inner exceptions for details.", exceptions);
                }
            }
        }


        // This code added to correctly implement the disposable pattern.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "<Pending>")]
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}

