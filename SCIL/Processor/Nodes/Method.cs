using System;
using System.Collections.Generic;
using Mono.Cecil;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.Nodes
{
    public class Method : Element
    {
        private readonly List<Block> _blocks = new List<Block>();

        public Method(MethodDefinition method, Block startBlock, IEnumerable<Block> blocks)
        {
            Definition = method ?? throw new ArgumentNullException(nameof(method));
            StartBlock = startBlock ?? throw new ArgumentNullException(nameof(startBlock));

            if (blocks == null) throw new ArgumentNullException(nameof(blocks));
            _blocks.AddRange(blocks);
        }

        public IReadOnlyCollection<Block> Blocks => _blocks.AsReadOnly();
        public MethodDefinition Definition { get; }
        public Block StartBlock { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var block in Blocks)
            {
                visitor.Visit(block);
            }
        }
    }
}