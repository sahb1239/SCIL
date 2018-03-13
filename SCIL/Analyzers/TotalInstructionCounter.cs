using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SCIL.Logger;

namespace SCIL.Analyzers
{
    [EmitterOrder(0), IgnoreEmitter]
    class TotalInstructionCounter : IInstructionEmitter, IInstructionAnalyzer
    {
        private long _count = 0;
        public string GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            _count++;

            return null;
        }

        public void Reset()
        {
            _count = 0;
        }

        public IEnumerable<string> GetOutput()
        {
            yield return "Number of instructions: " + _count;
        }
    }
}
