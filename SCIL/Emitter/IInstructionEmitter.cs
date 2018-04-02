using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL
{
    interface IFlixInstructionGenerator
    {
        string GetCode(MethodBody methodBody, Instruction instruction, IFlixInstructionProgramState programState);
    }

    interface IFlixInstructionProgramState
    {
        string PeekStack();
        string PopStack();
        string PushStack();

        string GetArg(uint argno);
        string StoreArg(uint argno);

        string GetVar(uint varno);
        string StoreVar(uint varno);
    }

    public class ProgramStateException : Exception
    {
        public ProgramStateException(string message) : base(message)
        {
        }
    }
}