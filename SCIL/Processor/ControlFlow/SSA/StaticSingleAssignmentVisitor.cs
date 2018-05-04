using System;
using System.Collections.Generic;
using System.Linq;
using SCIL.Processor.ControlFlow.SSA.Analyzers;
using SCIL.Processor.ControlFlow.SSA.NameGenerators;
using SCIL.Processor.ControlFlow.SSA.Simplifiers;
using SCIL.Processor.Nodes;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.ControlFlow.SSA
{
    [RegistrerVisitor(RegistrerVisitorAttribute.RewriterOrder + 1)]
    public class StaticSingleAssignmentVisitor : BaseVisitor
    {
        public override void Visit(Module module)
        {
            // Update all stack assignements
            var stackAnalyzerVisitor = new StackAnalyzerVisitor();
            stackAnalyzerVisitor.Visit(module);

            // Visit the module
            base.Visit(module);
            
            // Rewrite phi nodes
            var phiRewriter = new PhiNodeRewriterVisitor();
            phiRewriter.Visit(module);

            // Update all stack assignements (it should be updated since we have updated the tree)
            stackAnalyzerVisitor.Visit(module);

            // Generate names for stack, variables and arguments
            var stackNameGeneratorVisitor = new StackNameGeneratorVisitor();
            stackNameGeneratorVisitor.Visit(module);

            var variableNameGeneratorVisitor = new VariableNameGeneratorVisitor();
            variableNameGeneratorVisitor.Visit(module);
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
           
            // Get all variables
            var variables = GetVariables(method);

            // Compute and insert phi nodes
            InsertPhis(method, variables);

            // TODO: Arguments
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
        
        private void InsertPhis(Method method, IDictionary<Node, int> variableUpdates)
        {
            GenericInsertPhis<PhiVariableNode, int>(method, variableUpdates,
                (node, variableIndex) => node.VariableIndex == variableIndex,
                (block, parents, variableIndex) => new PhiVariableNode(block, parents, variableIndex));
        }

        private void InsertPhis(Method method, IDictionary<Node, IReadOnlyCollection<int>> nodeStackPushes)
        {
            // Filter such that stack index is only pushed one time in a block. We only need the last push
            // TODO: Group by block
            List<KeyValuePair<Node, int>> pushesFilteredByBlock = new List<KeyValuePair<Node, int>>();

            foreach (var block in method.Blocks)
            {
                pushesFilteredByBlock.AddRange(nodeStackPushes
                    .Where(variable => block.Nodes.Any(node => node == variable.Key))
                    .SelectMany(e => e.Value.Select(q => new KeyValuePair<Node, int>(e.Key, q))).GroupBy(e => e.Value)
                    .Select(e => e.Last()));
            }

            GenericInsertPhis<PhiStackNode, int>(method, pushesFilteredByBlock,
                (node, stackIndex) => node.StackIndex == stackIndex,
                (block, parents, stackIndex) => new PhiStackNode(block, parents, stackIndex));
        }

        private void GenericInsertPhis<TNode, TKey>(Method method, IEnumerable<KeyValuePair<Node, TKey>> stateUpdates, Func<TNode, TKey, bool> compareNodeToKey, Func<Block, List<Node>, TKey, TNode> createTNode)
           where TNode : PhiNode
        {
            // Add list of all nodes which should be added
            IDictionary<Block, List<TNode>> addedNodes = new Dictionary<Block, List<TNode>>();

            // Initilize dictionary with lists
            foreach (var block in method.Blocks)
            {
                addedNodes.Add(block, new List<TNode>());
            }

            // Place nodes
            bool addedPhiNode;
            do
            {
                addedPhiNode = false;

                foreach (var block in method.Blocks)
                {
                    // Check if we have any variable assignment in this block
                    IEnumerable<KeyValuePair<Node, TKey>> stateUpdatesInBlock = stateUpdates.Where(variable => block.Nodes.Any(node => node == variable.Key));

                    // Handle each stack push
                    foreach (var stateUpdate in stateUpdatesInBlock)
                    {
                        // Put a phi node on each dominance frontier
                        foreach (var dominanceFrontier in block.DomninanceFrontiers)
                        {
                            // Get block list
                            var blockList = addedNodes[dominanceFrontier];

                            // Double check that the phi node does not exists currently
                            var currentPhiNode = blockList.FirstOrDefault(phiNode =>
                                compareNodeToKey(phiNode, stateUpdate.Value));
                            if (currentPhiNode != null)
                            {
                                // Detect if the phiNode contains this block as parent
                                if (!currentPhiNode.Parents.Contains(stateUpdate.Key))
                                {
                                    currentPhiNode.Parents.Add(stateUpdate.Key);
                                    addedPhiNode = true;
                                }
                            }
                            else
                            {
                                currentPhiNode = createTNode(dominanceFrontier, new List<Node>() { stateUpdate.Key },
                                    stateUpdate.Value);
                                blockList.Add(currentPhiNode);
                                addedPhiNode = true;
                            }
                        }
                    }
                }
            } while (addedPhiNode);

            // Insert the nodes into the blocks
            foreach (KeyValuePair<Block, List<TNode>> node in addedNodes)
            {
                if (node.Value.Any())
                {
                    node.Key.InsertNodesAtIndex(0, node.Value.Where(e => e.Parents.Count > 1).ToArray());
                }
            }
        }
    }
}
