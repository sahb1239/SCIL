using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.Nodes
{
    [DebuggerDisplay("{Definition.Name}")]
    public class Method : Element
    {
        private readonly List<Block> _blocks = new List<Block>();

        public Method(MethodDefinition method, Block startBlock, IEnumerable<Block> blocks)
        {
            Definition = method ?? throw new ArgumentNullException(nameof(method));
            StartBlock = startBlock;

            if (blocks == null) throw new ArgumentNullException(nameof(blocks));
            _blocks.AddRange(blocks);

            // Update method for each block
            _blocks.ForEach(block => block.Method = this);
        }

        public IReadOnlyCollection<Block> Blocks => _blocks.AsReadOnly();
        public MethodDefinition Definition { get; }
        public Block StartBlock { get; }
        public Type Type { get; set; }

        public void Insert(int index, params Block[] newBlocks)
        {
            // Check index
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Invalid index");
            }

            // Insert new blocks
            _blocks.InsertRange(index, newBlocks);
        }

        public void Replace(Block block, params Block[] newBlocks)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));

            // Get block
            var index = _blocks.IndexOf(block);
            if (index == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(block), "Block not found");
            }

            // Remove and insert new blocks at index
            _blocks.RemoveAt(index);
            _blocks.InsertRange(index, newBlocks);
        }

        public void Remove(Block block)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));

            // Get block
            var index = _blocks.IndexOf(block);
            if (index == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(block), "Block not found");
            }

            // Remove block at index
            _blocks.RemoveAt(index);
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}