using System;
using System.Collections.Generic;
using System.Linq;
using SCIL.Processor.Nodes;
using Mono.Cecil.Cil;

namespace SCIL.Processor.ControlFlow
{
    class SingleStaticAssignment
    {
        private static List<Block> DF = new List<Block>();
        public static void ComputeSSA(Method method)
        {
            //Find dominators in method
            Dominance.SimpleDominators(method);
            List<Tuple<Node, byte>> Variables = new List<Tuple<Node, byte>>();

            //Find all variables defined in method
            for(int i=0; i < method.Blocks.Count; i++)
            {
                foreach(Node node in method.Blocks.ElementAt(i).Nodes)
                {
                    if(node.OpCode.StackBehaviourPop > 0)
                    {
                        Variables.Add(Tuple.Create(node, GetVariable(node)));
                    }
                }
            }

            foreach (Block block in method.Blocks)
            {
                DF = Dominance.SimpleDominanceFrontier(block, method);
                PlacePhis(Variables);
            }
            Rename(method);
        }
        //Place phi nodes
        public static void PlacePhis(List<Tuple<Node, byte>> variables)
        {
            List<Node> IsProcessed = new List<Node>();
            List<Block> HasPhi = new List<Block>();
            List<Tuple<Node, byte>> Definitions = new List<Tuple<Node, byte>>();
            List<Tuple<Node, byte>> worklist = new List<Tuple<Node, byte>>();

            //Find all nodes where a value is popped, i.e. a variable is defined or redefined
            foreach(var var in variables)
            {
                if(var.Item1.PopStackNames.Count > 0)
                {
                    Definitions.Add(var);
                }
            }
            //Create initial worklist and isprocessed from definition nodes
            foreach (var definition in Definitions)
            {
                if (!IsProcessed.Contains(definition.Item1))
                {
                    worklist.Add(definition);
                    IsProcessed.Add(definition.Item1);
                }
            }

            //Run through worklist, removing one element and adding elements if new are found in the node's DF
            while (worklist.Any())
            {
                Tuple<Node, byte> first = worklist.First();
                worklist.RemoveAt(0);
                foreach(Block df in DF)
                {
                    //Only one phi node is needed for a single variable in a single block
                    if (!HasPhi.Contains(df))
                    {
                        //Create new PhiNode for block df with variable from the worklist and parents from the variables list
                        List<Node> parents = new List<Node>();
                        foreach(var var in variables)
                        {
                            if(var.Item2 == first.Item2)
                            {
                                parents.Add(var.Item1);
                            }
                        }
                        PhiNode phi = new PhiNode(df, first.Item1, parents.ToArray());
                        HasPhi.Add(df);
                        //DF is examined to add new nodes to the worklist if they are not already processed
                        List<Tuple<Node, byte>> dfNodelist = new List<Tuple<Node, byte>>();
                        foreach(Node dfnode in df.Nodes)
                        {
                            dfNodelist.Add(Tuple.Create(dfnode, GetVariable(dfnode)));
                        }

                        foreach(var node in dfNodelist)
                        {
                            if (!IsProcessed.Contains(node.Item1))
                            {
                                worklist.Add(node);
                                IsProcessed.Add(node.Item1);
                            }
                        }
                    }
                }
            }
        }

        private static byte GetVariable(Node node)
        {
            switch (node.OpCode.Code)
            {
                case Code.Starg:
                case Code.Starg_S:
                case Code.Stfld:
                case Code.Stloc:
                case Code.Stloc_0:
                case Code.Stloc_1:
                case Code.Stloc_2:
                case Code.Stloc_3:
                case Code.Stloc_S:
                case Code.Stsfld:
                    return node.OpCode.Op1;

            }
            throw new InvalidOperationException();
        }

        private static void Rename(Method method)
        {
            throw new NotImplementedException();
        }
    }

}
