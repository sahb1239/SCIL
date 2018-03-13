using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Analyzers
{
    [EmitterOrder(0), IgnoreEmitter]
    class InstructionCounterAnalyzer : IInstructionEmitter, IInstructionAnalyzer
    {
        private readonly IDictionary<string, long> _count = new ConcurrentDictionary<string, long>();
        public string GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            var key = instruction.OpCode.Name;
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
            yield return "Instructions:";
            foreach (var instruction in _count.OrderByDescending(e => e.Value))
            {
                yield return instruction.Key + ": " + instruction.Value;
            }
        }
    }
}