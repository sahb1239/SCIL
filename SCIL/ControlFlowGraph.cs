using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL
{
    class ControlFlowGraph
    {
        public static Block GenerateBlock(MethodDefinition method)
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

            return startBlock;
        }

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
                return Instruction.Offset.ToString();
            }
        }

        public class Block
        {
            private readonly List<Block> _targets = new List<Block>();
            private readonly List<Block> _sources = new List<Block>();
            private readonly List<Node> _nodes = new List<Node>();

            public Block(params Instruction[] instructions)
            {
                _nodes.AddRange(instructions.Select(instruction => new Node(instruction, this)));
            }

            public void AddTarget(Block target)
            {
                _targets.Add(target);
                target._sources.Add(this);
            }

            public bool CanConcat(Block block)
            {
                if (this.Targets.Count() != 1)
                    return false;
                if (this.Targets.First() != block)
                    return false;

                if (block.Sources.Count() != 1)
                    return false;
                if (block.Sources.First() != this)
                    return false;

                return true;
            }

            public void ConcatBlock(Block block)
            {
                if (!CanConcat(block))
                    throw new InvalidOperationException("Cannot concat with the block");

                // Add nodes to this block
                _nodes.AddRange(block.Nodes);

                // Update all nodes and set block to this block
                _nodes.ForEach(node => node.Block = this);

                // Clear our targets
                _targets.Clear();
                _targets.AddRange(block.Targets);

                // Update next sources
                foreach (var target in _targets)
                {
                    // Remove our old source
                    target._sources.Remove(block);
                    target._sources.Add(this);
                }
            }

            public IEnumerable<Node> Nodes => _nodes;

            public IEnumerable<Block> Targets => _targets.AsReadOnly();

            public IEnumerable<Block> Sources => _sources.AsReadOnly();

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                foreach (var node in _nodes)
                {
                    sb.AppendLine(node.ToString());
                }

                if (_targets.Count > 1)
                {
                    // Convert targets to string
                    List<string[]> targetStrings = _targets.Select(e => e.ToString().Split(Environment.NewLine)).ToList();

                    // Get the target with the max number of strings in it
                    int maxLengthTargets = targetStrings.Max(t => t.Length);

                    // Add string builders
                    var stringBuilders = new List<StringBuilder>();
                    for(int i = 0; i < maxLengthTargets; i++)
                        stringBuilders.Add(new StringBuilder());

                    // Add each target
                    for (int targetIndex = 0; targetIndex < targetStrings.Count; targetIndex++)
                    {
                        string[] target = targetStrings[targetIndex];

                        // Get target line max length
                        int targetMaxLength = target.Max(e => e.Length);

                        // Add start char to stringBuilder
                        stringBuilders.ForEach(s => s.Append("|"));

                        // Loop throug all length
                        for (int lineIndex = 0; lineIndex < maxLengthTargets; lineIndex++)
                        {
                            stringBuilders[lineIndex].Append(' ');
                            if (target.Count() - 1 < lineIndex)
                            {
                                stringBuilders[lineIndex].Append(' ', targetMaxLength);
                            }
                            else
                            {
                                stringBuilders[lineIndex].Append(target[lineIndex].PadLeft(targetMaxLength));
                            }
                        }

                    }

                    // Add end char to stringBuilder
                    stringBuilders.ForEach(s => s.Append("|"));

                    // Add all stringBuilders to base builder
                    stringBuilders.ForEach(s => sb.AppendLine(s.ToString()));
                }
                else if (_targets.Count == 1)
                {
                    sb.AppendLine(_targets.First().ToString());
                }

                return sb.ToString();
            }
        }
    }

   
}
