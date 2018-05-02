using System;
using System.Collections.Generic;
using System.Linq;
using SCIL.Processor.Nodes;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.ControlFlow
{
    //[RegistrerVisitor(RegistrerVisitorAttribute.RewriterOrder + 2)]
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
            private readonly IDictionary<Block, CILStack> _stacks = new Dictionary<Block, CILStack>();
            private readonly IDictionary<Block, Variables> _variables = new Dictionary<Block, Variables>();

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

                // Get variables
                Variables currentVariables;
                if (block.Sources.Any())
                {
                    var variables = _variables[block.Sources.First()];
                    currentVariables = variables.Copy();
                }
                else
                {
                    currentVariables = new Variables(_method);
                }

                // Add to stack list
                _variables[block] = currentVariables;

                // Run visitor
                var visitor = new BlockVisitor(currentStack, currentVariables);
                visitor.Visit(block);
            }
        }

        private class BlockVisitor : BaseVisitor
        {
            private readonly CILStack _stack;
            private readonly Variables _variables;

            public BlockVisitor(CILStack stack, Variables variables)
            {
                _stack = stack ?? throw new ArgumentNullException(nameof(stack));
                _variables = variables ?? throw new ArgumentNullException(nameof(variables));
            }

            public override void Visit(Node node)
            {
                // Detect exception handling
                if (node.Block.Method.Definition.Body.ExceptionHandlers.Any(e => e.HandlerStart == node.Instruction))
                {
                    var pushNames = new List<string>();
                    pushNames.Add(_stack.PushStack());

                    // Get all pop names
                    node.SetPopStackNames(GetPopStackNames(node).ToArray());

                    // Get the rest of the push names
                    for (int i = 1; i < node.GetRequiredNames().pushNames; i++)
                    {
                        pushNames.Add(_stack.PushStack());
                    }
                    node.SetPushStackNames(pushNames.ToArray());
                }
                else
                {
                    node.SetPopStackNames(GetPopStackNames(node).ToArray());
                    node.SetPushStackNames(GetPushStackNames(node).ToArray());
                }

                // Set argument names
                node.ArgumentName = $"\"{node.GetRequiredArgumentIndex()}\"";

                // Set variable names
                var variableIndex = node.GetRequiredVariableIndex();
                if (variableIndex.variableInstruction)
                {
                    // Get variable name
                    string variableName;

                    if (variableIndex.set)
                    {
                        variableName = _variables.SetIndex(variableIndex.index);
                    }
                    else
                    {
                        variableName = _variables.GetIndex(variableIndex.index);
                    }

                    node.VariableName = $"{variableName}";
                }

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

    public class CILStack
    {
        private readonly Method _method;
        private Stack<string> _stack = new Stack<string>();
        private readonly SharedNames _stackNames;

        public CILStack(Method method)
        {
            _method = method;
            _stackNames = new SharedNames();
        }

        private CILStack(Method method, SharedNames stackNames)
        {
            _method = method;
            _stackNames = stackNames;
        }

        public string PopStack()
        {
            var popped = _stack.Pop();
            return popped;
        }

        public string PushStack()
        {
            var index = _stack.Count;
            var methodName = _method.Definition.NameOnly();

            string indexName = $"\"{methodName}_{_stackNames.GetNewName(index)}\"";
            _stack.Push(indexName);
            return indexName;
        }

        public CILStack Copy()
        {
            // Copy stack
            // TODO: Check if this is returning what is expected
            var newStack = new CILStack(_method, _stackNames) { _stack = new Stack<string>(_stack) };

            return newStack;
        }
    }

    public class Variables
    {
        private readonly Method _method;
        private readonly SharedNames _variableNames;

        private List<string> _currentNames = new List<string>();

        public Variables(Method method)
        {
            _method = method;
            _variableNames = new SharedNames();
        }

        private Variables(Method method, SharedNames names)
        {
            _method = method;
            _variableNames = names;
        }

        public string GetIndex(int index)
        {
            // Add null to names
            while (_currentNames.Count <= index)
                _currentNames.Add(null);

            // If current name is not set (for example ldarga we need to set it)
            var currentName = _currentNames[index];
            if (currentName == null)
                return SetIndex(index);

            return _currentNames[index];
        }

        public string SetIndex(int index)
        {
            // Add null to names
            while (_currentNames.Count <= index)
                _currentNames.Add(null);

            var methodName = _method.Definition.NameOnly();

            // Add new name
            return _currentNames[index] = $"\"{methodName}_{ _variableNames.GetNewName(index)}\"";
        }

        public Variables Copy()
        {
            return new Variables(_method, _variableNames)
            {
                _currentNames = new List<string>(_currentNames)
            };
        }
    }

    public class SharedNames
    {
        private readonly List<List<string>> _names = new List<List<string>>();

        public string GetNewName(int index)
        {
            // Add extra index list if it does not exists
            while (_names.Count <= index)
            {
                _names.Add(new List<string>());
            }

            // Get index list
            var indexList = _names[index];
            var indexName = $"{index}_{indexList.Count}";
            indexList.Add(indexName);

            return indexName;
        }

        public string GetCurrentName(int index)
        {
            // Add extra index list if it does not exists
            while (_names.Count <= index)
            {
                _names.Add(new List<string>());
            }

            // Get index list
            var indexList = _names[index];
            return indexList.Last();
        }
    }
}
