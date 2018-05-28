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
                List<Block> otherTargets = node.Block.Targets.ToList();
                foreach (var target in new List<Block>(blockTargets))
                {
                    // Only removes the first occurence
                    otherTargets.Remove(target);
                }

                // Get next target
                var lastNodeInCurrentBlock = node.Block.Nodes.Last();
                Block nextTarget = node.Block.Targets.First(e =>
                    e.Nodes.Any(n => n.Instruction == lastNodeInCurrentBlock.Instruction.Next));

                // Update other targets
                otherTargets = otherTargets.Except(new[] {nextTarget}).ToList();

                // Theese other targets should all be exception handlers if not - we have done something wrong
                Debug.Assert(otherTargets.All(target => target.Method.Definition.Body.ExceptionHandlers.Any(handler => handler.HandlerStart == target.Nodes.First().Instruction)));
                
                // Currently we have a control flow of the following
                // node.Sources -> node.block -> node.Targets (blockTargets, otherTargets, nexttarget)
                // We now want to update node.block to use branching

                // Add a list of new blocks
                var branchingBlocks = new List<Block>();
                // Start creating new instructions
                // Switch is always 0 indexed (i think)
                // Lets use dup and pop (we could have used variables, however which variable can we just use?)
                for (int i = 0; i < blockTargets.Count; i++)
                {
                    var target = blockTargets[i];

                    // Add list of new nodes we should include instead of switch node
                    var newNodes = new List<Node>();

                    // Add dup stm such that we can use it for next instruction
                    newNodes.Add(new Node(node.Instruction, node.Block) {OverrideOpCode = OpCodes.Dup});

                    // Load compare to onto stack
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Ldc_I8, OverrideOperand = (long) i });

                    // Compare the two values
                    // Compare equal stm
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Ceq });

                    // Add branch true
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Brtrue, OverrideOperand = target.Nodes.First().Instruction });

                    // Add the nodes to a new block
                    var newBlock = new Block(newNodes.ToArray()) { Method = node.Block.Method };

                    // Update the block
                    newBlock.Nodes.ForEach(n => n.Block = newBlock);

                    // Add target from new block to pop block
                    newBlock.AddTarget(target);

                    // Remove target from old block
                    node.Block.RemoveTarget(target);

                    // Add to the list of blocks
                    branchingBlocks.Add(newBlock);
                }

                // Add targets to next for all new blocks
                // Minus 1 since last should target next block and we don't want a IndexOutOfRangeException
                for (int i = 0; i < branchingBlocks.Count - 1; i++)
                {
                    branchingBlocks[i].AddTarget(branchingBlocks[i + 1]);
                }

                // Add target to first block
                node.Block.AddTarget(branchingBlocks.First());

                // Add next target for last block
                branchingBlocks.Last().AddTarget(nextTarget);

                // Remove next from current block
                node.Block.RemoveTarget(nextTarget);

                // We now have
                // node.Sources -> node.block -> branchingBlock1 -> branchingBlock2 -> node.Targets (blockTargets, otherTargets, nexttarget)

                // Add exception targets to all blocks
                foreach (var block in branchingBlocks)
                {
                    foreach (var otherTarget in otherTargets)
                    {
                        block.AddTarget(otherTarget);
                    }
                }

                // Add the new blocks to the method
                var indexOfSwitch = node.Block.Method.Blocks.ToList().IndexOf(node.Block);

                // We need to insert just after the switch
                node.Block.Method.Insert(indexOfSwitch + 1, branchingBlocks.ToArray());

                // We now insert pop to next block (after switch)
                //InsertPopBeforeTarget(nextTarget, branchingBlocks, otherTargets);
                // This is combined now since it can also be a seperate target, and we don't want to add multiple pop (since we then will get errors)

                // We now needs to insert pop stm to remove all the extra values
                InsertPopToAllTargets(blockTargets.Concat(new [] {nextTarget}).ToList(), branchingBlocks, otherTargets);

                // Remove the switch
                RemoveSwitchFromCurrentBlock(node, branchingBlocks, otherTargets);
            }

            base.Visit(node);
        }

        private void InsertPopBeforeTarget(Block target, List<Block> branchingBlocks, List<Block> exceptionTargets)
        {
            var otherSources = target.Sources.Except(branchingBlocks);
            if (!otherSources.Any())
            {
                // We can safely just insert a pop instruction into the nodes
                target.InsertNodesAtIndex(0,
                    new Node(target.Nodes.First().Instruction, target) { OverrideOpCode = OpCodes.Pop });
            }
            else
            {
                // We need to insert a new block... :(
                Block newBlock =
                    new Block(new Node(target.Nodes.First().Instruction, target) { OverrideOpCode = OpCodes.Pop }) { Method = target.Method };
                newBlock.Nodes.ForEach(n => n.Block = newBlock);

                // We now add the sources (and removes them)
                var sourcesToAdd = target.Sources.Except(otherSources).ToList();
                foreach (var source in sourcesToAdd)
                {
                    source.AddTarget(newBlock);
                    source.RemoveTarget(target);
                }

                // Add target to the new block
                newBlock.AddTarget(target);

                // We now add the exception targets
                foreach (var exceptionTarget in exceptionTargets)
                {
                    newBlock.AddTarget(exceptionTarget);
                }

                // Add the new blocks to the method
                var indexOfTarget = target.Method.Blocks.ToList().IndexOf(target);

                // We need to insert just before the target
                target.Method.Insert(indexOfTarget, newBlock);
            }
        }

        private void InsertPopToAllTargets(List<Block> blockTargets, List<Block> branchingBlocks, List<Block> exceptionTargets)
        {
            foreach (var target in blockTargets.Distinct())
            {
                InsertPopBeforeTarget(target, branchingBlocks, exceptionTargets);
            }
        }

        private void RemoveSwitchFromCurrentBlock(Node node, List<Block> newBlocks, List<Block> exceptionTargets)
        {
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
                foreach (var other in exceptionTargets)
                {
                    node.Block.RemoveTarget(other);
                }

                Debug.Assert(!node.Block.Targets.Any());
                Debug.Assert(!node.Block.Sources.Any());
            }
        }
    }
}
