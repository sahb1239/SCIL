using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.Nodes
{
    [DebuggerDisplay("Start = {_nodes[0]}")]
    public class Block : Element
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
            if (this.Targets.Count != 1)
                return false;
            if (this.Targets.First() != block)
                return false;

            if (block.Sources.Count != 1)
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

        public void InsertNodesAtIndex(int index, params Node[] newNodes)
        {
            // Check index
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Invalid index");
            }

            // Insert new nodes
            _nodes.InsertRange(index, newNodes);
        }

        public void ReplaceNode(Node node, params Node[] newNodes)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            // Get node
            var index = _nodes.IndexOf(node);
            if (index == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(node), "Node not found");
            }

            // Remove and insert new nodes at index
            _nodes.RemoveAt(index);
            _nodes.InsertRange(index, newNodes);
        }

        public IEnumerable<Block> Union(IEnumerable<Block> blocks)
        {
            var startList = new[] {this};
            return startList.Union(blocks);
        }

        public IReadOnlyCollection<Node> Nodes => _nodes.AsReadOnly();

        public IReadOnlyCollection<Block> Targets => _targets.AsReadOnly();

        public IReadOnlyCollection<Block> Sources => _sources.AsReadOnly();

        public List<Block> DominatedBy { get; } = new List<Block>();
        public List<Block> DomninanceFrontiers { get; } = new List<Block>();

        public Method Method { get; set; }

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
                for (int i = 0; i < maxLengthTargets; i++)
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

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}