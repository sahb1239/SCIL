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

        private class CILStack
        {
            private readonly Method _method;
            private readonly Stack<string> _stack = new Stack<string>();
            private readonly SharedStackNames _stackNames;

            public CILStack(Method method)
            {
                _method = method;
                _stackNames = new SharedStackNames();
            }

            private CILStack(Method method, SharedStackNames names)
            {
                _method = method;
                _stackNames = names;
            }

            public string PopStack()
            {
                var popped = _stack.Pop();
                return popped;
            }

            public string PushStack()
            {
                var index = _stack.Count;
                var methodName = _method.Definition.FullName;

                string indexName = $"\"{methodName}_{_stackNames.GetNewName(index)}\"";
                _stack.Push(indexName);
                return indexName;
            }

            public CILStack Copy()
            {
                var newStack = new CILStack(_method, _stackNames);

                // Copy stack
                foreach (var currentStackItem in _stack)
                {
                    newStack._stack.Push(currentStackItem);
                }

                return newStack;
            }

            private class SharedStackNames
            {
                private List<List<string>> _names = new List<List<string>>();

                public string GetNewName(int index)
                {
                    // Add extra index list if it does not exists
                    if (_names.Count <= index)
                    {
                        _names.Add(new List<string>());
                    }

                    // Get index list
                    var indexList = _names[index];
                    var indexName = $"{index}_{indexList.Count}";
                    indexList.Add(indexName);

                    return indexName;
                }
            }
        }

        private class MethodVisitor : BaseVisitor
        {
            private readonly Method _method;
            private readonly IDictionary<Block, CILStack> _stacks = new Dictionary<Block, CILStack>();

            public MethodVisitor(Method method)
            {
                _method = method;
            }

            public override void Visit(Block block)
            {
                CILStack currentStack;

                // Get stack
                if (block.Sources.Any())
                {
                    var stack = _stacks[block.Sources.First()];
                    currentStack = stack.Copy();
                }
                else
                {
                    currentStack = new CILStack(_method);
                }

                // Add to stack list
                _stacks[block] = currentStack;

                // Run visitor
                var visitor = new BlockVisitor(currentStack);
                visitor.Visit(block);
            }
        }

        private class BlockVisitor : BaseVisitor
        {
            private readonly CILStack _stack;

            public BlockVisitor(CILStack stack)
            {
                _stack = stack ?? throw new ArgumentNullException(nameof(stack));
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
                for (int i = 0; i < node.GetRequiredNames().popNames; i++)
                {
                    yield return _stack.PopStack();
                }
            }

            private IEnumerable<string> GetPushStackNames(Node node)
            {
                for (int i = 0; i < node.GetRequiredNames().pushNames; i++)
                {
                    yield return _stack.PushStack();
                }
            }
        }
    }
}
