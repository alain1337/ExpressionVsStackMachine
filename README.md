# ExpressionVsStackMachine

Simple test to compare performance of a stack machine vs. .NET Expression trees

```
Simple
        StackMachine                  :      20 ms
        Expression                    :      49 ms
        Lambda                        :       0 ms
        Roslyn Scripting              :      31 ms

Complex
        StackMachine                  :     325 ms
        Expression                    :      50 ms
        Lambda                        :       2 ms
        Roslyn Scripting              :      34 ms
```
