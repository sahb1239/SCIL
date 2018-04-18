﻿using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL
{
    interface IOldFlixInstructionGenerator
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
        string GetStoreArg(MethodReference method, uint argno);

        string GetVar(uint varno);
        string StoreVar(uint varno);

        string GetField(string fieldName);
        string StoreField(string fieldName);
    }

    public class ProgramStateException : Exception
    {
        public ProgramStateException(string message) : base(message)
        {
        }
    }
}