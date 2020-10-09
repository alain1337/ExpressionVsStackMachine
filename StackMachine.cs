using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressionVsStackMachine
{
    public static class StackMachine
    {
        public static int? Execute(StackProgram program, bool trace = false)
        {
            var stack = new Stack<int>();
            var retStack = new Stack<int>();

            var ip = 0;
            while (ip < program.Instructions.Length)
            {
                if (trace)
                {
                    Console.WriteLine(new string('-', 70));
                    for (var i = 0; i < program.Instructions.Length; i++)
                        Console.WriteLine($"{(i == ip ? " > " : "   ")} {program.Instructions[i]}");
                    Console.WriteLine($"Stack:        [{String.Join(",", stack.Reverse())}]");
                    Console.WriteLine($"Return stack: [{String.Join(",", retStack.Reverse())}]");
                    Console.WriteLine("Press [Space], [q] or [g]");
                    var key = Console.ReadKey();
                    if (key.KeyChar == 'q')
                        return null;
                    else if (key.KeyChar == 'g')
                        trace = false;
                    Console.WriteLine();
                }

                var instruction = program.Instructions[ip++];
                int a, b, c;
                switch (instruction.OpCode)
                {
                    case StackOpCode.Value:
                        stack.Push(instruction.Value);
                        break;
                    case StackOpCode.Label:
                        // just skip over
                        break;
                    case StackOpCode.Jmp:
                        ip = instruction.Value;
                        break;
                    case StackOpCode.Call:
                        retStack.Push(ip);
                        ip = instruction.Value;
                        break;
                    case StackOpCode.Ret:
                        ip = retStack.Pop();
                        break;
                    case StackOpCode.Cmp:
                        stack.Push(stack.Pop().CompareTo(stack.Pop()));
                        break;
                    case StackOpCode.Je:
                    case StackOpCode.Jz:
                        if (stack.Pop() == 0)
                            ip = instruction.Value;
                        break;
                    case StackOpCode.Jne:
                    case StackOpCode.Jnz:
                        if (stack.Pop() != 0)
                            ip = instruction.Value;
                        break;
                    case StackOpCode.Jl:
                        if (stack.Pop() < 0)
                            ip = instruction.Value;
                        break;
                    case StackOpCode.Jg:
                        if (stack.Pop() > 0)
                            ip = instruction.Value;
                        break;
                    case StackOpCode.Plus:
                        stack.Push(stack.Pop() + stack.Pop());
                        break;
                    case StackOpCode.Minus:
                        a = stack.Pop();
                        b = stack.Pop();
                        stack.Push(b - a);
                        break;
                    case StackOpCode.Mul:
                        stack.Push(stack.Pop() * stack.Pop());
                        break;
                    case StackOpCode.Div:
                        a = stack.Pop();
                        b = stack.Pop();
                        stack.Push(b / a);
                        break;
                    case StackOpCode.Mod:
                        a = stack.Pop();
                        b = stack.Pop();
                        stack.Push(b % a);
                        break;
                    case StackOpCode.Dup:
                        stack.Push(stack.Peek());
                        break;
                    case StackOpCode.Drop:
                        stack.Pop();
                        break;
                    case StackOpCode.Swap:
                        a = stack.Pop();
                        b = stack.Pop();
                        stack.Push(a);
                        stack.Push(b);
                        break;
                    case StackOpCode.Rot:
                        a = stack.Pop();
                        b = stack.Pop();
                        c = stack.Pop();
                        stack.Push(b);
                        stack.Push(a);
                        stack.Push(c);
                        break;
                    case StackOpCode.Over:
                        a = stack.Pop();
                        b = stack.Pop();
                        stack.Push(a);
                        stack.Push(b);
                        stack.Push(a);
                        break;
                    case StackOpCode.Print:
                        Console.WriteLine(stack.Pop());
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("OpCode not implemented: " + instruction.OpCode);
                }
            }

            return stack.Count > 0 ? stack.Pop() : (int?)null;
        }

        static int FindLabel(StackInstruction[] instructions, string label)
        {
            for (var i=0; i < instructions.Length; i++)
                if (instructions[i].OpCode == StackOpCode.Label && String.Equals(label, instructions[i].Operand))
                    return i;
            throw new Exception("Label not found: " + label);
        }

        public static StackProgram Compile(IEnumerable<string> program)
        {
            var instructions = program.Select(s => new StackInstruction(s)).ToArray();

            foreach (var i in instructions.Where(si => si.OpCode.IsJump()))
                i.Value = FindLabel(instructions, i.Operand);

            return new StackProgram
            {
                Instructions = instructions.ToArray()
            };
        }
    }

    public class StackProgram
    {
        public StackInstruction[] Instructions;
    }

    public class StackInstruction
    {
        public StackOpCode OpCode { get; }
        public int Value { get; internal set; }
        public string Operand { get; }

        public StackInstruction(StackOpCode opCode, string operand, int value)
        {
            Value = value;
            OpCode = opCode;
            Operand = operand;
        }

        public StackInstruction(string instruction)
        {
            if (int.TryParse(instruction, out var iv))
            {
                OpCode = StackOpCode.Value;
                Value = iv;
            }
            else if (instruction[0] == ':')
            {
                OpCode = StackOpCode.Label;
                Operand = instruction;
            }
            else
            {
                var tokens = instruction.Split();
                if (Enum.TryParse<StackOpCode>(tokens[0], true, out var ov))
                {
                    OpCode = ov;
                    Operand = tokens.Length > 1 ? tokens[1] : null;
                }
                else if (Aliases.TryGetValue(tokens[0], out ov))
                {
                    OpCode = ov;
                    Operand = tokens.Length > 1 ? tokens[1] : null;
                }
                else
                    throw new Exception("Unknown instruction: " + instruction);
            }
        }

        public override string ToString()
        {
            return OpCode == StackOpCode.Value ? Value.ToString() : OpCode + (Operand != null ? " " + Operand : "");
        }

        static readonly Dictionary<string, StackOpCode> Aliases = new Dictionary<string, StackOpCode>
        {
            ["+"] = StackOpCode.Plus,
            ["-"] = StackOpCode.Minus,
            ["*"] = StackOpCode.Mul,
            ["/"] = StackOpCode.Div,
            ["%"] = StackOpCode.Mod
        };
    }

    public enum StackOpCode
    {
        Value,
        Label,
        Jmp,
        Call,
        Ret,
        Cmp,
        Je,
        Jne,
        Jz,
        Jnz,
        Jl,
        Jg,
        Plus,
        Minus,
        Mul,
        Div,
        Mod,
        Dup,
        Drop,
        Swap,
        Rot,
        Over,
        Print
    }

    public static class StackOpCodeExtensions
    {
        public static bool IsJump(this StackOpCode code)
        {
            switch (code)
            {
                case StackOpCode.Jmp:
                case StackOpCode.Call:
                case StackOpCode.Je:
                case StackOpCode.Jne:
                case StackOpCode.Jz:
                case StackOpCode.Jnz:
                case StackOpCode.Jl:
                case StackOpCode.Jg:
                    return true;
                default:
                    return false;
            }
        }
    }
}
