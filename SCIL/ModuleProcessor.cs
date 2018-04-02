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
        public IReadOnlyCollection<IFlixInstructionGenerator> Generators { get; }
        public IModuleWriter ModuleWriter { get; }
        public ILogger Logger { get; }

        private Lazy<IReadOnlyCollection<IInstructionAnalyzer>> Analyzers =>
            new Lazy<IReadOnlyCollection<IInstructionAnalyzer>>(() =>
                new ReadOnlyCollection<IInstructionAnalyzer>(Generators.OfType<IInstructionAnalyzer>().ToList()));

        public ModuleProcessor(IReadOnlyCollection<IFlixInstructionGenerator> generators, IModuleWriter moduleWriter, ILogger logger)
        {
            Generators = generators;
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
                        await typeModuleWriter.WriteMethod(type, methodDefinition, ProcessCIL(methodDefinition.Body))
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        await typeModuleWriter.WriteMethod(type, methodDefinition).ConfigureAwait(false);
                    }
                }
            }
        }
        
        private string ProcessCIL(MethodBody methodBody)
        {
            var methodState = new FlixInstructionProgramState(methodBody);

            StringBuilder builder = new StringBuilder();
            foreach (var instruction in methodBody.Instructions)
            {
                foreach (var emitter in Generators)
                {
                    string output = emitter.GetCode(methodBody, instruction, methodState);
                    if (output == null)
                        continue;

                    builder.AppendLine(output);
                    break;
                }
            }
            return builder.ToString();
        }
    }

    public class FlixInstructionProgramState : IFlixInstructionProgramState
    {
        private readonly MethodBody _methodBody;

        private readonly Stack<List<string>> _stack = new Stack<List<string>>();
        private readonly Stack<List<string>> _poppedStack = new Stack<List<string>>();

        private readonly IDictionary<uint, List<string>> _argList = new Dictionary<uint, List<string>>();
        private readonly IDictionary<uint, List<string>> _storeVar = new Dictionary<uint, List<string>>();

        public FlixInstructionProgramState(MethodBody methodBody)
        {
            _methodBody = methodBody;
        }

        public string PeekStack()
        {
            return _stack.Peek().Last();
        }

        public string PopStack()
        {
            var popped = _stack.Pop();
            _poppedStack.Push(popped);
            return popped.Last();
        }

        public string PushStack()
        {
            var index = _stack.Count;

            if (_poppedStack.Any())
            {
                var pop = _poppedStack.Pop();
                _stack.Push(pop);

                string indexName = $"{index}_{pop.Count}";
                pop.Add(indexName);
                return indexName;
            }
            else
            {
                string indexName = index.ToString();
                _stack.Push(new List<string> { indexName });
                return indexName;
            }
        }

        public string GetArg(uint argno)
        {
            return argno.ToString();
            //return _argList[argno].Last();
        }

        public string StoreArg(uint argno)
        {
            return argno.ToString();
            var indexName = $"{argno}_{_argList[argno].Count}";
            _argList[argno].Add(indexName);
            return indexName;
        }

        public string GetVar(uint varno)
        {
            return _storeVar[varno].Last();
        }

        public string StoreVar(uint varno)
        {
            var indexName = $"{varno}_{_storeVar[varno].Count}";
            _storeVar[varno].Add(indexName);
            return indexName;
        }
    }
}
