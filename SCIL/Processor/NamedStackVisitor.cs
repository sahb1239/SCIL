using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SCIL.Processor.Nodes;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor
{
    [RegistrerVisitor(RegistrerVisitorAttribute.RewriterOrder + 1)]
    public class NamedStackVisitor : BaseVisitor
    {
        public override void Visit(Method method)
        {
            var visitor = new MethodVisitor(method);
            visitor.Visit(method);
        }

        private class MethodVisitor : BaseVisitor
        {
            private readonly Method _method;
            private readonly Stack<List<string>> _stack = new Stack<List<string>>();
            private readonly Stack<List<string>> _poppedStack = new Stack<List<string>>();

            public MethodVisitor(Method method)
            {
                _method = method;
            }

            public override void Visit(Method method)
            {
                if (method != _method)
                    throw new NotSupportedException();

                base.Visit(method);
            }

            public override void Visit(Block block)
            {
                base.Visit(block);
            }

            public override void Visit(Node node)
            {
                // Set stack names
                node.SetPopStackNames(GetPopStackNames(node).ToArray());
                node.SetPushStackNames(GetPushStackNames(node).ToArray());
               
                // Set argument names
                
                // Set variable names

                base.Visit(node);
            }

            private IEnumerable<string> GetPopStackNames(Node node)
            {
                var code = node.OpCode;

                // Handle pop
                switch (code.StackBehaviourPop)
                {
                    case StackBehaviour.Pop0:
                        break;
                    case StackBehaviour.Pop1:
                    case StackBehaviour.Popi:
                        yield return PopStack();
                        break;
                    case StackBehaviour.Popref:
                        yield return PopStack();
                        break;
                    case StackBehaviour.Pop1_pop1:
                    case StackBehaviour.Popi_pop1:
                    case StackBehaviour.Popi_popi:
                    case StackBehaviour.Popi_popr4:
                    case StackBehaviour.Popi_popr8:
                        yield return PopStack();
                        yield return PopStack();
                        break;
                    case StackBehaviour.Popref_popi_popi:
                    case StackBehaviour.Popref_popi_popr4:
                    case StackBehaviour.Popref_popi_popr8:
                    case StackBehaviour.Popref_popi_popi8:
                    case StackBehaviour.Popref_popi_popref:
                        yield return PopStack();
                        yield return PopStack();
                        yield return PopStack();
                        break;
                    case StackBehaviour.Varpop:
                        //yield return PopStack();
                        Console.WriteLine("Not sure what this is");
                        break;
                    case StackBehaviour.PopAll:
                        while (_stack.Any())
                        {
                            PopStack();
                        }
                        break;
                    default:
                        throw new NotImplementedException($"StackBehaviour on pop {code.StackBehaviourPop} not implemented");
                }
            }
            private IEnumerable<string> GetPushStackNames(Node node)
            {
                var code = node.OpCode;

                // Handle pop
                switch (code.StackBehaviourPush)
                {
                    case StackBehaviour.Push0:
                        break;
                    case StackBehaviour.Push1:
                    case StackBehaviour.Pushi:
                    case StackBehaviour.Pushi8:
                    case StackBehaviour.Pushr4:
                    case StackBehaviour.Pushr8:
                    case StackBehaviour.Pushref:
                        yield return PushStack();
                        break;
                    case StackBehaviour.Push1_push1:
                        yield return PushStack();
                        yield return PushStack();
                        break;
                    case StackBehaviour.Varpush:
                        //yield return PushStack();
                        //Console.WriteLine("Not sure what this is");
                        break;
                    default:
                        throw new NotImplementedException($"StackBehaviour on push {code.StackBehaviourPush} not implemented");
                }
            }

            private string PopStack()
            {
                var popped = _stack.Pop();
                _poppedStack.Push(popped);
                return popped.Last();
            }

            private string PushStack()
            {
                var index = _stack.Count;
                var methodName = _method.Definition.FullName;

                if (_poppedStack.Any())
                {
                    var pop = _poppedStack.Pop();
                    _stack.Push(pop);

                    string indexName = $"\"{methodName}_{index}_{pop.Count}\"";
                    pop.Add(indexName);
                    return indexName;
                }
                else
                {
                    string indexName = $"\"{methodName}_{index}\"";
                    _stack.Push(new List<string> { indexName });
                    return indexName;
                }
            }
        }
    }
}
