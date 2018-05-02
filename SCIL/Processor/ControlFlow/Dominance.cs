using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SCIL.Processor.Nodes;

namespace SCIL.Processor.ControlFlow
{
    public class Dominance
    {
        // Simply returns the block's "dominates" field
        public static void SimpleDominators(Method method)
        {
            // Validate that startNode is first in blocks
            if (method.StartBlock != method.Blocks.FirstOrDefault())
            {
                throw new NotSupportedException("StartBlock needs to be first element in method.Blocks");
            }

            // Set domninator for the start node to it self
            method.StartBlock.DominatedBy.Clear();
            method.StartBlock.DominatedBy.Add(method.StartBlock);

            // Each block has its dominates initialized as every block in the graph except first
            foreach (Block block in method.Blocks.Skip(1))
            {
                block.DominatedBy.AddRange(method.Blocks);
            }

            // Eliminate nodes which is not domninators
            bool changesToDom;
            do
            {
                changesToDom = false;

                // Iterate over each block exept first block
                foreach (var block in method.Blocks.Skip(1))
                {
                    // The intersection of the block's sources "dominates" is added to the new list
                    List<Block> newDom = block.Union(
                        block.Sources?.Aggregate(block.Sources.SelectMany(e => e.DominatedBy),
                            (current, next) => current.Intersect(next.DominatedBy))).ToList();

                    // If the newly calculated newdom is different than the block's existing "dominates", it is updated
                    if (!newDom.SequenceEqual(block.DominatedBy))
                    {
                        block.DominatedBy.Clear();
                        block.DominatedBy.AddRange(newDom);

                        // Update that we should loop throught the tree one more time
                        changesToDom = true;
                    }
                }
            } while (changesToDom);
        }

        public static void SimpleDominanceFrontiers(Method method)
        {
            foreach (var block in method.Blocks)
            {
                block.DomninanceFrontiers.AddRange(SimpleDominanceFrontier(block, method));
            }
        }

        private static List<Block> SimpleDominanceFrontier(Block block, Method method)
        {
            List<Block> dominanceFrontier = new List<Block>();
            
            // All blocks in the graph that are dominated by this block are considered
            List<Block> dominatedBy = new List<Block>();
            foreach(Block other in method.Blocks)
            {
                if (other.DominatedBy.Contains(block))
                {
                    dominatedBy.Add(other);
                }
            }

            // Each dominated block's targets are checked 
            foreach(Block dominated in dominatedBy)
            {
                // If the block does not strictly dominate one of the targets, the target is added to the frontier
                foreach(Block target in dominated.Targets)
                {
                    if(!dominatedBy.Except(new List<Block> { block }).Contains(target)){
                        dominanceFrontier.Add(target);
                    }
                }
            }

            // Get only unique items
            var uniqueDominanceFrontier = dominanceFrontier.Distinct().ToList();

            return uniqueDominanceFrontier;
        }
        //TODO: Implement the fast dominators and dominance frontiers algorithms
        //Crafing a Compiler (2009) p. 580 & 584 (609 & 613 in PDF)
        
    }
}