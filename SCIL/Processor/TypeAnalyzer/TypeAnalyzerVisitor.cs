using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SCIL.Processor.Extentions;
using SCIL.Processor.Nodes;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.TypeAnalyzer
{
    [RegistrerAnalyzer]
    public class TypeAnalyzerVisitor : BaseVisitor
    {
        private bool _changed = false;

        private readonly IDictionary<int, ArgumentInformation> _argumentInformations =
            new Dictionary<int, ArgumentInformation>();

        private readonly IDictionary<int, VariableInformation> _variableDefinitions =
            new Dictionary<int, VariableInformation>();

        private readonly IDictionary<Node, List<StackPushInformation>> _stackTransactions =
            new Dictionary<Node, List<StackPushInformation>>();

        public override void Visit(Method method)
        {
            // Clear lists
            _argumentInformations.Clear();
            _variableDefinitions.Clear();
            _stackTransactions.Clear();

            // Read arguments
            foreach (var parameter in method.Definition.Parameters)
            {
                var parameterType = GetGenericType(method.Definition, parameter.ParameterType);

                _argumentInformations[parameter.Index] = new ArgumentInformation(parameter.Index, parameter, parameterType);
            }

            // Read variables
            if (method.Definition.Body?.Variables != null)
            {
                foreach (var variable in method.Definition.Body?.Variables)
                {
                    var variableType = GetGenericType(method.Definition, variable.VariableType);
                    
                    _variableDefinitions[variable.Index] =
                        new VariableInformation(variable.Index, variable, variableType);
                }
            }

            // Run analyzer
            do
            {
                _changed = false;

                base.Visit(method);
            } while (_changed);

            // Add stack push information to nodes and to the method
            foreach (var nodePushInformations in _stackTransactions)
            {
                foreach (var push in nodePushInformations.Value)
                {
                    nodePushInformations.Key.AddElement(push);

                    method.AddElement(push);
                }
            }

            // Add variable info to method
            foreach (var variableInfo in _variableDefinitions)
            {
                method.AddElement(variableInfo.Value);
            }

            // Add argument information to method
            foreach (var argumentInfo in _argumentInformations)
            {
                method.AddElement(argumentInfo.Value);
            }
        }

        public override void Visit(Node node)
        {
            // Add and get list with stack transactions
            if (!_stackTransactions.ContainsKey(node))
            { 
                _stackTransactions.Add(node, new List<StackPushInformation>());
            }
            List<StackPushInformation> stackTransactions = _stackTransactions[node];

            // If there are already tranactions registrered we just skip the node
            if (stackTransactions.Any())
            {
                return;
            }
            
            // Switch on opcode
            switch (node.OpCode.Code)
            {
                case Code.Newobj:
                    _changed |= NewObjTransactions(node, stackTransactions);
                    break;
                case Code.Call:
                case Code.Calli:
                case Code.Callvirt:
                    _changed |= CallTransactions(node, stackTransactions);
                    break;
                case Code.Ldarg:
                case Code.Ldarga:
                    _changed |= LdArgTransactions(node, stackTransactions);
                    break;
                // Load field
                case Code.Ldfld:
                case Code.Ldflda:
                // Load static field
                case Code.Ldsfld:
                case Code.Ldsflda:
                    _changed |= LdFldTransactions(node, stackTransactions);
                    break;
                case Code.Ldloc:
                case Code.Ldloca:
                    _changed |= LdLocTransactions(node, stackTransactions);
                    break;
                case Code.Stloc:
                case Code.Stloc_S:
                    // Get variable index
                    int variableIndex = node.OpCode.GetVariableIndex(node.Operand);

                    // Get argument
                    var variable = _variableDefinitions[variableIndex];

                    // Get pop
                    var stLocPopName = node.PopStackNames.Single();

                    // Find stack tranaction
                    var stTrans = _stackTransactions.SelectMany(e => e.Value).SingleOrDefault(e => e.StackName == stLocPopName);
                    if (stTrans == null)
                    {
                        variable.AddStackInformation(stLocPopName);
                        break;
                    }
                    
                    // Add transaction to index
                    variable.AddStackInformation(stLocPopName, stTrans);

                    break;
                case Code.Ldc_I8:
                    stackTransactions.Add(StackPushInformation.CreateFromConstant(node.PushStackNames.Single(), (long)node.Operand));

                    // Set changed
                    _changed = true;
                    break;
                case Code.Ldc_R8:
                    stackTransactions.Add(StackPushInformation.CreateFromFloatingPointConstant(node.PushStackNames.Single(), (double)node.Operand));

                    // Set changed
                    _changed = true;
                    break;
                case Code.Ldnull:
                    stackTransactions.Add(StackPushInformation.CreateFromNull(node.PushStackNames.Single()));

                    // Set changed
                    _changed = true;
                    break;
                case Code.Ldstr:
                    stackTransactions.Add(StackPushInformation.CreateFromString(node.PushStackNames.Single(), node.Operand as string));

                    // Set changed
                    _changed = true;
                    break;
                case Code.Neg:
                case Code.Not:
                case Code.Dup:
                    // Get pop instruction
                    var popName = node.PopStackNames.Single();
                    var popNode = node.Block.Method.Blocks.SelectMany(e => e.Nodes)
                        .Distinct().SingleOrDefault(n => n.PushStackNames.Any(e => e == popName));

                    // If we cannot find it - we wait
                    if (popNode == null)
                    {
                        break;
                    }

                    // Get transaction for this node
                    if (!_stackTransactions.ContainsKey(popNode))
                    {
                        break;
                    }

                    // Get sources (there should only be one source)
                    var source = _stackTransactions[popNode].SingleOrDefault(transaction => transaction.StackName == popName);
                    if (source == null)
                    {
                        break;
                    }

                    // Get pushName
                    var pushNames = node.PushStackNames.OrderBy(e => e);
                    foreach (var pushName in pushNames)
                    {
                        // Add transaction
                        stackTransactions.Add(StackPushInformation.CreateCopy(pushName, source));
                    }

                    // Set changed
                    _changed = true;
                    break;
                case Code.Castclass:
                    // TODO: Easy to implement
                    break;
                case Code.Isinst:
                    // TODO:
                    break;
                case Code.Ldtoken:
                case Code.Ldftn:
                case Code.Ldvirtftn:
                    break;
                case Code.Ceq:
                case Code.Cgt:
                case Code.Cgt_Un:
                case Code.Clt:
                case Code.Clt_Un:
                    // TODO:
                    break;
                case Code.Add:
                case Code.Add_Ovf:
                case Code.Add_Ovf_Un:
                case Code.Sub:
                case Code.Sub_Ovf:
                case Code.Sub_Ovf_Un:
                case Code.Mul:
                case Code.Mul_Ovf:
                case Code.Mul_Ovf_Un:
                case Code.Div:
                case Code.Div_Un:
                case Code.Rem:
                case Code.Rem_Un:
                    // TODO:
                    break;
                case Code.And:
                case Code.Or:
                case Code.Xor:
                    // TODO: 
                    break;
                case Code.Box:
                case Code.Unbox:
                case Code.Unbox_Any:
                    // TODO:
                    break;
                case Code.Conv_I1:
                case Code.Conv_I2:
                case Code.Conv_I4:
                case Code.Conv_I8:
                    // TODO:
                    break;
                case Code.Conv_R4:
                case Code.Conv_R8:
                case Code.Conv_R_Un:
                    // TODO:
                    break;
                case Code.Sizeof:
                    // TODO:
                    break;
                case Code.Ldobj:
                    // TODO:
                    break;
                case Code.Newarr:
                case Code.Ldelem_Any:
                case Code.Ldelem_I1:
                case Code.Ldelem_I2:
                case Code.Ldelem_I4:
                case Code.Ldelem_I8:
                case Code.Ldelem_R4:
                case Code.Ldelem_R8:
                case Code.Ldelem_Ref:
                case Code.Ldlen:
                    break;
                default:
                    // We need to handle all nodes which pushes to the stack
                    Debug.Assert(node.PushCountFromStack == 0);
                    break;
            }

            base.Visit(node);
        }

        private bool LdLocTransactions(Node node, List<StackPushInformation> stackTransactions)
        {
            // Get variable index
            int variableIndex = node.OpCode.GetVariableIndex(node.Operand);

            // Get argument
            var variable = _variableDefinitions[variableIndex];
            bool isReference = node.OpCode.Code == Code.Ldloca || node.OpCode.Code == Code.Ldloca_S;

            stackTransactions.Add(StackPushInformation.CreateFromVariable(node.PushStackNames.Single(), variable, isReference));
            return true;
        }

        private bool LdFldTransactions(Node node, List<StackPushInformation> stackTransactions)
        {
            // Get field index
            FieldDefinition field = node.OpCode.GetField(node.Operand).Resolve();
            bool isReference = node.OpCode.Code == Code.Ldflda || node.OpCode.Code == Code.Ldsflda;

            stackTransactions.Add(StackPushInformation.CreateFromField(node.PushStackNames.Single(), field, isReference));
            return true;
        }

        private bool LdArgTransactions(Node node, IList<StackPushInformation> stackTransactions)
        {
            // Get argument index
            int argumentIndex = node.OpCode.GetArgumentIndex(node.Operand);

            if (node.Block.Method.Definition.HasThis)
            {
                // Handle this argument
                if (argumentIndex == 0)
                {
                    stackTransactions.Add(
                        StackPushInformation.CreateFromThisArgument(node.PushStackNames.Single(), node.Block.Method.Definition.DeclaringType));
                    return true;
                }

                // Decrement with 1 to get correct argument
                argumentIndex--;
            }

            // Get argument
            ArgumentInformation argument = _argumentInformations[argumentIndex];
            bool isReference = node.OpCode.Code == Code.Ldarga || node.OpCode.Code == Code.Ldarga_S;

            stackTransactions.Add(StackPushInformation.CreateFromArgument(node.PushStackNames.Single(), argument, isReference));
            return true;
        }

        private bool NewObjTransactions(Node node, IList<StackPushInformation> stackTransactions)
        {
            if (node.Operand is MethodReference constructor)
            {
                // Resolve the new initilized type
                var type = constructor.DeclaringType.Resolve();
                stackTransactions.Add(StackPushInformation.CreateFromNewObj(node.PushStackNames.Single(), type));

                return true;
            }

            // The operand should be a MethodReference - if not - we currently don't handle it
            throw new NotSupportedException();
        }

        // Get return type helper
        TypeReference GetGenericType(MethodReference method, TypeReference typeReference)
        {
            if (typeReference.Resolve() != null)
            {
                return typeReference;
            }
            else if (typeReference.IsGenericParameter)
            {
                // Get generic parameter
                var genericParameter = (GenericParameter)typeReference;

                // Check if method is a generic instance method
                if (method is GenericInstanceMethod genericInstanceMethod)
                {
                    // Get parameter
                    TypeReference parameterFromMethod =
                        genericInstanceMethod.GenericArguments[genericParameter.Position];

                    return parameterFromMethod;
                }
                // Check if type is a generic type
                else if (method.DeclaringType is GenericInstanceType genericInstanceType)
                {
                    // Get parameter
                    TypeReference parameterFromMethod =
                        genericInstanceType.GenericArguments[genericParameter.Position];

                    return parameterFromMethod;
                }
                else if (method.GenericParameters.Any(methodGenericParameter => methodGenericParameter == genericParameter))
                {
                    // Method has generic parameters
                    foreach (var methodGenericParameter in method.GenericParameters)
                    {
                        if (methodGenericParameter == genericParameter)
                        {
                            return genericParameter;
                        }
                    }

                    throw new NotSupportedException();
                }
                else if (method.DeclaringType.GenericParameters.Any(declaringTypeGenericParameter => declaringTypeGenericParameter == genericParameter))
                {
                    // Method has generic parameters
                    foreach (var declaringTypeGenericParameter in method.DeclaringType.GenericParameters)
                    {
                        if (declaringTypeGenericParameter == genericParameter)
                        {
                            return genericParameter;
                        }
                    }

                    throw new NotSupportedException();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else if (typeReference.IsArray)
            {
                if (typeReference is ArrayType arrayType)
                {
                    // Get elementType
                    var elementType = GetGenericType(method, arrayType.ElementType);
                    return new ArrayType(elementType); // Let's hope this work
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                if (typeReference.HasGenericParameters)
                {
                    throw new NotSupportedException();
                }

                return typeReference;
            }
        }

        private bool CallTransactions(Node node, IList<StackPushInformation> stackTransactions)
        {
            if (node.Operand is MethodReference method)
            {
                // Resolve the result type and detect if it's a void method
                var returnType = GetGenericType(method, method.ReturnType);
                if (returnType.IsVoid())
                {
                    return false;
                }

                // Get method definition
                var methodDefinition = method.Resolve();
                stackTransactions.Add(StackPushInformation.CreateFromMethodCall(node.PushStackNames.Single(), methodDefinition));

                return true;
            }

            // The operand should be a MethodReference - if not - we currently don't handle it
            throw new NotSupportedException();
        }
    }

    public class StackPushInformation
    {
        private StackPushInformation(string stackName)
        {
            StackName = stackName;
        }

        public static StackPushInformation CreateCopy(string stackName, StackPushInformation pushInformation)
        {
            return new StackPushInformation(stackName) { FromCopy = pushInformation };
        }

        public static StackPushInformation CreateFromThisArgument(string stackName, TypeDefinition thisDefinition)
        {
            return new StackPushInformation(stackName) { FromThis = thisDefinition };
        }

        public static StackPushInformation CreateFromArgument(string stackName, ArgumentInformation argument, bool isReference = false)
        {
            return new StackPushInformation(stackName) { FromArgument = argument, IsReference = isReference };
        }

        public static StackPushInformation CreateFromVariable(string stackName, VariableInformation variable, bool isReference = false)
        {
            return new StackPushInformation(stackName) { FromVariable = variable, IsReference = isReference };
        }

        public static StackPushInformation CreateFromStack(string stackName, StackPushInformation stack)
        {
            return new StackPushInformation(stackName) { FromStack = stack, IsReference = stack.IsReference };
        }

        public static StackPushInformation CreateFromNewObj(string stackName, TypeDefinition type)
        {
            return new StackPushInformation(stackName) { FromNewObj = type };
        }

        public static StackPushInformation CreateFromMethodCall(string stackName, MethodDefinition method)
        {
            return new StackPushInformation(stackName) { FromMethodCall = method };
        }

        public static StackPushInformation CreateFromField(string stackName, FieldDefinition field, bool isReference = false)
        {
            return new StackPushInformation(stackName) { FromField = field, IsReference = isReference };
        }
        
        public static StackPushInformation CreateFromConstant(string stackName, long constant)
        {
            return new StackPushInformation(stackName) { FromLongConstant = constant};
        }

        public static StackPushInformation CreateFromFloatingPointConstant(string stackName, double constant)
        {
            return new StackPushInformation(stackName) { FromDoubleConstant = constant };
        }

        public static StackPushInformation CreateFromNull(string stackName)
        {
            return new StackPushInformation(stackName) { FromNull = true};
        }

        public static StackPushInformation CreateFromString(string stackName, string str)
        {
            return new StackPushInformation(stackName) { FromString = str };
        }

        public FieldDefinition FromField { get; private set; }
        public TypeDefinition FromThis { get; private set; }
        public ArgumentInformation FromArgument { get; private set; }
        public VariableInformation FromVariable { get; private set; }
        public StackPushInformation FromStack { get; private set; }
        public TypeDefinition FromNewObj { get; private set; }
        public MethodDefinition FromMethodCall { get; private set; }
        public long? FromLongConstant { get; private set; }
        public double? FromDoubleConstant { get; private set; }
        public bool FromNull { get; private set; }
        public string FromString { get; private set; }
        public StackPushInformation FromCopy { get; private set; }

        public bool IsReference { get; private set; }

        public bool IsSpecificType
        {
            get { return FromNewObj != null || FromThis != null || (FromVariable?.IsSpecificType ?? false) || (FromCopy?.IsSpecificType ?? false); }
        }

        public TypeReference Type => GetTypeReference();

        private TypeReference GetTypeReference()
        {
            if (FromField != null)
            {
                return FromField.DeclaringType;
            }
            else if (FromThis != null)
            {
                return FromThis;
            }
            else if (FromArgument != null)
            {
                return FromArgument.Type;
            }
            else if (FromVariable != null)
            {
                return FromVariable.Type;
            }
            else if (FromStack != null)
            {
                return FromStack.Type;
            }
            else if (FromNewObj != null)
            {
                return FromNewObj;
            }
            else if (FromMethodCall != null)
            {
                return FromMethodCall.ReturnType;
            }
            else if (FromCopy != null)
            {
                return FromCopy.Type;
            }
            else
            {
                Debug.Assert(false);
                return null;
            }
        }

        public string StackName { get; private set; }
    }

    public class VariableInformation
    {
        private readonly List<StackPushInformation> _fromStackInformation = new List<StackPushInformation>();

        public VariableInformation(int variableIndex, VariableDefinition definition, TypeReference type)
        {
            VariableIndex = variableIndex;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        public VariableDefinition Definition { get; }
        public int VariableIndex { get; }
        public TypeReference Type { get; }

        public IEnumerable<TypeReference> GetConcreteTypes
        {
            get
            {
                if (IsSpecificType)
                {
                    return new[] {FromStackInformation.First().Type};
                }
                else
                {
                    return FromStackInformation.Select(s => s.Type).Concat(new[] {Type}).Distinct();
                }
            }
        }

        public bool IsSpecificType
        {
            get
            {
                if (NotAssignedStackInformation.Any())
                    return false;
                if (!FromStackInformation.Any())
                    return false;

                return FromStackInformation.All(s => s.IsSpecificType);
            }
        }

        public IEnumerable<StackPushInformation> FromStackInformation => _fromStackInformation.AsReadOnly();
       
        private List<string> NotAssignedStackInformation = new List<string>();

        public void AddStackInformation(string stLocPopName)
        {
            NotAssignedStackInformation.Add(stLocPopName);
        }

        internal void AddStackInformation(string stLocPopName, StackPushInformation stTrans)
        {
            if (!_fromStackInformation.Any(s => s.StackName == stLocPopName))
            {
                _fromStackInformation.Add(stTrans);
            }

            NotAssignedStackInformation.Remove(stLocPopName);
        }
    }

    public class ArgumentInformation
    {
        public ArgumentInformation(int parameterIndex, ParameterDefinition definition, TypeReference type)
        {
            ParameterIndex = parameterIndex;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        public ParameterDefinition Definition { get; }
        public int ParameterIndex { get; }
        public TypeReference Type { get; }
    }
}
