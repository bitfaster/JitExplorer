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
            Compiler c = new Compiler();

            string source = "public class Test { public int i; }";

            var exec = c.Compile("foo.cs", source, true);
        }
    }
}
