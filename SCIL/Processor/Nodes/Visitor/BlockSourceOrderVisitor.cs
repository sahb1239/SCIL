using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SCIL.Processor.Nodes.Visitor
{
    public class BlockSourceOrderVisitor : BaseVisitor
    {
        private readonly Queue<Block> _pendingBlocks = new Queue<Block>();
        private readonly List<Block> _visitedBlocks = new List<Block>();

        public override void Visit(Method method)
        {
            Debug.Assert(!_pendingBlocks.Any());

            base.Visit(method);

            int lastCount;
            do
            {
                lastCount = _pendingBlocks.Count;

                // Dequeue all item into list
                var queueCopy = new List<Block>();
                while (_pendingBlocks.Any())
                    queueCopy.Add(_pendingBlocks.Dequeue());

                // Visit all blocks again
                foreach (var block in queueCopy)
                {
                    Visit(block);
                }
            } while (lastCount != _pendingBlocks.Count);

            // Assert that all blocks is handled
            Debug.Assert(!_pendingBlocks.Any());
        }

        public sealed override void Visit(Block block)
        {
            // Detect if any source is in the visited blocks
            // The first any check is to detect the first block
            if (block.Method.StartBlock == block || !block.Sources.Any() || block.Sources.Any(source => _visitedBlocks.Contains(source)))
            {
                // Visit the block
                VisitBlock(block);

                // Add to visited blocks
                _visitedBlocks.Add(block);

                return;
            }

            // Enqueue block again to be processed at a later time
            _pendingBlocks.Enqueue(block);
        }

        public virtual void VisitBlock(Block block)
        {
            base.Visit(block);
        }
    }
}