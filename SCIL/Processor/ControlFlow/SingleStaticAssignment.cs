using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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

        public static void PlacePhis(List<Tuple<Node, byte>> variables)
        {
            List<Node> IsProcessed = new List<Node>();
            List<Block> HasPhi = new List<Block>();
            List<Node> Definitions = new List<Node>();
            List<Node> worklist = new List<Node>();

            //Find all nodes where a value is popped, i.e. a variable is defined or redefined
            foreach(Tuple<Node, byte> var in variables)
            {
                if(var.Item1.PopStackNames.Count > 0)
                {
                    Definitions.Add(var.Item1);
                }
            }
            
            foreach (Node definition in Definitions)
            {
                if (!IsProcessed.Contains(definition))
                {
                    worklist.Add(definition);
                    IsProcessed.Add(definition);
                }
            }

            while (worklist.Any())
            {
                Node first = worklist.First();
                worklist.RemoveAt(0);
                foreach(Block df in DF)
                {
                    if (!HasPhi.Contains(df))
                    {
                        HasPhi.Add(df);
                        //TODO: Add phi function node
                        foreach(Node node in df.Nodes)
                        {
                            if (!IsProcessed.Contains(node))
                            {
                                worklist.Add(node);
                                IsProcessed.Add(node);
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
