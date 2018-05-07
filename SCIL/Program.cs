using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using SCIL.Analyzers;
using SCIL.Decompressor;
using SCIL.Flix;
using SCIL.Logger;
using SCIL.Processor;
using SCIL.Writer;
using SCIL.Processor;

namespace SCIL
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<ConsoleOptions>(args)
                .WithParsed<ConsoleOptions>(RunOptionsAndReturnExitCode)
                .WithNotParsed(error => {});
#if DEBUG
            Console.ReadKey();
#endif
        }

        private static void RunOptionsAndReturnExitCode(ConsoleOptions opts)
        {
            Run(opts).GetAwaiter().GetResult();
        }

        private static async Task Run(ConsoleOptions opts)
        {
            // Check output path
            var outputPathInfo = new DirectoryInfo(opts.OutputPath);
            if (!outputPathInfo.Exists)
            {
                outputPathInfo.Create();
            }
            
            // Create logger
            var logger = new ConsoleLogger(opts.Verbose, opts.Wait);
            
            // Create configuration
            var configuration = new Configuration(opts.Excluded, opts.OutputPath);

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
                executor.Execute(generatedFiles, opts.FlixArgs.ToArray());
            }
        }
    }

    public class ConsoleOptions
    {
        [Option('f', "InputFile", Required = false, HelpText = "Apk files to be processed.")]
        public string InputFile { get; set; }

        [Option('p', "InputPath", Required = false, HelpText = "Input path to search for apk or dll/exe files")]
        public string InputPath { get; set; }

        [Option('o', "OutputPath", Required = true, HelpText = "Output path")]
        public string OutputPath { get; set; }

        [Option('e', "excluded", Required = false, Separator = ',', HelpText = "Assemblies to exclude")]
        public IEnumerable<string> Excluded { get; set; }

        [Option("verbose", Required = false, HelpText = "Verbose output")]
        public bool Verbose { get; set; }

        [Option("NoFlix", Required = false, HelpText = "Disable running flix")]
        public bool NoFlix { get; set; }

        [Option("FlixArgs", Required = false, HelpText = "Additional flix args")]
        public IEnumerable<string> FlixArgs { get; set; }

        [Option('w', "wait",  Required = false, HelpText = "Wait for some error messages")]
        public bool Wait { get; set; }

        [Option('r', "recursive", Required = false, HelpText = "Recursive search input path")]
        public bool Recursive { get; set; }
    }
}
