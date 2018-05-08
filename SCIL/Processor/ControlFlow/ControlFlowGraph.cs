using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSharpx;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SCIL.Processor.Nodes;
using SCIL.Processor.Nodes.Visitor;
using Type = SCIL.Processor.Nodes.Type;

namespace SCIL
{
    public class ControlFlowGraph
    {
        // Class which updates all the methods references
        private class ControlFlowMethodGeneratorVisitor : BaseVisitor
        {
            private readonly List<Method> _allMethods;

            public ControlFlowMethodGeneratorVisitor(List<Method> allMethods)
            {
                _allMethods = allMethods ?? throw new ArgumentNullException(nameof(allMethods));
            }

            public override void Visit(Node node)
            {
                switch (node.OpCode.Code)
                {
                    case Code.Call:
                    case Code.Calli:
                    case Code.Callvirt:
                        // Get operand
                        if (node.Operand is MethodReference methodReference)
                        {
                            // Detect if we can find a match for the method
                            var matchingMethod = _allMethods.FirstOrDefault(method =>
                                method.Definition == methodReference);

                            if (matchingMethod != null)
                            {
                                // Replace the node
                                node.Replace(new MethodCallNode(node, matchingMethod));
                            }
                        }

                        break;
                }
            }
        }

        public static Module GenerateModule(ModuleDefinition module)
        {
            // Generate all the types
            List<Type> types = module.Types.Select(GenerateType).ToList();

            // Create the module
            var createdModule = new Module(module, types);

            // Get all methods
            List<Method> allMethods = types.SelectMany(type => type.Methods).ToList();

            // Create visitor which should update all nodes with method calls to MethodCallNode
            var visitor = new ControlFlowMethodGeneratorVisitor(allMethods);
            visitor.Visit(createdModule);

            return createdModule;
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
            // Skip methods without body...
            if (!method.HasBody)
            {
                var emptyMethod = new Method(method, null, new List<Block>() {});
                return emptyMethod;
            }

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

                        // Handle switch
                        if (node.Instruction.OpCode.Code == Code.Switch)
                        {
                            IEnumerable<Instruction> instructions = (IEnumerable<Instruction>) node.Instruction.Operand;

                            // Add targets
                            IEnumerable<Block> targetsToAdd = instructions.Select(instruction =>
                                blocks.Single(bl => bl.Nodes.Any(e => e.Instruction == instruction)));
                            targetsToAdd.ForEach(target => block.AddTarget(target));

                            break;
                        }
                        
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

            // Order reachable blocks
            reachableBlocks = reachableBlocks.OrderBy(e => e.Nodes.First().Instruction.Offset).ToList();

            // Get method block
            var methodBlock = new Method(method, startBlock, reachableBlocks);
            return methodBlock;
        }

        
    }
}
