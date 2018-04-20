using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SCIL.Processor.Nodes;

namespace SCIL.Processor.ControlFlow
{
    public class DominanceTree
    {
        private static List<Block> Dominates(Block block)
        {
            return block.Dominates;
        }

        public static void SimpleDominators(List<Block> graph)
        {
            foreach(Block block in graph)
            {
                Dominates(block).AddRange(graph);
            }

            List<Block> worklist = new List<Block> { graph.First() };
            while (worklist.Any())
            {
                Block block = worklist.First();
                worklist.RemoveAt(0);

                List<Block> newdom = new List<Block> { block };

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

                if (!newdom.Equals(Dominates(block)))
                {
                    Dominates(block).Clear();
                    Dominates(block).AddRange(newdom);
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

        public static List<Block> SimpleDominanceFrontiers(List<Block> graph)
        {
            List<Block> DominanceFrontier = new List<Block>();

            foreach(Block block in graph)
            {
                List<Block> DominatedBy = new List<Block>();
                foreach(Block other in graph)
                {
                    if (Dominates(other).Contains(block))
                    {
                        DominatedBy.Add(other);
                    }
                }

                foreach(Block dominator in DominatedBy)
                {
                    foreach(Block target in dominator.Targets)
                    {
                        if(!DominatedBy.Except(new List<Block> { block }).Contains(target)){
                            DominanceFrontier.Add(target);
                        }
                    }
                }
            }

            return DominanceFrontier;
        }
        
    }
}