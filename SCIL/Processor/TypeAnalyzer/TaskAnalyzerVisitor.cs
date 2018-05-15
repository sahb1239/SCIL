using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SCIL.Processor.Nodes;
using SCIL.Processor.Nodes.Visitor;
using Type = SCIL.Processor.Nodes.Type;

namespace SCIL.Processor.TypeAnalyzer
{
    [RegistrerAnalyzer]
    public class TaskAnalyzerVisitor : BaseVisitor
    {
        public override void Visit(Module module)
        {
            base.Visit(module);
        }

        public override void Visit(Type type)
        {
            if (type.Definition.Interfaces.Any(implementedInterface =>
                implementedInterface.InterfaceType.FullName == "System.Runtime.CompilerServices.IAsyncStateMachine"))
            {
                // Find all initilization points
                type.IsGeneratedTaskType = true;
            }

            base.Visit(type);
        }

        public override void Visit(Method method)
        {
            base.Visit(method);
            taskMethodReferencesStack.Clear();
            taskMethodReferencesVariables.Clear();
        }

        private IDictionary<string, MethodReference> taskMethodReferencesStack = new Dictionary<string, MethodReference>();
        private IDictionary<string, MethodReference> taskMethodReferencesVariables = new Dictionary<string, MethodReference>();

        public override void Visit(Node node)
        {
            base.Visit(node);

            if (node.OpCode.Code == Code.Call)
            {
                if (node.Operand is MethodReference methodCall)
                {
                    if (methodCall.ReturnType.FullName.StartsWith("System.Threading.Tasks.Task") && 
                        methodCall.ReturnType is GenericInstanceType genericReturnType && 
                        genericReturnType.HasGenericArguments)
                    {
                        // Set push stack to this method
                        taskMethodReferencesStack.Add(node.PushStackNames.Single(), methodCall);

                        // Set task method
                        node.TaskMethod = methodCall;
                    }
                }
            }

            // Pop from stack and push to stack/to variables
            var set = taskMethodReferencesStack.SingleOrDefault(task => node.PopStackNames.Contains(task.Key)).Value;
            if (set != null)
            {
                // Update node
                node.TaskMethod = set;

                // Handle all pushes (just add to all)
                foreach (var push in node.PushStackNames)
                {
                    taskMethodReferencesStack.Add(push, set);
                }

                // Set variables
                if (node.VariableName != null)
                {
                    // Detect if it's a set variable instruction
                    if (node.GetRequiredVariableIndex().set)
                    {
                        taskMethodReferencesVariables.Add(node.VariableName, set);
                    }
                }
            }

            // Load variable
            if (node.VariableName != null)
            {
                // Get variable method
                var variableMethod = taskMethodReferencesVariables
                    .SingleOrDefault(variable => variable.Key == node.VariableName).Value;
                
                if (variableMethod != null)
                {
                    // Update node
                    node.TaskMethod = variableMethod;

                    // Add method to all stack pushes
                    if (node.GetRequiredVariableIndex().set == false)
                    {
                        foreach (var push in node.PushStackNames)
                        {
                            taskMethodReferencesStack.Add(push, variableMethod);
                        }
                    }
                }
            }

            // Handle phi var node
            if (node is PhiVariableNode phiVariableNode)
            {
                var variableMethod = taskMethodReferencesVariables
                    .Where(variable => phiVariableNode.Parents.Any(parent => parent.VariableName == variable.Key))
                    .Select(e => e.Value).Distinct().SingleOrDefault();

                if (variableMethod != null)
                {
                    node.TaskMethod = variableMethod;

                    taskMethodReferencesVariables.Add(node.VariableName, variableMethod);
                }
            }

            // Handle phi stack node
            if (node is PhiStackNode phiStackNode)
            {
                var stackMethod = taskMethodReferencesStack
                    .Where(stack => phiStackNode.Parents.Any(parent =>
                        parent.PushStackNames.Any(parentStackName => parentStackName == stack.Key)))
                    .Select(e => e.Value).Distinct().SingleOrDefault();

                if (stackMethod != null)
                {
                    node.TaskMethod = stackMethod;


                    // Handle all pushes (just add to all)
                    foreach (var push in node.PushStackNames)
                    {
                        taskMethodReferencesStack.Add(push, stackMethod);
                    }
                }
            }
        }
    }
}
