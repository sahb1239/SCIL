using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SCIL.Logger;

namespace SCIL.Analyzers
{
    [EmitterOrder(0), IgnoreEmitter]
    class InstructionCounter : IInstructionEmitter, IInstructionAnalyzer
    {
        private readonly IDictionary<string, long> _count = new ConcurrentDictionary<string, long>();
        public string GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            var key = typeDefinition.Module.Name;
            if (_count.ContainsKey(key))
            {
                _count[key]++;
            }
            else
            {
                _count.Add(key, 1);
            }

            return null;
        }

        public void Reset()
        {
            _count.Clear();
        }

        public IEnumerable<string> GetOutput()
        {
            yield return "Number of instructions in modules:";
            foreach (var (moduleName, instructionsCount) in GetInstructions())
            {
                yield return string.Join(": ", moduleName, instructionsCount);
            }
        }

        public IEnumerable<(string moduleName, long instructionsCount)> GetInstructions()
        {
            foreach (var c in _count)
            {
                yield return (c.Key, c.Value);
            }
        }
    }
}
