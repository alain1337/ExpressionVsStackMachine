using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace ExpressionVsStackMachine
{
    public class Roslyn
    {
        readonly string _code;
        readonly Script<int> _script;
        readonly ScriptRunner<int> _runner;

        public Roslyn(string code)
        {
            _code = code;
            _script = CSharpScript.Create<int>(code, ScriptOptions.Default);
            _runner = _script.CreateDelegate();
        }

        public int Call()
        {
            return _runner.Invoke().Result;
        }
    }
}
