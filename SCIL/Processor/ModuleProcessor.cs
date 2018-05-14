using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using SCIL.Logger;
using SCIL.Processor;
using SCIL.Processor.FlixInstructionGenerators;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL
{
    public class ModuleProcessor
    {
        public ModuleProcessor(ILogger logger, FlixCodeGeneratorVisitor flixCodeGenerator, ControlFlowGraph controlFlowGraph, IEnumerable<IVisitor> visitors, Configuration configuration)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            FlixCodeGenerator = flixCodeGenerator ?? throw new ArgumentNullException(nameof(flixCodeGenerator));
            ControlFlowGraph = controlFlowGraph ?? throw new ArgumentNullException(nameof(controlFlowGraph));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if (visitors == null)
            {
                throw new ArgumentNullException(nameof(visitors));
            }

            Visitors = visitors.Select(visitor => new
                {
                    visitor,
                    attribute = visitor.GetType().GetCustomAttribute<RegistrerVisitorAttribute>()
                }).Where(e => e.attribute != null)
                .Where(e => !e.attribute.Ignored)
                .OrderBy(e => e.attribute.Order)
                .Select(e => e.visitor)
                .Concat(new List<IVisitor>() {FlixCodeGenerator});
        }

        public ILogger Logger { get; }

        public FlixCodeGeneratorVisitor FlixCodeGenerator { get; }

        public ControlFlowGraph ControlFlowGraph { get; }

        public IEnumerable<IVisitor> Visitors { get; }

        public Configuration Configuration { get; }

        public async Task<string> ProcessAssembly(Stream stream)
        {
            // Load Module using Mono.Cecil
            Logger.Log("Loading module", true);
            using (var module = ModuleDefinition.ReadModule(stream))
            {
                // Check if we should ignore the module
                if (Configuration.ExcludedModules.Contains(module.Name))
                {
                    Logger.Log("Skipping excluded module: " + module.Name);
                    return null;
                }
                else
                {
                    Logger.Log("Results from module " + module.Name);
                    return await ReadModule(module).ConfigureAwait(false);
                }
            }
        }

        public async Task<string> ReadModule(ModuleDefinition module)
        {
            Logger.Log("Reading module", true);

            // Run all visitors
            var moduleBlock = ControlFlowGraph.GenerateModule(module);
            foreach (var visitor in Visitors)
            {
                visitor.Visit(moduleBlock);
            }

            // Write output to file
            var file = new FileInfo(Path.Combine(Configuration.OutputPath, GetSafePath(module.Name) + ".flix"));

            // Get UTF-8 encoding without BOM (default for File.CreateText which was used before) and just ignore invalid UTF-8 chars
            var encoder = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);

            using (var stream = new StreamWriter(File.Open(file.FullName, FileMode.Create), encoder))// File.CreateText(file.FullName))
            {
                await stream.WriteAsync(FlixCodeGenerator.ToString());
            }

            // Clear code generator
            FlixCodeGenerator.Clear();

            // Return file name
            return file.ToString();
        }

        private static string GetSafePath(string input)
        {
            return new string(input.Where(c => Char.IsLetterOrDigit(c) || c == '.').ToArray());
        }
    }
}
