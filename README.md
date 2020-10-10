# ExpressionVsStackMachine

Simple test to compare performance of a stack machine vs. .NET Expression trees

```
Simple
        StackMachine                  :      20 ms
        Expression                    :      48 ms

Complex
        StackMachine                  :     322 ms
        Expression                    :      50 ms
        Lambda                        :       2 ms
        Roslyn Scripting              :      36 ms
```
