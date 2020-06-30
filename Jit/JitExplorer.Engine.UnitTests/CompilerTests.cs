using System;
using Xunit;

using JitExplorer.Engine.Compile;

namespace JitExplorer.Engine.UnitTests
{
    public class CompilerTests
    {
        [Fact]
        public void Test1()
        {
            var options = new CompilerOptions() { OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication };
            Compiler c = new Compiler(options);

            string source = "public class Program { public static void Main() {} }";

            var exec = c.Compile("test.exe", "foo.cs", source);
        }
    }
}
