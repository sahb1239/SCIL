using Microsoft.Extensions.DependencyInjection;
using SCIL;
using SCIL.Flix;
using SCIL.Logger;
using SCIL.Processor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public static class Helper
    {
        public static async Task AnalyzeTestProgram(string name, List<string> logs)
        {
            await Helper.Run(new ConsoleOptions
            {
                InputFile = @"..\..\..\..\TestPrograms\" + name + @"\bin\Debug\netcoreapp2.0\" + name + ".dll",
                OutputPath = @"./bin/Debug/netcoreapp2.0/Output/",
                NoFlix = false,
                Excluded = new List<string>(),
                JavaArgs = new List<string>(),
                FlixArgs = new List<string> { "--print Results" },
                Verbose = false,
                Wait = false,
                Recursive = false,
                UpdateIgnored = false
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
            var configuration = new Configuration(opts.Excluded, opts.OutputPath, opts.Async, opts.JavaArgs, opts.FlixArgs, opts.ShowFlixWindow, opts.UpdateIgnored);

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
                if (!string.IsNullOrWhiteSpace(opts.InputFile))
                {
                    // Check if path is input file
                    var fileInfo = new FileInfo(opts.InputFile);
                    if (fileInfo.Exists)
                    {
                        var files = await fileProcessor.ProcessFile(fileInfo);
                        ProcessFlix(files, executor, opts);
                    }
                    else
                    {
                        logger.Log($"File {opts.InputFile} not found");
                    }

                    return;
                }

                // Check for input path
                if (!string.IsNullOrWhiteSpace(opts.InputPath))
                {
                    var pathInfo = new DirectoryInfo(opts.InputPath);
                    if (pathInfo.Exists)
                    {
                        var searchOption =
                            opts.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                        foreach (var file in
                            Directory.GetFiles(pathInfo.FullName, "*.apk", searchOption)
                                .Concat(Directory.GetFiles(pathInfo.FullName, "*.exe", searchOption))
                                .Concat(Directory.GetFiles(pathInfo.FullName, "*.dll", searchOption)))
                        {
                            var fileInfo = new FileInfo(file);
                            var files = await fileProcessor.ProcessFile(fileInfo);
                            ProcessFlix(files, executor, opts);
                        }
                    }
                    else
                    {
                        logger.Log($"Path {opts.InputPath} not found");
                    }

                    return;
                }

                logger.Log("Please select file or path");
            }
        }

        public static void ProcessFlix(IEnumerable<string> generatedFiles, IFlixExecutor executor, ConsoleOptions opts)
        {
            // Execute
            if (!opts.NoFlix)
            {
                executor.Execute(generatedFiles);
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

                    var i = 3;
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
}
