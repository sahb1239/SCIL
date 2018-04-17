using System;
using Mono.Cecil.Cil;

namespace SCIL
{
    public class Node
    {
        public Node(Instruction instruction, Block block)
        {
            Instruction = instruction ?? throw new ArgumentNullException(nameof(instruction));
            Block = block ?? throw new ArgumentNullException(nameof(block));
        }

        public Instruction Instruction { get; }
        public Block Block { get; set; }

        public override string ToString()
        {
            return Instruction.ToString();
        }
    }
}