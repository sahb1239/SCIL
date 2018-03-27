using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpx;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SCIL.Logger;
using SCIL.Writer;

namespace SCIL
{
    class ModuleProcessor
    {
        public IReadOnlyCollection<IInstructionEmitter> Emitters { get; }
        public IModuleWriter ModuleWriter { get; }
        public ILogger Logger { get; }

        private Lazy<IReadOnlyCollection<IInstructionAnalyzer>> Analyzers =>
            new Lazy<IReadOnlyCollection<IInstructionAnalyzer>>(() =>
                new ReadOnlyCollection<IInstructionAnalyzer>(Emitters.OfType<IInstructionAnalyzer>().ToList()));

        public ModuleProcessor(IReadOnlyCollection<IInstructionEmitter> emitters, IModuleWriter moduleWriter, ILogger logger)
        {
            Emitters = emitters;
            ModuleWriter = moduleWriter;
            Logger = logger;
        }

        public async Task ProcessAssembly(Stream stream)
        {
            // Resetting analyzers
            Logger.Log("Resetting analyzers", true);
            Analyzers.Value.ForEach(analyzer => analyzer.Reset());

            // Load Module using Mono.Cecil
            Logger.Log("Loading module", true);
            using (var module = ModuleDefinition.ReadModule(stream))
            {
                Logger.Log("Results from module " + module.Name);

                await ReadModule(module).ConfigureAwait(false);
            }

            // Print output from analyzers
            Logger.Log(string.Join(Environment.NewLine + Environment.NewLine,
                           Analyzers.Value.Select(analyzer => string.Join(Environment.NewLine, analyzer.GetOutput()))) +
                       Environment.NewLine);
        }

        private async Task ReadModule(ModuleDefinition module)
        {
            Logger.Log("Reading module", true);

            var moduleWriter = await ModuleWriter.GetAssemblyModuleWriter(module.Name).ConfigureAwait(false);

            foreach (var type in module.Types)
            {
                Logger.Log($"Processing type {type.FullName}", true);

                var typeModuleWriter = await moduleWriter.GetTypeModuleWriter(type).ConfigureAwait(false);

                foreach (var methodDefinition in type.Methods)
                {
                    Logger.Log($"Processing method {methodDefinition.Name}", true);

                    if (methodDefinition.HasBody)
                    {
                        await typeModuleWriter.WriteMethod(type, methodDefinition, ProcessCIL(type, methodDefinition.Body))
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        await typeModuleWriter.WriteMethod(type, methodDefinition).ConfigureAwait(false);
                    }
                }
            }
        }
        
        private string ProcessCIL(TypeDefinition typeDefinition, MethodBody methodBody)
        {
            Dictionary<uint, List<string>> stackDirectory = new Dictionary<uint, List<string>>();
            long currentStackIndex = 0;

            StringBuilder builder = new StringBuilder();
            foreach (var instruction in methodBody.Instructions)
            {
                foreach (var emitter in Emitters)
                {
                    InstructionEmitterOutput emitterOutput = emitter.GetCode(typeDefinition, methodBody, instruction);
                    if (emitterOutput == null)
                        continue;
                    
                    List<string> stackElements = new List<string>();

                    // Update currentStackIndex
                    for (int i = 0; i < emitterOutput.Pop; i++)
                    {
                        stackElements.Add(GetStackIndex((uint) --currentStackIndex, stackDirectory));
                    }

                    // Peek
                    if (emitterOutput.Peek)
                    {
                        stackElements.Add(GetStackIndex((uint) currentStackIndex - 1, stackDirectory));
                    }

                    // Add push
                    if (emitterOutput.Push)
                    {
                        stackElements.Add(SetStackIndex((uint)currentStackIndex++, stackDirectory));
                    }

                    string[] stackArray = stackElements.Select(e => "\"" + methodBody.Method.FullName + "__" + e + "\"").ToArray();

                    // Append format
                    var emitterFormatOutput = string.Format(emitterOutput.FlixStmFormatString, stackArray);

                    builder.AppendLine(emitterFormatOutput);
                    break;
                }
            }
            return builder.ToString();
        }

        private string GetStackIndex(uint index, IDictionary<uint, List<string>> stackDictionary)
        {
            return stackDictionary[index].Last();
        }

        private string SetStackIndex(uint index, IDictionary<uint, List<string>> stackDictionary)
        {
            if (stackDictionary.ContainsKey(index))
            {
                string indexName = $"{index}_{stackDictionary[index].Count}";
                stackDictionary[index].Add(indexName);
                return indexName;
            }
            else
            {
                string indexName = index.ToString();
                stackDictionary.Add(index, new List<string>() {indexName});
                return indexName;
            }
        }
    }
}
