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
using SCIL.Processor.Visitors;
using SCIL.Writer;

namespace SCIL
{
    class ModuleProcessor
    {
        public IReadOnlyCollection<IOldFlixInstructionGenerator> Generators { get; }
        public IModuleWriter ModuleWriter { get; }
        public ILogger Logger { get; }

        private Lazy<IReadOnlyCollection<IInstructionAnalyzer>> Analyzers =>
            new Lazy<IReadOnlyCollection<IInstructionAnalyzer>>(() =>
                new ReadOnlyCollection<IInstructionAnalyzer>(Generators.OfType<IInstructionAnalyzer>().ToList()));

        public ModuleProcessor(IReadOnlyCollection<IOldFlixInstructionGenerator> generators, IModuleWriter moduleWriter, ILogger logger)
        {
            Generators = generators;
            ModuleWriter = moduleWriter;
            Logger = logger;
        }

        public async Task ProcessAssembly(Stream stream, IEnumerable<string> excluded)
        {
            // Resetting analyzers
            Logger.Log("Resetting analyzers", true);
            Analyzers.Value.ForEach(analyzer => analyzer.Reset());

            // Load Module using Mono.Cecil
            Logger.Log("Loading module", true);
            using (var module = ModuleDefinition.ReadModule(stream))
            {
                if (excluded.Contains(module.Name))
                {
                    Logger.Log("Skipping excluded module: " + module.Name);
                    return;
                }
                else
                {
                    Logger.Log("Results from module " + module.Name);
                    await ReadModule(module).ConfigureAwait(false);
                }
            }

            // Print output from analyzers
            Logger.Log(string.Join(Environment.NewLine + Environment.NewLine,
                           Analyzers.Value.Select(analyzer => string.Join(Environment.NewLine, analyzer.GetOutput()))) +
                       Environment.NewLine);
        }

        private async Task ReadModule(ModuleDefinition module)
        {
            Logger.Log("Reading module", true);

            using (var moduleWriter = await ModuleWriter.GetAssemblyModuleWriter(module.Name).ConfigureAwait(false)) {

                foreach (var type in module.Types)
                {
                    Logger.Log($"Processing type {type.FullName}", true);

                    using (var typeModuleWriter = await moduleWriter.GetTypeModuleWriter(type).ConfigureAwait(false))
                    {
                        foreach (var methodDefinition in type.Methods)
                        {
                            Logger.Log($"Processing method {methodDefinition.Name}", true);

                            if (methodDefinition.HasBody)
                            {
                                await typeModuleWriter
                                    .WriteMethod(type, methodDefinition, ProcessCIL(methodDefinition.Body))
                                    .ConfigureAwait(false);
                            }
                            else
                            {
                                await typeModuleWriter.WriteMethod(type, methodDefinition).ConfigureAwait(false);
                            }
                        }
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
            var methodName = _methodBody.Method.FullName;

            if (_poppedStack.Any())
            {
                var pop = _poppedStack.Pop();
                _stack.Push(pop);

                string indexName = $"\"{methodName}_{index}_{pop.Count}\"";
                pop.Add(indexName);
                return indexName;
            }
            else
            {
                string indexName = $"\"{methodName}_{index}\"";
                _stack.Push(new List<string> { indexName });
                return indexName;
            }
        }

        public string GetArg(uint argno)
        {
            if (!_argList.ContainsKey(argno))
            {
                // Not good..
                return StoreArg(argno);
            }
            return _argList[argno].Last();
        }

        public string StoreArg(uint argno)
        {
            var methodName = _methodBody.Method.FullName;
            string indexName;
            if (_argList.ContainsKey(argno))
            {
                indexName = $"\"{methodName}_{argno}_{_argList[argno].Count}\"";
                _argList[argno].Add(indexName);
            }
            else
            {
                indexName = $"\"{methodName}_{argno}\"";
                _argList.Add(argno, new List<string> { indexName });
            }
            return indexName;
        }

        public string GetStoreArg(MethodReference method, uint argno)
        {
            var methodName = method.FullName;
            return $"\"{methodName}\"";
        }

        public string GetVar(uint varno)
        {
            if (!_storeVar.ContainsKey(varno))
            {
                // Not good..
                // It could be a struct and therefore we just bury our head in the sand and stores the variable
                return StoreVar(varno);
            }
            return _storeVar[varno].Last();
        }

        public string StoreVar(uint varno)
        {
            var methodName = _methodBody.Method.FullName;
            string indexName;
            if (_storeVar.ContainsKey(varno))
            {
                indexName = $"\"{methodName}_{varno}_{_storeVar[varno].Count}\"";
                _storeVar[varno].Add(indexName);
            }
            else
            {
                indexName = $"\"{methodName}_{varno}\"";
                _storeVar.Add(varno, new List<string> {indexName});
            }
            return indexName;
        }

        public string GetField(string fieldName)
        {
            return $"\"{fieldName}\"";
        }

        public string StoreField(string fieldName)
        {
            return $"\"{fieldName}\"";
        }
    }
}
