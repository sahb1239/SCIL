using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SCIL.Processor.Nodes;
using SCIL.Processor.Nodes.Visitor;
using Type = SCIL.Processor.Nodes.Type;

namespace SCIL.Processor.TypeAnalyzer
{
    [RegistrerVisitor(RegistrerVisitorAttribute.AnalyzerOrder + 1)]
    public class TaskAnalyzerVisitor : BaseVisitor
    {
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

            if (node.OpCode.Code == Code.Call || node.OpCode.Code == Code.Calli || node.OpCode.Code == Code.Callvirt)
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

                        return;
                    }
                }
            }

            // Pop from stack and push to stack/to variables
            Debug.Assert( taskMethodReferencesStack.Count(task => node.PopStackNames.Contains(task.Key)) <= 1);
            var set = taskMethodReferencesStack.FirstOrDefault(task => node.PopStackNames.Contains(task.Key)).Value;
            if (set != null)
            {
                // Update node
                node.TaskMethod = set;

                // Handle all pushes (just add to all)
                foreach (var push in node.PushStackNames)
                {
                    if (taskMethodReferencesStack.ContainsKey(push))
                    {
                        continue;
                    }
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
                Debug.Assert(taskMethodReferencesVariables.Count(variable => variable.Key == node.VariableName) <= 1);

                // Get variable method
                var variableMethod = taskMethodReferencesVariables
                    .FirstOrDefault(variable => variable.Key == node.VariableName).Value;
                
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
                var variableMethods = taskMethodReferencesVariables
                    .Where(variable => phiVariableNode.Parents.Any(parent => parent.VariableName == variable.Key))
                    .Select(e => e.Value).Distinct();
                
                foreach (var variableMethod in variableMethods.ToList())
                {
                    if (variableMethod != null)
                    {
                        node.TaskMethod = variableMethod;

                        taskMethodReferencesVariables.TryAdd(node.VariableName, variableMethod);
                    }
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
