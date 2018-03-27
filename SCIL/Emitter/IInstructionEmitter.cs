using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL
{
    interface IInstructionEmitter
    {
        InstructionEmitterOutput GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction);
    }

    class InstructionEmitterOutput
    {
        public InstructionEmitterOutput(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction,
            string flixStmFormatString, bool push, uint pop)
        {
            TypeDefinition = typeDefinition ?? throw new ArgumentNullException(nameof(typeDefinition));
            MethodBody = methodBody ?? throw new ArgumentNullException(nameof(methodBody));
            Instruction = instruction ?? throw new ArgumentNullException(nameof(instruction));
            FlixStmFormatString = flixStmFormatString ?? throw new ArgumentNullException(nameof(flixStmFormatString));
            Push = push;
            Pop = pop;
        }

        public InstructionEmitterOutput(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction,
            string flixStmFormatString, bool push, uint pop, bool peek)
        {
            TypeDefinition = typeDefinition ?? throw new ArgumentNullException(nameof(typeDefinition));
            MethodBody = methodBody ?? throw new ArgumentNullException(nameof(methodBody));
            Instruction = instruction ?? throw new ArgumentNullException(nameof(instruction));
            FlixStmFormatString = flixStmFormatString ?? throw new ArgumentNullException(nameof(flixStmFormatString));
            Push = push;
            Pop = pop;
            Peek = peek;
        }

        public static InstructionEmitterOutput Create(TypeDefinition typeDefinition, MethodBody methodBody,
            Instruction instruction,
            string flixStmFormatString, bool push, uint pop) => new InstructionEmitterOutput(typeDefinition, methodBody,
            instruction, flixStmFormatString, push, pop);

        public TypeDefinition TypeDefinition { get; set; }
        public MethodBody MethodBody { get; set; }
        public Instruction Instruction { get; set; }

        public string FlixStmFormatString { get; }
        public bool Push { get; set; }
        public uint Pop { get; set; }
        public bool Peek { get; set; }
    }
}