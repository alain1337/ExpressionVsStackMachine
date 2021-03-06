﻿using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace ExpressionVsStackMachine
{
    internal static class Program
    {
        static void Main()
        {
            Console.WriteLine("Simple");

            // 1+2*3=7

            const int loops = 100_000;
            var program = StackMachine.Compile(new[] { "1", "2", "3", "*", "+" });
            Test("StackMachine", () =>
            {
                for (var i = 0; i < loops; i++)
                    if (StackMachine.Execute(program) != 7)
                        throw new Exception("Oops");
            });

            var block = Expression.Block(
                Expression.Add(
                    Expression.Constant(1),
                    Expression.Multiply(
                        Expression.Constant(2),
                        Expression.Constant(3))));

            var simpleExpression = Expression.Lambda(block).Compile();
            Test("Expression", () =>
            {
                for (var i = 0; i < loops; i++)
                    if ((int)simpleExpression.DynamicInvoke() != 7)
                        throw new Exception("Oops");
            });

            int Simple()
            {
                return 1 + 2 * 3;
            }

            Test("Lambda", () =>
            {
                for (var i = 0; i < loops; i++)
                {
                    if (Simple() != 7)
                        throw new Exception("Oops");
                }
            });

            var simpleRoslyn = new Roslyn("1+2*3");
            Test("Roslyn Scripting", () =>
            {
                for (var i = 0; i < loops; i++)
                {
                    if (simpleRoslyn.Call() != 7)
                        throw new Exception("Oops");
                }
            });
            Console.WriteLine();
            Console.WriteLine("Complex");


            // Fac(10) = 3_628_800
            const int fac10 = 3_628_800;
            program = StackMachine.Compile(new[] {
                "10",
                "1",
                ":loop",
                "swap",
                "dup",
                "1",
                "cmp",
                "je :end",
                "dup",
                "rot",
                "*",
                "swap",
                "1",
                "-",
                "swap",
                "jmp :loop",
                ":end",
                "swap"
            });

            Test("StackMachine", () =>
            {
                for (var i = 0; i < loops; i++)
                    if (StackMachine.Execute(program) != fac10)
                        throw new Exception("Oops");
            });

            var result = Expression.Variable(typeof(int), "result");
            var n = Expression.Variable(typeof(int), "result");
            var end = Expression.Label(typeof(int));
            block = Expression.Block(
                new[] { result, n },
                Expression.Assign(result, Expression.Constant(1)),
                Expression.Assign(n, Expression.Constant(10)),
                Expression.Loop(
                    Expression.Block(
                        Expression.IfThen(
                            Expression.Equal(n, Expression.Constant(1)),
                            Expression.Break(end, result)
                            ),
                        Expression.MultiplyAssign(result, n),
                        Expression.SubtractAssign(n, Expression.Constant(1))
                        )
                    , end)
            );
            simpleExpression = Expression.Lambda(block).Compile();
            Test("Expression", () =>
            {
                for (var i = 0; i < loops; i++)
                {
                    if ((int)simpleExpression.DynamicInvoke() != fac10)
                        throw new Exception("Oops");
                }
            });

            int Fac()
            {
                var r = 1;
                for (var i = 1; i <= 10; i++)
                    r *= i;
                return r;
            }

            Test("Lambda", () =>
            {
                for (var i = 0; i < loops; i++)
                {
                    if (Fac() != fac10)
                        throw new Exception("Oops");
                }
            });

            var roslyn = new Roslyn("var result = 1; for (var i=1; i <= 10; i++) result*=i; return result;");
            Test("Roslyn Scripting", () =>
            {
                for (var i = 0; i < loops; i++)
                {
                    if (roslyn.Call() != fac10)
                        throw new Exception("Oops");
                }
            });

            Console.WriteLine();
        }

        static void Test(string caption, Action action)
        {
            Console.Write($"\t{caption,-30}: ");
            var sw = Stopwatch.StartNew();
            action();
            Console.WriteLine($"{sw.ElapsedMilliseconds + " ms",10}");
        }
    }
}
