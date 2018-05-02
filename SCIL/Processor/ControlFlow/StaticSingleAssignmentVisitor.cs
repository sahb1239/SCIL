using System;
using System.Collections.Generic;
using System.Linq;
using SCIL.Processor.Nodes;
using Mono.Cecil.Cil;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.ControlFlow
{
    [RegistrerVisitor(RegistrerVisitorAttribute.RewriterOrder + 1)]
    public class StaticSingleAssignmentVisitor : BaseVisitor
    {
        public override void Visit(Module module)
        {
            base.Visit(module);

            // Update all stack assignements
            var stackAnalyzerVisitor = new StackAnalyzerVisitor();
            stackAnalyzerVisitor.Visit(module);
        }

        public override void Visit(Method method)
        {
            // Find dominators in the method (adds to the Domninators list)
            Dominance.SimpleDominators(method);

            // Find dominance frontiers
            Dominance.SimpleDominanceFrontiers(method);

            // Find all pushes to the stack
            var stackPushes = GetStackPushes(method);

            // Compute and insert phi nodes
            InsertPhis(method, stackPushes);
           
            // TODO: Variables, Arguments
        }

        

        private IDictionary<Node, IReadOnlyCollection<int>> GetStackPushes(Method method)
        {
            // Find all pushes to the stack
            return
                method.Blocks.SelectMany(e => e.Nodes).Where(e => e.PushStack.Any())
                    .ToDictionary(node => node, node => node.PushStack);
        }

        // TODO: Fix ldloc.a
        private IDictionary<Node, int> GetVariables(Method method)
        {
            // Find all variables defined in method
            Dictionary<Node, int> variables = new Dictionary<Node, int>();
            for (var i = 0; i < method.Blocks.Count; i++)
            {
                foreach (Node node in method.Blocks.ElementAt(i).Nodes)
                {
                    var variableInfo = node.GetRequiredVariableIndex();

                    if (variableInfo.variableInstruction && variableInfo.set)
                    {
                        variables.Add(node, variableInfo.index);
                    }
                }
            }

            return variables;
        }

        private void InsertPhis(Method method, IDictionary<Node, IReadOnlyCollection<int>> nodeStackPushes)
        {
            // Add list of all nodes which should be added
            IDictionary<Block, List<PhiNode>> addedNodes = new Dictionary<Block, List<PhiNode>>();

            // Initilize dictionary with lists
            foreach (var block in method.Blocks)
            {
                addedNodes.Add(block, new List<PhiNode>());
            }

            // Place nodes
            bool addedPhiNode;
            do
            {
                addedPhiNode = false;

                foreach (var block in method.Blocks)
                {
                    // Check if we have any variable assignment in this block
                    var stackPushesInBlock =
                        nodeStackPushes.Where(variable => block.Nodes.Any(node => node == variable.Key)).ToArray();

                    // TODO: Stack indexes could be assigned multiple times in one block, we really only need to take the last assignment since it could be problematic with other values
                    // TODO: Filter here ^^
                    var lastStackPushesInBlock =
                        stackPushesInBlock.SelectMany(e => e.Value.Select(q => new {stackIndex = q, node = e.Key}))
                            .GroupBy(e => e.stackIndex).Select(e => e.Last());

                    if (lastStackPushesInBlock.Any())
                    {
                        // Handle each stack push
                        foreach (var stackPush in lastStackPushesInBlock)
                        {
                                // Put a phi node on each dominance frontier
                                foreach (var dominanceFrontier in block.DomninanceFrontiers)
                                {
                                    // Get block list
                                    var blockList = addedNodes[dominanceFrontier];

                                    // Double check that the phi node does not exists currently
                                    var currentPhiNode = blockList.FirstOrDefault(phiNode =>
                                        phiNode.StackIndex == stackPush.stackIndex);
                                    if (currentPhiNode != null)
                                    {
                                        // Detect if the phiNode contains this block as parent
                                        if (!currentPhiNode.Parents.Contains(stackPush.node))
                                        {
                                            currentPhiNode.Parents.Add(stackPush.node);
                                            addedPhiNode = true;
                                        }
                                    }
                                    else
                                    {
                                        currentPhiNode = new PhiNode(dominanceFrontier,
                                            new List<Node>() { stackPush.node }, stackPush.stackIndex);
                                        blockList.Add(currentPhiNode);
                                        addedPhiNode = true;
                                    }
                                }
                        }
                    }
                }
            } while (addedPhiNode);

            // Insert the nodes into the blocks
            foreach (var addedPhi in addedNodes)
            {
                if (addedPhi.Value.Any())
                {
                    addedPhi.Key.InsertNodesAtIndex(0, addedPhi.Value.ToArray());
                }
            }
        }
        /*
        private void InsertPhis(Method method, IDictionary<Node, int> variables)
        {
            // Add list of all nodes which should be added
            IDictionary<Block, List<PhiNode>> addedNodes = new Dictionary<Block, List<PhiNode>>();

            // Initilize dictionary with lists
            foreach (var block in method.Blocks)
            {
                addedNodes.Add(block, new List<PhiNode>());
            }

            // Place nodes
            bool addedPhiNode;
            do
            {
                addedPhiNode = false;

                foreach (var block in method.Blocks)
                {
                    // Check if we have any variable assignment in this block
                    var variableAssignementsInBlock =
                        variables.Where(variable => block.Nodes.Any(node => node == variable.Key)).ToArray();
                    if (variableAssignementsInBlock.Any())
                    {
                        // Handle each variable
                        foreach (var variableAssignment in variableAssignementsInBlock)
                        {
                            // Put a phi node on each dominance frontier
                            foreach (var dominanceFrontier in block.DomninanceFrontiers)
                            {
                                // Get block list
                                var blockList = addedNodes[dominanceFrontier];

                                // Double check that the phi node does not exists currently
                                var currentPhiNode = blockList.FirstOrDefault(phiNode =>
                                    phiNode.VariableIndex == variableAssignment.Value);
                                if (currentPhiNode != null)
                                {
                                    // Detect if the phiNode contains this block as parent
                                    if (!currentPhiNode.Parents.Contains(variableAssignment.Key))
                                    {
                                        currentPhiNode.Parents.Add(variableAssignment.Key);
                                        addedPhiNode = true;
                                    }
                                }
                                else
                                {
                                    currentPhiNode = new PhiNode(dominanceFrontier, new List<Node>() {variableAssignment.Key}, variableAssignment.Value);
                                    blockList.Add(currentPhiNode);
                                    addedPhiNode = true;
                                }
                            }
                        }
                    }
                }
            } while (addedPhiNode);

            // Insert the nodes into the blocks
            foreach (var addedPhi in addedNodes)
            {
                if (addedPhi.Value.Any())
                {
                    addedPhi.Key.InsertNodesAtIndex(0, addedPhi.Value.ToArray());
                }
            }
        }*/

        
    }
}
