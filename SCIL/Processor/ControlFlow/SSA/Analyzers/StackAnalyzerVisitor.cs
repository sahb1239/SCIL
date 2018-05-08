using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil.Cil;
using SCIL.Processor.Nodes;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.ControlFlow.SSA.Analyzers
{
    public class StackAnalyzerVisitor : BaseVisitor
    {
        public override void Visit(Method method)
        {
            var visitor = new MethodVisitor();
            visitor.Visit(method);
        }

        private class MethodVisitor : BlockSourceOrderVisitor
        {
            private readonly IDictionary<Block, int> _stacks = new Dictionary<Block, int>();
            private int _nextStack = 0;

            public override void Visit(Method method)
            {
                Debug.Assert(_nextStack == 0);

                base.Visit(method);
                
                // Assert that the stack is handled on all last nodes (next stack should be 0)
                var blocksWithEnds = method.Blocks
                    .Where(block => !block.Targets.Any() || block.Nodes.Any(node => node.OpCode.Code == Code.Ret));
                var stackAtEnds = blocksWithEnds.Select(block => _stacks[block]);

                Debug.Assert(stackAtEnds
                    .All(nextStack => nextStack == 0));
            }

            public override void VisitBlock(Block block)
            {
                // Get stack
                if (block.Method.StartBlock == block || !block.Sources.Any())
                {
                    _nextStack = 0;
                }
                else
                {
                    var key = block.Sources.First(source => _stacks.ContainsKey(source));
                    var stack = _stacks[key];
                    _nextStack = stack;
                }
                
                // Run visitor
                var visitor = new BlockVisitor(_nextStack);
                visitor.Visit(block);

                // Add to stack list
                _stacks[block] = visitor.GetNextStack();
            }

            private class BlockVisitor : BaseVisitor
            {
                private int _nextStack;

                public BlockVisitor(int nextStack)
                {
                    this._nextStack = nextStack;
                }

                public override void Visit(Node node)
                {
                    // Detect phi nodes
                    if (node is PhiStackNode phiNode)
                    {
                        var parentPushNames = phiNode.Parents.Select(e => e.PushStack.Last()).Distinct().ToList();

                        // Parent pushnames should all have the same index
                        // Debug.Assert(parentPushNames.Count == 1);
                        // Not always the case for example if parent is dup

                        // Debug that all parents is the same
                        Debug.Assert(phiNode.Parents.Skip(1).All(parent =>
                            parent.PushStack.Last() == phiNode.Parents.First().PushStack.Last()));

                        // Set the push stack
                        node.SetPushStack(parentPushNames.First());

                        base.Visit(node);

                        Debug.Assert(node.PushStack.Count == node.PushCountFromStack);
                        Debug.Assert(node.PopStack.Count == node.PopCountFromStack);

                        return;
                    }

                    // Detect exception handling
                    var exceptionHandler =
                        node.Block.Method.Definition.Body.ExceptionHandlers.SingleOrDefault(e =>
                            e.HandlerStart == node.Instruction);
                    if (exceptionHandler != null)
                    {
                        // Background info
                        // https://stackoverflow.com/questions/11987953/how-are-cil-fault-clauses-different-from-catch-clauses-in-c
                        switch (exceptionHandler.HandlerType)
                        {
                            case ExceptionHandlerType.Catch:
                                // Popall and push exception
                                _nextStack = 1;
                                break;
                            case ExceptionHandlerType.Finally:
                                // Popall
                                _nextStack = 0;
                                break;
                            case ExceptionHandlerType.Filter:
                                // Popall and let's asume we get a exception (seems like it's the case)
                                _nextStack = 1;
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }

                    // Detect leave
                    switch (node.OpCode.Code)
                    {
                        case Code.Leave:
                        case Code.Leave_S:
                            // The leave.s instruction empties the evaluation stack and ensures that the appropriate surrounding finally blocks are executed.
                            // https://msdn.microsoft.com/en-us/library/system.reflection.emit.opcodes.leave_s(v=vs.110).aspx
                            _nextStack = 0;
                            break;
                    }

                    node.SetPopStack(GetPopStackNames(node).ToArray());
                    node.SetPushStack(GetPushStackNames(node).ToArray());

                    Debug.Assert(node.PushStack.Count == node.PushCountFromStack);
                    Debug.Assert(node.PopStack.Count == node.PopCountFromStack);

                    base.Visit(node);
                }

                private IEnumerable<int> GetPopStackNames(Node node)
                {
                    for (int i = 0; i < node.PopCountFromStack; i++)
                    {
                        yield return --_nextStack;
                    }
                    
                    Debug.Assert(_nextStack >= 0);
                }

                private IEnumerable<int> GetPushStackNames(Node node)
                {
                    Debug.Assert(_nextStack >= 0);

                    for (int i = 0; i < node.PushCountFromStack; i++)
                    {
                        yield return _nextStack++;
                    }
                }

                public int GetNextStack()
                {
                    return _nextStack;
                }
            }

        }
    }
}
