using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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

        public ModuleProcessor(IReadOnlyCollection<IInstructionEmitter> emitters, IModuleWriter moduleWriter, ILogger logger)
        {
            Emitters = emitters;
            ModuleWriter = moduleWriter;
            Logger = logger;
        }

        public async Task ProcessAssembly(Stream stream)
        {
            Logger.Log("Loading module", true);

            using (var module = ModuleDefinition.ReadModule(stream))
            {
                await ReadModule(module).ConfigureAwait(false);
            }
        }

        private async Task ReadModule(ModuleDefinition module)
        {
            Logger.Log("Reading module", true);

            foreach (var type in module.Types)
            {
                Logger.Log($"Processing type {type.FullName}", true);
                await ModuleWriter.WriteType(type).ConfigureAwait(false);

                foreach (var methodDefinition in type.Methods)
                {
                    Logger.Log($"Processing method {methodDefinition.Name}", true);

                    if (methodDefinition.HasBody)
                    {
                        await ModuleWriter.WriteMethod(type, methodDefinition, ProcessCIL(type, methodDefinition.Body))
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        await ModuleWriter.WriteMethod(type, methodDefinition).ConfigureAwait(false);
                    }
                }
            }
        }
        
        private string ProcessCIL(TypeDefinition typeDefinition, MethodBody methodBody)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var instruction in methodBody.Instructions)
            {
                bool foundEmitter = false;
                foreach (var emitter in Emitters)
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
                    Logger.Log($"Error: No emitter found for code {instruction.OpCode.Name}");
                    Logger.Wait();
                }
            }
            return builder.ToString();
        }
    }
}
