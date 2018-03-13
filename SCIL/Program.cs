using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using SCIL.Analyzers;
using SCIL.Logger;
using SCIL.Writer;

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

            // Load instruction emitters
            var emitterInterface = typeof(IInstructionEmitter);
            var emitters = Assembly.GetExecutingAssembly().DefinedTypes
                .Where(e => e.ImplementedInterfaces.Any(i => i == emitterInterface) &&
                            e.CustomAttributes.All(attr => typeof(IgnoreEmitterAttribute) != attr.AttributeType))
                .OrderBy(e =>
                    e.CustomAttributes.Any(attr => typeof(EmitterOrderAttribute) == attr.AttributeType)
                        ? e.GetCustomAttribute<EmitterOrderAttribute>().Order
                        : 10)
                .Select(Activator.CreateInstance)
                .Cast<IInstructionEmitter>()
                .ToList();

            // Count instructions
            var instructionCounter = new InstructionCounter();
            if (opts.CountInstructions)
            {
                emitters.Insert(0, instructionCounter);
            }

            // Create logger
            var logger = new ConsoleLogger(opts.Verbose, opts.Wait);

            // Create module writer
            IModuleWriter moduleWriter = opts.NoOutput ? (IModuleWriter) new NoOutputWriter() : new ModuleWriter(outputPathInfo.FullName);
            
            // Check for input file
            if (!string.IsNullOrWhiteSpace(opts.InputFile))
            {
                // Check if path is input file
                var fileInfo = new FileInfo(opts.InputFile);
                if (fileInfo.Exists)
                {
                    await AnalyzeFile(fileInfo, emitters, logger, moduleWriter);
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
                    foreach (var file in 
                        Directory.GetFiles(pathInfo.FullName, "*.apk", SearchOption.TopDirectoryOnly)
                            .Concat(Directory.GetFiles(pathInfo.FullName, "*.exe", SearchOption.TopDirectoryOnly))
                            .Concat(Directory.GetFiles(pathInfo.FullName, "*.dll", SearchOption.TopDirectoryOnly)))
                    {
                        var fileInfo = new FileInfo(file);
                        await AnalyzeFile(fileInfo, emitters, logger, moduleWriter);
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

        private static async Task AnalyzeFile(FileInfo fileInfo, IReadOnlyCollection<IInstructionEmitter> emitters, ILogger logger, IModuleWriter moduleWriter)
        {
            // Reset analyzers
            foreach (var analyzer in emitters.OfType<IInstructionAnalyzer>())
            {
                analyzer.Reset();
            }

            // Detect if file is zip
            if (await ZipHelper.CheckSignature(fileInfo.FullName))
            {
                await LoadZip(fileInfo, emitters, logger, moduleWriter);
            }
            else
            {
                // TODO : Detect dll and exe
                throw new NotImplementedException();
            }

            // Print analyzers
            foreach (var analyzer in emitters.OfType<IInstructionAnalyzer>())
            {
                logger.Log(string.Join(Environment.NewLine, analyzer.GetOutput()));
            }
        }

        private static async Task LoadZip(FileInfo fileInfo, IReadOnlyCollection<IInstructionEmitter> emitters, ILogger logger, IModuleWriter moduleWriter)
        {
            // Open zip file
            using (var zipFile = ZipFile.OpenRead(fileInfo.FullName))
            {
                var assemblies = zipFile.Entries.Where(FilterXamarinAssembliesDlls);
                foreach (var assembly in assemblies)
                {
                    using (var stream = new MemoryStream())
                    {
                        await assembly.Open().CopyToAsync(stream).ConfigureAwait(false);

                        // Set position 0
                        stream.Position = 0;

                        await ProcessAssymbly(stream, await moduleWriter.GetAssemblyModuleWriter(assembly.Name), emitters, logger).ConfigureAwait(false);
                    }
                }
            }
        }

        private static Task ProcessAssymbly(Stream stream, IModuleWriter moduleWriter, IReadOnlyCollection<IInstructionEmitter> emitters, ILogger logger)
        {
            var moduleProcessor = new ModuleProcessor(emitters, moduleWriter, logger);
            return moduleProcessor.ProcessAssembly(stream);
        }

        private static bool FilterXamarinAssembliesDlls(ZipArchiveEntry entry)
        {
            return entry.FullName.StartsWith("assemblies", StringComparison.OrdinalIgnoreCase) &&
                   entry.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
        }
    }

    class ConsoleOptions
    {
        [Option('f', "InputFile", Required = false, HelpText = "Apk files to be processed.")]
        public string InputFile { get; set; }

        [Option('p', "InputPath", Required = false, HelpText = "Input path to search for apk or dll/exe files")]
        public string InputPath { get; set; }

        [Option('o', "OutputPath", Required = true, HelpText = "Output path")]
        public string OutputPath { get; set; }

        [Option("noOutput", Required = false, HelpText = "Verbose output")]
        public bool NoOutput { get; set; }

        [Option("verbose", Required = false, HelpText = "Verbose output")]
        public bool Verbose { get; set; }

        [Option('w', "wait",  Required = false, HelpText = "Wait for some error messages")]
        public bool Wait { get; set; }

        [Option('c', "countInstructions", Required = false, HelpText = "Count number of instructions for each module")]
        public bool CountInstructions { get; set; }
    }
}
