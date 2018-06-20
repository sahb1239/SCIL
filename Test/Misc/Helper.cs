using Microsoft.Extensions.DependencyInjection;
using SCIL;
using SCIL.Flix;
using SCIL.Logger;
using SCIL.Processor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public static class Helper
    {
        public static async Task AnalyzeTestProgram(string name, List<string> logs)
        {
            // Extract extra Flix file
            var _tempPath = Path.Combine(Path.GetTempPath(), "SCIL", Guid.NewGuid().ToString() + "-test");
            Directory.CreateDirectory(_tempPath);

            // Extra files to use
            var _compileFlixList = new List<string>();

            // Extract from assembly
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            foreach (var resource in resources)
            {
                // Skip non flix files and non Flix jar files
                if (!resource.EndsWith(".flix"))
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

                // Set file list which should be compiled
                _compileFlixList.Add(outputPath);
            }

            // Add flix arguments
            var flixArgs = new List<string>();
            flixArgs.AddRange(_compileFlixList);
            flixArgs.Add("--print Results");

            // Run test
            await Helper.Run(new ConsoleOptions
            {
                InputFile = @"..\..\..\..\TestPrograms\" + name + @"\bin\Debug\netcoreapp2.0\" + name + ".dll",
                OutputPath = @"./bin/Debug/netcoreapp2.0/Output/",
                NoFlix = false,
                Excluded = new List<string>(),
                JavaArgs = new List<string>(),
                FlixArgs = new List<string> { string.Join(" ", flixArgs) },
                Verbose = false,
                Wait = false,
                Recursive = false,
                UpdateIgnored = false,
                NoStringAnalysis = false
            }, logs);
        }

        public static async Task StringAnalysisOnTestProgram(string name, List<string> logs)
        {
            await Helper.Run(new ConsoleOptions
            {
                InputFile = @"..\..\..\..\TestPrograms\" + name + @"\bin\Debug\netcoreapp2.0\" + name + ".dll",
                OutputPath = @"./bin/Debug/netcoreapp2.0/Output/",
                NoFlix = false,
                Excluded = new List<string>(),
                JavaArgs = new List<string>(),
                FlixArgs = new List<string> { "--print SecretStrings" },
                Verbose = false,
                Wait = false,
                Recursive = false,
                UpdateIgnored = false,
                NoStringAnalysis = false
            }, logs);
        }

        public static async Task Run(ConsoleOptions opts, List<string> logs)
        {
            // Check output path
            var outputPathInfo = new DirectoryInfo(opts.OutputPath);
            if (!outputPathInfo.Exists)
            {
                outputPathInfo.Create();
            }

            // Create logger
            var logger = new TestLogger(logs);

            // Create configuration
            var configuration = new Configuration(opts.Excluded, opts.OutputPath, opts.Async, opts.JavaArgs, opts.FlixArgs, opts.ShowFlixWindow, opts.NoStringAnalysis, opts.UpdateIgnored);

            // Registrer services
            var serviceCollection = new ServiceCollection();
            Startup.ConfigureServices(serviceCollection, configuration, logger);
            var services = serviceCollection.BuildServiceProvider();

            // Create flix executor
            using (var executor = services.GetRequiredService<IFlixExecutor>())
            {
                // Get file processor
                var fileProcessor = services.GetRequiredService<FileProcessor>();

                // Check for input file
                // Check if path is input file
                var fileInfo = new FileInfo(opts.InputFile);
                if (fileInfo.Exists)
                {
                    var files = await fileProcessor.ProcessFile(fileInfo);
                    await ProcessFlix(files, executor, opts);
                }
                else
                {
                    logger.Log($"File {opts.InputFile} not found");
                }
            }
        }

        public static async Task ProcessFlix(IEnumerable<string> generatedFiles, IFlixExecutor executor, ConsoleOptions opts)
        {
            // Execute
            if (!opts.NoFlix)
            {
                await executor.Execute(generatedFiles);
            }
        }

        public static List<Result> ParseResults(List<string> rawLog)
        {
            var results = new List<Result>();

            foreach (var entry in rawLog)
            {
                if (entry == null)
                    continue;

                if (entry.StartsWith("|"))
                {
                    var result = entry.Replace(" ", string.Empty).Split('|', StringSplitOptions.RemoveEmptyEntries);

                    results.Add(new Result
                    {
                        Source = result[0],
                        Sink = result[1],
                        Type = result[2]
                    });
                }
            }

            return results.Skip(1).ToList();
        }

        public static List<StringAnalysisResult> ParseStringAnalysisResults(List<string> rawLog)
        {
            var results = new List<StringAnalysisResult>();

            foreach (var entry in rawLog)
            {
                if (entry == null)
                    continue;

                if (entry.StartsWith("|"))
                {
                    var result = entry.Replace(" ", string.Empty).Split('|', StringSplitOptions.RemoveEmptyEntries);

                    results.Add(new StringAnalysisResult
                    {
                        Name = result[0],
                        Charset = result[1]
                    });
                }
            }

            return results.Skip(1).ToList();
        }
    }

    public class TestLogger : ILogger
    {
        private List<string> _logs;

        public TestLogger(List<string> Logs)
        {
            _logs = Logs;
        }

        public void Log(string message) => Log(message, false);

        public void Log(string message, bool verbose)
        {
            _logs.Add(message);
        }

        public void Wait() { }
    }

    public class Result
    {
        public string Source { get; set; }

        public string Sink { get; set; }

        public string Type { get; set; }

        public override bool Equals(object obj)
        {
            Result result = obj as Result;

            return (obj != null)
                && (Source == result.Source)
                && (Sink == result.Sink)
                && (Type == result.Type);
        }
    }

    public class StringAnalysisResult
    {
        public string Name { get; set; }
        public string Charset { get; set; }

        public override bool Equals(object obj)
        {
            StringAnalysisResult result = obj as StringAnalysisResult;

            return (obj != null)
                && (Name == result.Name)
                && (Charset == result.Charset);
        }
    }
}
