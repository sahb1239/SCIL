using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SCIL.Processor.Nodes;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL
{
    public class Node : Element
    {
        private readonly List<string> _popStackNames = new List<string>();
        private readonly List<string> _pushStackNames = new List<string>();

        public Node(Block block)
        {
            Block = block ?? throw new ArgumentNullException(nameof(block));
        }

        public Node(Instruction instruction, Block block)
        {
            Instruction = instruction ?? throw new ArgumentNullException(nameof(instruction));
            Block = block ?? throw new ArgumentNullException(nameof(block));
        }

        public Instruction Instruction { get; }
        public Block Block { get; set; }

        public OpCode OpCode => OverrideOpCode ?? Instruction.OpCode;
        public OpCode? OverrideOpCode { get; set; }

        public object Operand => OverrideOperand ?? Instruction.Operand;
        public object OverrideOperand { get; set; }


        public void Replace(params Node[] nodes)
        {
            this.Block.ReplaceNode(this, nodes);
        }

        public int PopCountFromStack
        {
            get
            {
                int pop;
                switch (OpCode.StackBehaviourPop)
                {
                    case StackBehaviour.Pop0:
                        pop = 0;
                        break;
                    case StackBehaviour.Pop1:
                    case StackBehaviour.Popi:
                        pop = 1;
                        break;
                    case StackBehaviour.Popref:
                        pop = 1;
                        break;
                    case StackBehaviour.Pop1_pop1:
                    case StackBehaviour.Popi_pop1:
                    case StackBehaviour.Popi_popi:
                    case StackBehaviour.Popi_popr4:
                    case StackBehaviour.Popi_popr8:
                        pop = 2;
                        break;
                    case StackBehaviour.Popref_popi_popi:
                    case StackBehaviour.Popref_popi_popr4:
                    case StackBehaviour.Popref_popi_popr8:
                    case StackBehaviour.Popref_popi_popi8:
                    case StackBehaviour.Popref_popi_popref:
                        pop = 3;
                        break;
                    case StackBehaviour.Varpop: // Pop from variables
                        pop = 0;
                        break;
                    case StackBehaviour.PopAll:
                        pop = 0;
                        break;
                    case StackBehaviour.Popref_pop1:
                        pop = 2;
                        break;
                    default:
                        throw new NotImplementedException($"StackBehaviour on pop {OpCode.StackBehaviourPop} not implemented");
                }
                
                // Detect method calling
                switch (OpCode.Code)
                {
                    case Code.Call:
                    case Code.Calli:
                    case Code.Callvirt:
                        // Detect the required numbers of parameters to pop
                        var method = (MethodReference)Operand;
                        var parameters = method.Parameters.Count;

                        // Detect this
                        if (method.HasThis)
                        {
                            parameters++;
                        }

                        pop = parameters;

                        break;
                }

                // Detect return
                if (OpCode.Code == Code.Ret && this.Block.Method.Definition.ReturnType.FullName != "System.Void")
                {
                    pop = 1;
                }

                return pop;
            }
        }

        public int PushCountFromStack
        {
            get
            {
                int push;
                switch (OpCode.StackBehaviourPush)
                {
                    case StackBehaviour.Push0:
                        push = 0;
                        break;
                    case StackBehaviour.Push1:
                    case StackBehaviour.Pushi:
                    case StackBehaviour.Pushi8:
                    case StackBehaviour.Pushr4:
                    case StackBehaviour.Pushr8:
                    case StackBehaviour.Pushref:
                        push = 1;
                        break;
                    case StackBehaviour.Push1_push1:
                        push = 2;
                        break;
                    case StackBehaviour.Varpush:
                        // Push to variable
                        push = 0;
                        break;
                    default:
                        throw new NotImplementedException($"StackBehaviour on push {OpCode.StackBehaviourPush} not implemented");
                }


                // Detect method calling
                switch (OpCode.Code)
                {
                    case Code.Call:
                    case Code.Calli:
                    case Code.Callvirt:
                        // Get method
                        var method = (MethodReference)Operand;

                        // Detect the required arguments to push
                        if (method.ReturnType.FullName == "System.Void")
                        {
                            push = 0;
                        }
                        else
                        {
                            push = 1;
                        }

                        break;
                }

                // Detect exception handling
                if (this.Block.Method.Definition.Body.ExceptionHandlers.Any(e => e.HandlerStart == Instruction))
                {
                    // We recieve the exception in start of exception
                    push += 1;
                }

                return push;
            }
        }

        public (int popNames, int pushNames) GetRequiredNames()
        {
            return (PopCountFromStack, PushCountFromStack);
        }

        private int GetVariableIndex(object operand)
        {
            if (operand is VariableDefinition variableDefinition)
                return variableDefinition.Index;
            if (operand is sbyte index)
                return index;

            throw new NotImplementedException();
        }

        public (bool variableInstruction, int index, bool set) GetRequiredVariableIndex()
        {
            /*
             * if (Operand is VariableDefinition variableDefinition)
                {
                    return variableDefinition.Index.ToString();
                }
                return ((sbyte) Operand).ToString();
             */

            switch (OpCode.Code)
            {
                case Code.Ldloc:
                case Code.Ldloc_S:
                case Code.Ldloca:
                case Code.Ldloca_S:
                    return (true, GetVariableIndex(Operand), false);
                case Code.Stloc:
                case Code.Stloc_S:
                    return (true, GetVariableIndex(Operand), true);
                case Code.Ldloc_0:
                case Code.Ldloc_1:
                case Code.Ldloc_2:
                case Code.Ldloc_3:
                    return (true, OpCode.Code - Code.Ldloc_0, false);
                case Code.Stloc_0:
                case Code.Stloc_1:
                case Code.Stloc_2:
                case Code.Stloc_3:
                    return (true, OpCode.Code - Code.Stloc_0, true);
            }

            return (false, -1, false);
        }

        public int? GetRequiredArgumentIndex()
        {
            switch (OpCode.Code)
            {
                case Code.Ldarg:
                case Code.Ldarga:
                case Code.Ldarg_S:
                case Code.Starg:
                case Code.Starg_S:
                    if (Operand is sbyte index)
                    {
                        return index;
                    }

                    throw new NotImplementedException();
                case Code.Ldarg_0:
                case Code.Ldarg_1:
                case Code.Ldarg_2:
                case Code.Ldarg_3:
                    return OpCode.Code - Code.Ldarg_0;
            }

            return null;
        }

        public void SetPopStackNames(params string[] names)
        {
            var required = GetRequiredNames();
            if (required.popNames != names.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(names), $"{required.popNames} names is required");
            }
            _popStackNames.Clear();
            _popStackNames.AddRange(names);
        }

        public void SetPushStackNames(params string[] names)
        {
            var required = GetRequiredNames();
            if (required.pushNames != names.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(names), $"{required.pushNames} names is required");
            }
            _pushStackNames.Clear();
            _pushStackNames.AddRange(names);
        }

        public IReadOnlyCollection<string> PopStackNames => _popStackNames.AsReadOnly();
        public IReadOnlyCollection<string> PushStackNames => _pushStackNames.AsReadOnly();
        public string ArgumentName { get; set; }

        public FieldReference FieldReference => Operand as FieldReference;
        public string FieldName => FieldReference.FullName;

        public string VariableName { get; set; }

        public override string ToString()
        {
            return Instruction.ToString();
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}