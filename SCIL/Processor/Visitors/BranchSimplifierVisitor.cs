using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.Visitors
{
    [RegistrerVisitor(RegistrerVisitor.RewriterOrder)]
    public class BranchSimplifierVisitor : BaseVisitor
    {
        public override void Visit(Node node)
        {
            List<Node> newNodes = new List<Node>();
            switch (node.Code.Code)
            {
                case Code.Br:
                case Code.Br_S:
                    // Branch unconditional
                    // Load constant 1
                    newNodes.Add(new Node(node.Instruction, node.Block) {OverrideOpCode = OpCodes.Ldc_I4_1});
                    // Add branch true
                    newNodes.Add(new Node(node.Instruction, node.Block) {OverrideOpCode = OpCodes.Brtrue});
                    break;
                case Code.Brfalse:
                case Code.Brfalse_S:
                    // Branch if false
                    // Add negation
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Neg });
                    // Add branch true
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Brtrue });
                    break;
                case Code.Beq:
                case Code.Beq_S:
                    // Branch equal
                    // Add ceq (Compare equal - returns 1 if equal - 0 if not equal)
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Ceq });
                    // Add branch true
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Brtrue });
                    break;
                case Code.Bne_Un:
                case Code.Bne_Un_S:
                    // Branch not equal // TODO: Or unordered float values
                    // Add ceq (Compare equal - returns 1 if equal - 0 if not equal)
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Ceq });
                    // Add negation
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Neg });
                    // Add branch true
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Brtrue });
                    break;
                case Code.Ble:
                case Code.Ble_S:
                case Code.Ble_Un:
                case Code.Ble_Un_S:
                    // Branch if less than or equal
                    // Add cgt (Compare greather - returns 1 if greather - 0 otherwise)
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Cgt });
                    // Add negation
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Neg });
                    // Add branch true
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Brtrue });
                    break;
                case Code.Bge:
                case Code.Bge_S:
                case Code.Bge_Un:
                case Code.Bge_Un_S:
                    // Branch if greather than or equal
                    // Add clt (Compare less - returns 1 if less - 0 otherwise)
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Clt });
                    // Add negation
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Neg });
                    // Add branch true
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Brtrue });
                    break;
                case Code.Blt:
                case Code.Blt_S:
                case Code.Blt_Un:
                case Code.Blt_Un_S:
                    // Branch if less than
                    // Add clt (Compare less - returns 1 if less - 0 otherwise)
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Clt });
                    // Add branch true
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Brtrue });
                    break;
                case Code.Bgt:
                case Code.Bgt_S:
                case Code.Bgt_Un:
                case Code.Bgt_Un_S:
                    // Branch if greather than
                    // Add cgt (Compare greather - returns 1 if greather - 0 otherwise)
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Cgt });
                    // Add branch true
                    newNodes.Add(new Node(node.Instruction, node.Block) { OverrideOpCode = OpCodes.Brtrue });
                    break;
            }

            // Replace nodes if it was set
            if (newNodes.Any())
                 node.Replace(newNodes.ToArray());

            base.Visit(node);
        }
    }
}
