using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mono.Cecil;
using SCIL.Logger;
using SCIL.Processor;
using SCIL.Processor.FlixInstructionGenerators;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL
{
    public class VisitorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public VisitorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<IVisitor> GetVisitors()
        {
            return _serviceProvider.GetServices<IVisitor>().Select(visitor => new
                {
                    visitor,
                    attribute = visitor.GetType().GetCustomAttribute<RegistrerVisitorAttribute>()
                }).Where(e => e.attribute != null)
                .Where(e => !e.attribute.Ignored)
                .OrderBy(e => e.attribute.Order)
                .Select(e => e.visitor);
        }
    }

    public class ModuleProcessor
    {
        public ModuleProcessor(ILogger logger, FlixCodeGeneratorFactory flixCodeGeneratorFactory, ControlFlowGraph controlFlowGraph, VisitorFactory visitorFactory, Configuration configuration)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            FlixCodeGeneratorFactory = flixCodeGeneratorFactory ?? throw new ArgumentNullException(nameof(flixCodeGeneratorFactory));
            ControlFlowGraph = controlFlowGraph ?? throw new ArgumentNullException(nameof(controlFlowGraph));
            VisitorFactory = visitorFactory ?? throw new ArgumentNullException(nameof(visitorFactory));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public ILogger Logger { get; }

        public FlixCodeGeneratorFactory FlixCodeGeneratorFactory { get; }

        public ControlFlowGraph ControlFlowGraph { get; }
        public VisitorFactory VisitorFactory { get; }

        public Configuration Configuration { get; }

        public async Task<string> ReadModule(ModuleDefinition module)
        {
            // Check if we should ignore the module 
            if (Configuration.ExcludedModules.Contains(module.Name))
            {
                Logger.Log("[Skipped]: " + module.Name);
                return null;
            }

            Logger.Log("[Analyzing]: " + module.Name);

            // Get visitors
            var visitors = VisitorFactory.GetVisitors();

            // Run all visitors
            var moduleBlock = ControlFlowGraph.GenerateModule(module);
            foreach (var visitor in visitors)
            {
                visitor.Visit(moduleBlock);
            }

            // Run code generator
            var codeGeneratorVisitor = FlixCodeGeneratorFactory.Generate();
            codeGeneratorVisitor.Visit(moduleBlock);

            // Write output to file
            var file = new FileInfo(Path.Combine(Configuration.OutputPath, GetSafePath(module.Name) + ".flix"));

            // Get UTF-8 encoding without BOM (default for File.CreateText which was used before) and just ignore invalid UTF-8 chars
            var encoder = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);

            using (var stream = new StreamWriter(File.Open(file.FullName, FileMode.Create), encoder))// File.CreateText(file.FullName))
            {
                await stream.WriteAsync(codeGeneratorVisitor.GetGeneratedCode());
            }

            Logger.Log("[Processed]: " + module.Name);

            // Return file name
            return file.ToString();
        }

        private static string GetSafePath(string input)
        {
            return new string(input.Where(c => Char.IsLetterOrDigit(c) || c == '.').ToArray());
        }
    }
}
