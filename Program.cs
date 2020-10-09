using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace ExpressionVsStackMachine
{
    internal static class Program
    {
        static void Main()
        {
            // 1+2*3=7

            const int loops = 100_000;
            var program = StackMachine.Compile(new[] { "1", "2", "3", "*", "+" });
            Test("StackMachine (Simple)", () =>
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

            var lambda = Expression.Lambda(block).Compile();
            Test("Expression (Simple)", () =>
            {
                for (var i = 0; i < loops; i++)
                    if ((int)lambda.DynamicInvoke() != 7)
                        throw new Exception("Oops");
            });


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

            Test("StackMachine (Complex)", () =>
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
            lambda = Expression.Lambda(block).Compile();
            Test("Expression (Complex)", () =>
            {
                for (var i = 0; i < loops; i++)
                {
                    if ((int)lambda.DynamicInvoke() != fac10)
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

            Test("Lambda (Complex)", () =>
            {
                for (var i = 0; i < loops; i++)
                {
                    if (Fac() != fac10)
                        throw new Exception("Oops");
                }
            });
        }

        static void Test(string caption, Action action)
        {
            Console.Write($"{caption,-30}: ");
            var sw = Stopwatch.StartNew();
            action();
            Console.WriteLine($"{sw.ElapsedMilliseconds + " ms",10}");
        }
    }
}
