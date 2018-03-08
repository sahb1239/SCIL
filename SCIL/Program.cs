using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Mono.Cecil;
using SCIL.Instructions;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace SCIL
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(RunOptionsAndReturnExitCode)
                .WithNotParsed<Options>(HandleParseError);
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            /*foreach (var err in errs)
            {
                Console.WriteLine(err);
            }*/
        }

        private static void RunOptionsAndReturnExitCode(Options opts)
        {
            Run(opts).GetAwaiter().GetResult();
        }

        private static async Task Run(Options opts)
        {
            // Check input file
            var fileInfo = new FileInfo(opts.ApkFile);
            if (!fileInfo.Exists)
            {
                Console.WriteLine("File does not exists!");
                return;
            }

            // Check output path
            var outputPathInfo = new DirectoryInfo(opts.OutputPath);
            if (!outputPathInfo.Exists)
            {
                outputPathInfo.Create();
            }

            // Load instruction emitters
            var emitterInterface = typeof(IInstructionEmitter);
            var emitters = Assembly.GetExecutingAssembly().DefinedTypes
                .Where(e => e.ImplementedInterfaces.Any(i => i == emitterInterface))
                .Select(Activator.CreateInstance)
                .Cast<IInstructionEmitter>()
                .ToList();

            
            // Detect if file is zip
            if (await ZipHelper.CheckSignature(fileInfo.FullName))
            {
                await LoadZip(fileInfo, outputPathInfo, emitters);
                return;
            }

            // File is not a zip - detect dll and exe
            throw new NotImplementedException();
        }

        private static async Task LoadZip(FileInfo fileInfo, DirectoryInfo outputPathInfo, IEnumerable<IInstructionEmitter> emitters)
        {
            // Open zip file
            using (var zipFile = ZipFile.OpenRead(fileInfo.FullName))
            {
                var assemblies = zipFile.Entries.Where(FilterXamarinAssembliesDlls);
                foreach (var assembly in assemblies)
                {
                    // Create assembly path
                    var assemblyPath = outputPathInfo.CreateSubdirectory(assembly.Name);

                    using (var stream = new MemoryStream())
                    {
                        await assembly.Open().CopyToAsync(stream).ConfigureAwait(false);

                        // Set position 0
                        stream.Position = 0;

                        await ReadAssembly(stream, assemblyPath, emitters).ConfigureAwait(false);
                    }
                }
            }
        }

        private static async Task ReadAssembly(Stream stream, DirectoryInfo outputDirectory, IEnumerable<IInstructionEmitter> emitters)
        {
            using (var module = ModuleDefinition.ReadModule(stream))
            {
                await ReadModule(module, outputDirectory, emitters).ConfigureAwait(false);
            }
        }

        private static async Task ReadModule(ModuleDefinition module, DirectoryInfo outputDirectory, IEnumerable<IInstructionEmitter> emitters)
        {
            foreach (var type in module.Types)
            {
                var typeDirectory = GetSubpathName(type, outputDirectory);

                foreach (var methodDefinition in type.Methods)
                {
                    var file = new FileInfo(Path.Combine(typeDirectory.FullName, "method_" + GetSafePath(methodDefinition.Name)));

                    var output = methodDefinition.HasBody ? ProcessCIL(type, methodDefinition.Body, emitters) : "";
                    await File.WriteAllTextAsync(file.FullName, output).ConfigureAwait(false);
                }
            }
        }

        private static string ProcessCIL(TypeDefinition typeDefinition, MethodBody methodBody, IEnumerable<IInstructionEmitter> emitters)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var instruction in methodBody.Instructions)
            {
                bool foundEmitter = false;
                foreach (var emitter in emitters)
                {
                    var emitterOutput = emitter.GetCode(typeDefinition, methodBody, instruction);
                    if (emitterOutput == null)
                        continue;

                    foundEmitter = true;
                    builder.AppendLine(emitterOutput);
                    break;
                }

                if (!foundEmitter)
                {
                    Console.WriteLine($"Error: No emitter found for code {instruction.OpCode.Name}");
                    Console.ReadKey();
                }


                /*
                builder.Append($"{SimplifyOpCode(instruction)}");
                if (instruction.Operand != null)
                {
                    builder.AppendLine($": {instruction.Operand}");
                }
                else
                {
                    builder.AppendLine();
                }*/
            }
            return builder.ToString();
        }

        private static DirectoryInfo GetSubpathName(TypeDefinition type, DirectoryInfo outputDirectory)
        {
            var fullName = GetSafePath(type.FullName);

            return outputDirectory.CreateSubdirectory(fullName);
        }

        private static string GetSafePath(string input)
        {
            return new string(input.Where(Char.IsLetterOrDigit).ToArray());
        }

        private static bool FilterXamarinAssembliesDlls(ZipArchiveEntry entry)
        {
            return entry.FullName.StartsWith("assemblies", StringComparison.OrdinalIgnoreCase) &&
                   entry.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
        }
    }
}
