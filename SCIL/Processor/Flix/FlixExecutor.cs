using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using SCIL.Logger;

namespace SCIL.Flix
{
    public class FlixExecutor : IFlixExecutor
    {
        private readonly ILogger _logger;
        private readonly string _tempPath;
        private readonly List<string> _compileFlixList = new List<string>();
        private string _flixPath;

        public FlixExecutor(ILogger logger)
        {
            _logger = logger;
            _tempPath = Path.Combine(Path.GetTempPath(), "SCIL", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempPath);

            // Extract
            ExtractFlix();
        }

        private void ExtractFlix()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            foreach (var resource in resources)
            {
                // Skip non flix files and non Flix jar files
                if (!resource.EndsWith(".flix") && !resource.EndsWith("flix.jar"))
                {
                    continue;
                }

                // Extract file
                var outputPath = Path.Combine(_tempPath, resource);
                using (var fileStream = File.OpenWrite(outputPath))
                {
                    using (var resourceStrem = assembly.GetManifestResourceStream(resource))
                    {
                        resourceStrem.CopyTo(fileStream);
                    }
                }

                // Set flix jar file
                if (resource.EndsWith("flix.jar"))
                {
                    _flixPath = outputPath;
                }
                else
                {
                    // Set file list which should be compiled
                    _compileFlixList.Add(outputPath);
                }
            }
        }

        public void ExecuteFlix(params string[] args)
        {
            var fileName = "java";
            var arguments = GetArguments(args);

            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Process process = Process.Start(processInfo);

            // Attach handlers for process
            process.OutputDataReceived += (sender, eventArgs) => _logger.Log(eventArgs.Data);
            process.ErrorDataReceived += (sender, eventArgs) => _logger.Log(eventArgs.Data);

            // Asynchronously read the standard output of the spawned process. 
            // This raises OutputDataReceived events for each line of output.
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
        }

        private string GetArguments(params string[] args)
        {
            var listArguments = new List<string>
            {
                $"-jar {QuotePath(_flixPath)}"
            };
            listArguments.AddRange(_compileFlixList.Select(QuotePath));
            listArguments.AddRange(args);

            return string.Join(" ", listArguments);
        }

        private string QuotePath(string path) => $"\"{path}\"";

        public void Dispose()
        {
            try
            {
                Directory.Delete(_tempPath, true);
            }
            catch
            {
                // ignored since not fatal if we cannot clear our temp path
            }
        }

        public void Execute(IEnumerable<string> files, params string[] args)
        {
            var arguments = files.Select(QuotePath);

            if (args.Any())
            {
                arguments = arguments.Concat(args);
            }
            else
            {
                arguments = arguments.Concat(new List<string>()
                {
                    "--print Sources,Sinks,TaintListStack,TaintListLocalVar,TaintListArg,Results"
                });
            }

            ExecuteFlix(arguments.ToArray());
        }
    }
}
