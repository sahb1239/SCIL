using System.Collections.Generic;
using System.Linq;
using SCIL.Processor.ControlFlow.SSA.Helpers;
using SCIL.Processor.Nodes;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.ControlFlow.SSA.NameGenerators
{
    public class StackNameGeneratorVisitor : BaseVisitor
    {
        public override void Visit(Method method)
        {
            var visitor = new MethodVisitor(method);
            visitor.Visit(method);
        }

        private class MethodVisitor : BaseVisitor
        {
            private Method _method;
            private SharedNames _sharedNames = new SharedNames();

            public MethodVisitor(Method method)
            {
                this._method = method;
            }

            public override void Visit(Node node)
            {
                node.SetPopStackNames(GetPopStackNames(node).ToArray());
                node.SetPushStackNames(GetPushStackNames(node).ToArray());
                base.Visit(node);
            }

            private string GetMethodName()
            {
                return _method.Definition.NameOnly();
            }

            private IEnumerable<string> GetPopStackNames(Node node)
            {
                var popIndexes = node.PopStack;
                var methodName = GetMethodName();
                foreach (var popIndex in popIndexes)
                {
                    yield return $"\"{methodName}_{_sharedNames.GetCurrentName(popIndex)}\"";
                }
            }

            private IEnumerable<string> GetPushStackNames(Node node)
            {
                var pushIndexes = node.PushStack;
                var methodName = GetMethodName();
                foreach (var pushIndex in pushIndexes)
                {
                    yield return $"\"{methodName}_{_sharedNames.GetNewName(pushIndex)}\"";
                }
            }
        }
    }


}
