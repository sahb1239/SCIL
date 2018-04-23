using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SCIL.Processor.Nodes;
using Type = SCIL.Processor.Nodes.Type;

namespace SCIL
{
    public class ControlFlowGraph
    {
        public static Module GenerateModule(ModuleDefinition module)
        {
            List<Type> types = new List<Type>();

            foreach (var type in module.Types)
            {
                types.Add(GenerateType(type));
            }

            return new Module(module, types);
        }

        private static Type GenerateType(TypeDefinition type)
        {
            List<Method> methods = new List<Method>();

            foreach (var method in type.Methods)
            {
                methods.Add(GenerateMethod(method));
            }

            return new Type(type, methods);
        }

        private static Method GenerateMethod(MethodDefinition method)
        {
            // Generate blocks
            var blocks = method.Body.Instructions.Select(instruction => new Block(instruction)).ToList();

            // Set start block
            Block startBlock = blocks.First();

            // Analyze each block
            for (var index = 0; index < blocks.Count; index++)
            {
                // Get current block
                var block = blocks[index];
                
                // Assert that the block only contains one node
                Debug.Assert(block.Nodes.Count() == 1);

                // Get node
                var node = block.Nodes.First();

                switch (node.Instruction.OpCode.FlowControl)
                {
                    case FlowControl.Call:
                        // TODO: Add call target
                    case FlowControl.Next:
                        block.AddTarget(blocks[index + 1]);
                        break;
                    case FlowControl.Branch:
                        Instruction branchToInstruction = (Instruction) node.Instruction.Operand;
                        Block branchTargetBlock =
                            blocks.Single(bl => bl.Nodes.Any(e => e.Instruction == branchToInstruction));

                        block.AddTarget(branchTargetBlock);
                        break;
                    case FlowControl.Cond_Branch:
                        // Add next
                        block.AddTarget(blocks[index + 1]);

                        // Add branch target
                        Instruction condBranchToInstruction = (Instruction)node.Instruction.Operand;
                        Block condBranchTargetBlock =
                            blocks.Single(bl => bl.Nodes.Any(e => e.Instruction == condBranchToInstruction));

                        block.AddTarget(condBranchTargetBlock);
                        break;
                    case FlowControl.Return:
                        break;
                    case FlowControl.Throw:
                        break;
                    default:
                        Console.WriteLine("Not handled!");
                        break;
                }
            }

            // Analyze the exception handlers and add as target for each
            foreach (var handler in method.Body.ExceptionHandlers)
            {
                // TODO: Filter is not avalible in C# and is therefore not handled here
                // Get target block
                Instruction catchInstructionStart = handler.HandlerStart;
                Block catchInstructionBlock =
                    blocks.Single(bl => bl.Nodes.Any(e => e.Instruction == catchInstructionStart));

                Instruction currentInstruction = handler.TryStart;
                do
                {
                    // Get currentInstruction block
                    var currentInstructionBlock =
                        blocks.Single(bl => bl.Nodes.Any(e => e.Instruction == currentInstruction));

                    // Add target
                    currentInstructionBlock.AddTarget(catchInstructionBlock);

                    // Set next instruction
                    if (currentInstruction.Next == null) break;
                    currentInstruction = currentInstruction.Next;
                } while (currentInstruction.Previous == null || currentInstruction != handler.TryEnd);
            }

            // Remove all which cannot be targeted (remove dead code)
            // Only very simple dead code is removed
            // Skip first since it does not have any sources (maybe)
            List<Block> removeBlocks = new List<Block>();
            foreach (var block in blocks.Skip(1))
            {
                // Test if the block is not targeted by anyone
                if (!block.Sources.Any())
                {
                    removeBlocks.Add(block);
                }
            }

            // Remove the blocks
            removeBlocks.ForEach(block => blocks.Remove(block));

            // Optimize graph
            for (var index = 0; index < blocks.Count; index++)
            {
                var block = blocks[index];

                // While can concat with the next block
                while (index + 1 < blocks.Count && block.CanConcat(blocks[index + 1]))
                {
                    // Concat the block
                    block.ConcatBlock(blocks[index + 1]);

                    // Remove the block
                    blocks.RemoveAt(index + 1);
                }
            }

            // Calculate reachable blocks
            var reachableBlocks = new List<Block>();
            var pendingBlocks = new List<Block> {startBlock};

            while (pendingBlocks.Any())
            {
                // Get first pending and remove it from the pending list
                var pendingBlock = pendingBlocks.First();
                pendingBlocks.RemoveAt(0);

                // Detect if we not have already processed the block
                if (reachableBlocks.Contains(pendingBlock))
                {
                    continue;
                }

                // Add to reachableBlocks
                reachableBlocks.Add(pendingBlock);

                // Add all targets to pendingBlocks
                foreach (var target in pendingBlock.Targets)
                {
                    pendingBlocks.Add(target);
                }
            }

            // Get method block
            var methodBlock = new Method(method, startBlock, reachableBlocks);
            return methodBlock;
        }

        
    }
}
