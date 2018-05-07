using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CSharpx;
using Mono.Cecil.Cil;
using SCIL.Processor.Nodes;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.Simplifiers
{
    [RegistrerRewriter]
    public class SwitchRewriterVisitor : BaseVisitor
    {
        public override void Visit(Node node)
        {
            if (node.OpCode.Code == Code.Switch)
            {
                // Get operand
                var operand = (IEnumerable<Instruction>) node.Operand;

                // Get blocks matching operand
                List<Block> blockTargets = operand.Select(targetInstruction => node.Block.Targets.First(targetBlock =>
                    targetBlock.Nodes.FirstOrDefault()?.Instruction == targetInstruction)).ToList();

                // Get next target
                List<Block> otherTargets = node.Block.Targets.Except(blockTargets).ToList();

                // Get next target
                Block nextTarget;
                if (otherTargets.Count == 1)
                {
                    nextTarget = otherTargets.First();
                }
                else
                {
                    var lastNodeInCurrentBlock = node.Block.Nodes.Last();
                    nextTarget = otherTargets.Single(e =>
                        e.Nodes.Any(n => n.Instruction == lastNodeInCurrentBlock.Instruction.Next));
                }

                // Update other targets
                otherTargets = otherTargets.Except(new[] {nextTarget}).ToList();

                // Theese other targets should all be exception handlers if not - we have done something wrong
                Debug.Assert(otherTargets.All(target => target.Method.Definition.Body.ExceptionHandlers.Any(handler => handler.HandlerStart == target.Nodes.First().Instruction)));
                
                // Add a list of new blocks
                var newBlocks = new List<Block>();

                // TODO: We need to split the blocks
                // Start creating new instructions
                // Switch is always 0 indexed (i think)
                // Lets use dup and pop (we could have used variables, however which variable can we just use?)
                for (int i = 0; i < blockTargets.Count; i++)
                {
                    var target = blockTargets[i];

                    // Add list of new nodes we should include instead of switch node
                    var newNodes = new List<Node>();

                    // Add dup stm such that we can use it for next instruction
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Dup });

                    // Load compare to onto stack
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Ldc_I4, OverrideOperand = i });

                    // Compare the two values
                    // Compare equal stm
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Ceq });

                    // Add branch true
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Brtrue, OverrideOperand = target.Nodes.First().Instruction });

                    // Add the nodes to a new block
                    var newBlock = new Block(newNodes.ToArray()) {Method = node.Block.Method};

                    // Update the block
                    newBlock.Nodes.ForEach(n => n.Block = newBlock);

                    // Add target to the new block
                    newBlock.AddTarget(target);

                    // Remove target from old block
                    node.Block.RemoveTarget(target);

                    // Add to new blocks
                    newBlocks.Add(newBlock);
                }

                // Add targets to next for all new blocks
                // Minus 1 since last should target next block and we don't want a IndexOutOfRangeException
                for (int i = 0; i < newBlocks.Count - 1; i++)
                {
                    newBlocks[i].AddTarget(newBlocks[i + 1]);
                }

                // Add target to first block
                node.Block.AddTarget(newBlocks.First());

                // Add next target for last block
                newBlocks.Last().AddTarget(nextTarget);

                // Remove next from current block
                node.Block.RemoveTarget(nextTarget);

                // Add other targets to all blocks
                foreach (var block in newBlocks)
                {
                    foreach (var otherTarget in otherTargets)
                    {
                        block.AddTarget(otherTarget);
                    }
                }

                // Add pop instruction to each target
                foreach (var target in blockTargets.Distinct())
                {
                    // Add pop instruction at target to move duplicated variable on stack we inserted using dup
                    target.InsertNodesAtIndex(0,
                        new Node(target.Nodes.First().Instruction, target) { OverrideOpCode = OpCodes.Pop });
                }

                // Add pop to next block (to remove dup from all the branching)
                nextTarget.InsertNodesAtIndex(0,
                    new Node(nextTarget.Nodes.First().Instruction, nextTarget) {OverrideOpCode = OpCodes.Pop});
                
                // Add the new blocks to the method
                var indexOfSwitch = node.Block.Method.Blocks.ToList().IndexOf(node.Block);

                // We need to insert just after the switch
                node.Block.Method.Insert(indexOfSwitch + 1, newBlocks.ToArray());

                // Remove the switch from the current block
                node.Block.Remove(node);

                // If the block is now empty we need to remove it
                if (!node.Block.Nodes.Any())
                {
                    node.Block.Method.Remove(node.Block);

                    // Fix sources and add target to new block
                    foreach (var source in node.Block.Sources.ToList())
                    {
                        // source -> node.Block -> newBlocks.First()
                        // Remove the node from source (first arrow)
                        source.RemoveTarget(node.Block);

                        // Remove target to the first new block (second arrow)
                        node.Block.RemoveTarget(newBlocks.First());

                        // Add target from the old source to new new first block (add connection between source and newBlocks.First())
                        source.AddTarget(newBlocks.First());
                    }

                    // Remove exception handlers
                    foreach (var other in otherTargets)
                    {
                        node.Block.RemoveTarget(other);
                    }

                    Debug.Assert(!node.Block.Targets.Any());
                    Debug.Assert(!node.Block.Sources.Any());
                }
            }

            base.Visit(node);
        }
    }
}
