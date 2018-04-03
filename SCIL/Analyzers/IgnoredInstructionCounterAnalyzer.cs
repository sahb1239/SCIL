using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Analyzers
{
    [EmitterOrder(1000)]
    class IgnoredInstructionCounterAnalyzer : IFlixInstructionGenerator, IInstructionAnalyzer
    {
        private readonly IDictionary<string, long> _count = new ConcurrentDictionary<string, long>();

        public string GetCode(MethodBody methodBody, Instruction instruction, IFlixInstructionProgramState programState)
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
            yield return "Instructions not handled:";
            foreach (var instruction in _count.OrderByDescending(e => e.Value))
            {
                yield return instruction.Key + ": " + instruction.Value;
            }

            yield return "Total ignored instructions: " + _count.Count;
            yield return "Total ignored instructions count: " + _count.Sum(e => e.Value);
        }
    }
}
