using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SCIL.Processor.Nodes;

namespace SCIL.Processor.ControlFlow
{
    public class Dominance
    {
        //Simply returns the block's "dominates" field
        private static List<Block> Dominates(Block block)
        {
            return block.Dominates;
        }

        public static void SimpleDominators(Method method)
        {
            //Each block has its dominates initialized as every block in the graph
            foreach(Block block in method.Blocks)
            {
                Dominates(block).AddRange(method.Blocks);
            }

            //The worklist contains all blocks under consideration, initialized with the start block
            List<Block> worklist = new List<Block> { method.StartBlock };
            while (worklist.Any())
            {
                //The first element in the worklist is picked and removed
                Block block = worklist.First();
                worklist.RemoveAt(0);

                //New "dominates" list is computed, starts with the block itself
                List<Block> newdom = new List<Block> { block };

                //The intersection of the block's sources "dominates" is added to the new list
                List<Block> temp = new List<Block>();
                if (block.Sources.Any())
                {
                    temp.AddRange(Dominates(block.Sources.First()));
                    for (int i = 1; i < block.Sources.Count; i++)
                    {
                        temp.AddRange(temp.Intersect(Dominates(block.Sources.ElementAt(i))));
                    }
                }
                newdom.AddRange(temp);

                //If the newly calculated newdom is different than the block's existing "dominates", it is updated
                if (!newdom.Equals(Dominates(block)))
                {
                    Dominates(block).Clear();
                    Dominates(block).AddRange(newdom);
                    //The block's targets are all added to the worklist, unless they are in it already
                    foreach(Block target in block.Targets)
                    {
                        if (!worklist.Contains(target))
                        {
                            worklist.Add(target);
                        }
                        
                    }
                }

            }

        }

        public static List<Block> SimpleDominanceFrontier(Block block, Method method)
        {
            List<Block> DominanceFrontier = new List<Block>();
            
            //All blocks in the graph that are dominated by this block are considered
            List<Block> DominatedBy = new List<Block>();
            foreach(Block other in method.Blocks)
            {
                if (Dominates(other).Contains(block))
                {
                    DominatedBy.Add(other);
                }
            }

            //Each dominated block's targets are checked 
            foreach(Block dominated in DominatedBy)
            {
                //If the block does not strictly dominate one of the targets, the target is added to the frontier
                foreach(Block target in dominated.Targets)
                {
                    if(!DominatedBy.Except(new List<Block> { block }).Contains(target)){
                        DominanceFrontier.Add(target);
                    }
                }
            }

            return DominanceFrontier;
        }
        //TODO: Implement the fast dominators and dominance frontiers algorithms
        //Crafing a Compiler (2009) p. 580 & 584 (609 & 613 in PDF)
        
    }
}