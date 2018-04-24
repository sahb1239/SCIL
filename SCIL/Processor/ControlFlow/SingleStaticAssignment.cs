using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using SCIL.Processor.Nodes;

namespace SCIL.Processor.ControlFlow
{
    class SingleStaticAssignment
    {
        public static void ComputeSSA(Method method)
        {
            PlacePhis(method);
        }

        public static void PlacePhis(Method method)
        {
            Dominance.SimpleDominators(method);
            foreach(Block block in method.Blocks)
            {
                
            }
        }
    }

}
